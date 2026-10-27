namespace Tamiron.Domain;

/// <summary>
/// Servis kaydı — ürünün merkezi (VERI-MODELI §2). Tek durum alanı
/// <see cref="DeliveredAt"/>: boşsa araç serviste (K-004).
/// Para kuralları burada, saf: DB'siz test edilir (MIMARI §4.4).
/// </summary>
public class ServiceRecord : TenantEntity
{
    public Guid VehicleId { get; private set; }
    public ServiceType Type { get; private set; }
    public bool TypeSetManually { get; private set; }
    /// <summary>Binlik noktalı ("86.400"); yalnız bakımda istenir (K-002).</summary>
    public string? Km { get; private set; }
    public string Complaint { get; private set; } = "";
    // Açılış günü AYRI ALAN DEĞİL (VERI-MODELI §2: "OpenedAt YOK"): ortak
    // DateTimeCreated'tır. Türkiye saati sabit UTC+3, gün oradan kesin türer.
    public DateOnly? DeliveredAt { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    /// <summary>Veresiye sonradan ödendiyse ödendiği gün (K-071.1). Teslimde ödendiyse boş.</summary>
    public DateOnly? PaymentCollectedAt { get; private set; }
    public DateOnly? ReminderDate { get; set; }
    public bool ReminderSent { get; set; }
    public bool WaitingForPart { get; private set; }
    public DateOnly? WaitingSince { get; private set; }
    public bool ShowWaitingToCustomer { get; private set; }
    /// <summary>KDV tevkifat oranı (ör. 0,7 = 7/10) — yalnız YER (C6, Faz 3). Boş = belirlenmedi. Hesap yok.</summary>
    public decimal? WithholdingRate { get; private set; }
    /// <summary>Müşteri özet linkinin kodu (K-076.3). İlk istendiğinde üretilir, sonra değişmez: gönderilmiş link ölmesin.</summary>
    public string? SummaryCode { get; private set; }

    public string EnsureSummaryCode() => SummaryCode ??= CustomerSummary.NewCode();

    /// <summary>
    /// Yanlış açılan kayıt silinebilir (D13, K-076.12). Teslim edilmiş kayıt SİLİNMEZ: kasada,
    /// hatırlatmada ve müşterinin elindeki linkte izi var — silmek parayı yok etmek olur.
    /// </summary>
    public void EnsureDeletable()
    {
        if (DeliveredAt is not null)
            throw new DomainException("record.delivered_cannot_delete", "Teslim edilmiş kayıt silinmez. Yanlışsa düzelt.");
    }

    private readonly List<ServiceLine> _lines = [];
    public IReadOnlyList<ServiceLine> Lines => _lines;

    /// <summary>
    /// Alınan ödemeler (K-078 · elden taksit). Kasa bunlardan okunur; <see cref="PaymentMethod"/> ve
    /// <see cref="PaymentCollectedAt"/> bunların ÖZETİDİR (prototip odemeDurumu): kalan varken
    /// "almadım", kapanınca son ödemenin yolu ve (sonradan alındıysa) günü.
    /// </summary>
    private readonly List<Payment> _payments = [];
    public IReadOnlyList<Payment> Payments => _payments;

    public decimal Paid => _payments.Sum(p => p.Amount);

    /// <summary>Yazılış sırasıyla son ödeme (K-078.5) — liste sırasına güvenilmez.</summary>
    private Payment? LastPayment(Func<Payment, bool>? where = null)
        => _payments.Where(where ?? (_ => true)).MaxBy(p => p.Sequence);

    private Payment NewPayment(decimal amount, PaymentMethod method, DateOnly on, bool atDelivery)
        => new(this, amount, method, on, atDelivery, _payments.Count == 0 ? 1 : _payments.Max(p => p.Sequence) + 1);

    /// <summary>
    /// Kalan = toplam − alınanlar, en az 0 (prototip kalanTutar). Saklanmaz: teslimden sonra satır
    /// değişirse kalan yeniden hesaplanır, alınan para değişmez — para bir olgudur. Teslimsiz kayıtta 0.
    /// </summary>
    public decimal Remaining => DeliveredAt is null ? 0m : Math.Max(0m, Total - Paid);

    /// <summary>
    /// "Kalanı var mı" (<see cref="Remaining"/> &gt; 0) kuralının SQL'e çevrilen biçimi — alacak listesi,
    /// kasa özeti ve CSV yalnız bunları yükler. Toplam <see cref="Total"/>'ın aynısı (müşteri parçası
    /// girmez; silinmiş satır/ödeme sorgu süzgeciyle düşer). Boş toplam SQL'de NULL'dır, 0'a çevrilir:
    /// "x &gt; NULL" yanlış döner ve ödemesiz veresiye listeden SESSİZCE kaybolurdu. İki biçimin
    /// eşitliği <c>CollectedExpressionTests</c>'te her durumda ölçülür.
    /// </summary>
    public static System.Linq.Expressions.Expression<Func<ServiceRecord, bool>> HasRemaining
        => r => r.DeliveredAt != null
                && (r.Lines.Where(l => !(l.Kind == LineKind.Part && l.SuppliedBy == PartSupplier.Customer)).Sum(l => (decimal?)l.Price) ?? 0m)
                   > (r.Payments.Sum(p => (decimal?)p.Amount) ?? 0m);

    private ServiceRecord() { }   // EF

    /// <summary>
    /// Tür ekranda sorulmaz (Cemil, 06.09): verildiyse (sesin çözdüğü ya da
    /// ustanın seçtiği) "elle"dir; verilmediyse şikâyetten TAHMİN edilir.
    /// </summary>
    public ServiceRecord(Guid tenantId, Guid vehicleId, string complaint, ServiceType? manualType = null)
    {
        TenantId = tenantId;
        VehicleId = vehicleId;
        SetComplaint(complaint);
        if (manualType is { } t) SetType(t);
    }

    /// <summary>Şikâyet değişince tür onu TAKİP EDER — usta elle seçene kadar.</summary>
    public void SetComplaint(string? text)
    {
        Complaint = (text ?? "").Trim();
        if (!TypeSetManually) Type = ServiceTypeGuess.FromText(Complaint);
    }

    /// <summary>Elle seçim tahmini KALICI olarak yener.</summary>
    public void SetType(ServiceType type)
    {
        Type = type;
        TypeSetManually = true;
    }

    /// <summary>Km: rakam dışı atılır, en çok 7 hane, binlik noktalı (prototip GIRIS_TEMIZ.km). Boşsa yok.</summary>
    public void SetKm(string? raw)
    {
        var d = new string((raw ?? "").Where(char.IsAsciiDigit).ToArray()).TrimStart('0');
        if (d.Length > 7) d = d[..7];
        Km = d.Length == 0 ? null : long.Parse(d).ToString("#,0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
    }

    /// <summary>Parça beklendiği müşteriye söylensin mi — ustanın seçimi (A-M15); yalnız bayrak açıkken.</summary>
    public void SetShowWaitingToCustomer(bool show)
    {
        if (show && !WaitingForPart)
            throw new DomainException("record.not_waiting", "Önce \"parça bekliyor\" işaretle.");
        ShowWaitingToCustomer = show;
    }

    public void AddLine(ServiceLine line)
    {
        line.ServiceRecordId = Id;
        line.TenantId = TenantId;
        // Silinmiş satırlar yüklenmez; yeni satır onların sırasını alabilir. Geri
        // alınan satırla eşit sıra çıkarsa oluşturulma anı ayırır.
        line.Position = _lines.Count == 0 ? 1 : _lines.Max(l => l.Position) + 1;
        _lines.Add(line);
    }

    /// <summary>Kayıt toplamı — satırlardan hesaplanır, saklanmaz (K-037).</summary>
    public decimal Total => _lines.Where(l => l.CountsInTotal).Sum(l => l.Price);

    /// <summary>
    /// Tahsil günü (K-071.1): para kasaya hangi gün girdi. Ödenmediyse yok;
    /// sonradan ödendiyse ödendiği gün; teslimde ödendiyse teslim günü.
    /// Muhasebenin dönem tahsilatı bu güne göre sayar.
    /// </summary>
    public DateOnly? CollectedOn =>
        PaymentMethod is null or Domain.PaymentMethod.Unpaid ? null : PaymentCollectedAt ?? DeliveredAt;

    public bool IsMaintenance => Type is ServiceType.Maintenance or ServiceType.Both;

    /// <summary>
    /// Tahsil günü kuralının SQL'e çevrilebilen biçimi (C9): <see cref="CollectedOn"/> dönem
    /// içinde mi. İki biçim AYNI kuraldır — CollectedExpressionTests her durumda eşitliği ölçer.
    /// Kural değişirse ikisi BİRLİKTE değişir.
    /// </summary>
    public static System.Linq.Expressions.Expression<Func<ServiceRecord, bool>> CollectedBetween(DateOnly start, DateOnly end)
        // COALESCE yerine iki dal (D17): "PaymentCollectedAt ?? DeliveredAt" iki sütunu birleştirir
        // ve indeks kullanılamaz; dallar ayrı ayrı (TenantId, PaymentCollectedAt) ve
        // (TenantId, DeliveredAt) indekslerine oturur. Anlam AYNI — eşitlik testli.
        => r => r.PaymentMethod != null && r.PaymentMethod != Domain.PaymentMethod.Unpaid
                && ((r.PaymentCollectedAt != null && r.PaymentCollectedAt >= start && r.PaymentCollectedAt <= end)
                    || (r.PaymentCollectedAt == null && r.DeliveredAt >= start && r.DeliveredAt <= end));

    /// <summary>Hatırlatması geldi mi (prototip hatirlatmasiGelenler): bakım, tarihi geçmiş ya da bugün, gönderilmemiş.</summary>
    public bool IsReminderDue(DateOnly today)
        => IsMaintenance && ReminderDate is { } d && d <= today && !ReminderSent;

    /// <summary>
    /// "Hatırlatma" şablonlu mesaj (ARAMA DAHİL) gönderildi: yedinci işin döngüsü kapanır,
    /// yoksa aynı uyarı her gün çıkardı.
    /// </summary>
    public void MarkReminderSent()
    {
        if (ReminderDate is not null) ReminderSent = true;
    }

    /// <summary>
    /// Teslimden önce eksik görünenler (prototip kEksikler). EKSİKLERİ SÖYLER, YOLU
    /// KAPATMAZ: "fiyat bekleniyor" gerçek bir durum; teslimi engellemek gerçek işi
    /// yazılıma uydurmak olurdu. Müşterinin getirdiği parçada tutar beklenmez.
    /// Teslim edilmiş kayıtta liste boştur.
    /// </summary>
    public IReadOnlyList<string> DeliveryWarnings(bool hasPhone)
    {
        if (DeliveredAt is not null) return [];
        var w = new List<string>();
        var nameless = _lines.Count(l => string.IsNullOrWhiteSpace(l.Name));
        if (nameless > 0) w.Add($"{nameless} satırın adı yazılmamış");
        var priceless = _lines.Count(l => l.CountsInTotal && l.Price == 0);
        if (priceless > 0) w.Add($"{priceless} satırın tutarı yazılmamış");
        if (WaitingForPart) w.Add("parça hâlâ \"bekliyor\" işaretli");
        if (!hasPhone) w.Add("telefon yok — özet gönderilemez");
        return w;
    }

    /// <summary>
    /// Teslim (K-004'ün tek durum alanı). Bakımsa hatırlatma KURULUR — hesap tek
    /// yerde (MaintenanceReminder). <paramref name="otherReadings"/>: aynı aracın
    /// ÖTEKİ kayıtlarının km ölçümleri; bu kaydınki teslim günüyle eklenir.
    /// Eksikler teslimi ENGELLEMEZ (prototip kEksikler: "yolu kapatmaz").
    /// </summary>
    public void Deliver(PaymentMethod method, DateOnly on, MaintenancePolicy policy, IEnumerable<KmReading> otherReadings)
    {
        if (DeliveredAt is not null)
            throw new DomainException("record.already_delivered", "Bu kayıt zaten teslim edildi.");
        DeliveredAt = on;
        PaymentMethod = method;
        // Tam ödeme = toplamın tek ödeme satırı, teslim günü. Toplam 0'sa satır yok (ödeme > 0).
        if (method != Domain.PaymentMethod.Unpaid && Total > 0)
            _payments.Add(NewPayment(Total, method, on, atDelivery: true));
        var plan = PlanReminder(on, policy, otherReadings);
        ReminderDate = plan?.Date;
        ReminderSent = false;
    }

    /// <summary>
    /// Teslimde bir kısmını aldı (K-078 · "Bir kısmını aldım"): o kadarı teslim günü kasaya, kalanı
    /// veresiye. Kurallar teslimden ÖNCE sınanır — yanlış girişte araç teslim edilmiş görünmez
    /// (prototip 50-teslim). Tamamı yazıldıysa bu kısmi değil, tam ödemedir.
    /// </summary>
    public void DeliverPartly(decimal amount, PaymentMethod method, DateOnly on, MaintenancePolicy policy, IEnumerable<KmReading> otherReadings)
    {
        if (DeliveredAt is not null)
            throw new DomainException("record.already_delivered", "Bu kayıt zaten teslim edildi.");
        if (amount <= 0)
            throw new DomainException("payment.amount_required", "Ne kadar aldığını yaz.");
        if (method == Domain.PaymentMethod.Unpaid)
            throw new DomainException("payment.method_required", "Paranın nasıl alındığını seç: nakit, kart ya da havale.");
        if (amount > Total)
            throw new DomainException("payment.over_remaining", $"Toplam {MessageTemplates.Money(Total)} ₺ — fazlası girilemez.");
        if (amount == Total) { Deliver(method, on, policy, otherReadings); return; }
        Deliver(Domain.PaymentMethod.Unpaid, on, policy, otherReadings);
        _payments.Add(NewPayment(amount, method, on, atDelivery: true));
    }

    /// <summary>
    /// Yanlış teslimi geri al (D14, K-076.13) — kayıt teslim öncesi hâline döner. Dar tutuldu:
    /// yalnız AYNI GÜN (ertesi gün kasa kapanmış, müşteriye mesaj gitmiş olabilir); sonradan
    /// tahsil edildiyse önce tahsil geri alınır (UndoCollection) — para iki adımda, görünerek
    /// çıkar; hatırlatma gönderildiyse geri alınmaz (müşteriye söz verilmiş).
    /// </summary>
    public void UndoDelivery(DateOnly today)
    {
        if (DeliveredAt is null)
            throw new DomainException("delivery.nothing_to_undo", "Bu kayıt teslim edilmemiş.");
        if (DeliveredAt != today)
            throw new DomainException("delivery.undo_too_late", "Teslim yalnız aynı gün geri alınır. Yanlışsa kaydı düzelt.");
        // Sonradan alınan HER ödeme (kısmi taksit dahil) önce görünerek geri alınır.
        if (_payments.Any(p => !p.AtDelivery))
            throw new DomainException("delivery.undo_collection_first", "Önce ödemeyi geri al, sonra teslimi.");
        if (ReminderSent)
            throw new DomainException("delivery.undo_reminder_sent", "Bakım hatırlatması gönderildi — teslim geri alınamaz.");
        DeliveredAt = null;
        PaymentMethod = null;
        _payments.Clear();   // yalnız teslimde alınanlar kalmıştı; teslimle giderler
        ReminderDate = null;
        ReminderSent = false;
    }

    /// <summary>Teslim edilseydi kurulacak hatırlatma (teslim ekranının önizlemesi). Bakım değilse yok.</summary>
    public ReminderPlan? PlanReminder(DateOnly on, MaintenancePolicy policy, IEnumerable<KmReading> otherReadings)
    {
        if (!IsMaintenance) return null;
        var km = KmReading.Parse(Km);
        var history = km is { } k ? otherReadings.Append(new KmReading(on, k)) : otherReadings;
        return MaintenanceReminder.Compute(on, km, policy, history);
    }

    /// <summary>"Almadım" denmiş kaydın parası geldi — KALANIN TAMAMI (kayıt ekranındaki "Ödeme alındı").</summary>
    public void CollectPayment(PaymentMethod method, DateOnly on) => CollectPayment(Remaining, method, on);

    /// <summary>
    /// "x lira aldım" (K-078 · elden taksit). Kalandan fazlası girilemez (Cemil: "Kalandan fazla
    /// ödeme olmaz") — önceden verilen para (avans) ayrı kavram, sonra. Prototip odemeHatasi + odemeEkle.
    /// </summary>
    public void CollectPayment(decimal amount, PaymentMethod method, DateOnly on)
    {
        if (DeliveredAt is null || Remaining <= 0)
            throw new DomainException("payment.not_unpaid", "Bu kaydın alınmamış ödemesi yok.");
        if (method == Domain.PaymentMethod.Unpaid)
            throw new DomainException("payment.method_required", "Paranın nasıl alındığını seç: nakit, kart ya da havale.");
        if (on < DeliveredAt.Value)
            throw new DomainException("payment.before_delivery", "Ödeme teslimden önceki bir güne yazılamaz.");
        if (amount <= 0)
            throw new DomainException("payment.amount_required", "Ne kadar aldığını yaz.");
        if (amount > Remaining)
            throw new DomainException("payment.over_remaining", $"Kalan {MessageTemplates.Money(Remaining)} ₺ — fazlası girilemez.");
        _payments.Add(NewPayment(amount, method, on, atDelivery: false));
        RefreshPaymentStatus();
    }

    /// <summary>
    /// Durum özeti ödemelerden (prototip odemeDurumu): kalan varken "almadım"; kapanınca son ödemenin
    /// yolu, sonradan alındıysa günü (teslimde alındıysa tahsil günü teslim günüdür, ayrı tutulmaz).
    /// Ödemesi olmayan kapalı kayıtta (toplam 0) durum olduğu gibi kalır.
    /// </summary>
    private void RefreshPaymentStatus()
    {
        if (Remaining > 0) { PaymentMethod = Domain.PaymentMethod.Unpaid; PaymentCollectedAt = null; return; }
        if (LastPayment() is not { } last) return;
        PaymentMethod = last.Method;
        PaymentCollectedAt = last.AtDelivery ? null : last.PaidOn;
    }

    /// <summary>
    /// Ödeme yolunu düzelt (E1, K-077.2): "nakit aldım" dedi, kartla almıştı. Teslim günü ve (varsa)
    /// sonraki tahsil günü KORUNUR — yalnız yol değişir, kasa dağılımı kendiliğinden düzelir.
    /// Teslimde alınan ödeme dört yol arasında serbest; sonradan tahsil edilmişse yalnız tahsilin
    /// yolu değişir — "almadım"a çevirmek tahsili geri almaktır (UndoCollection), o ayrı adım.
    /// </summary>
    /// <remarks>
    /// K-078'den sonra düzeltilen, SON ÖDEME SATIRININ yoludur (tutarı ve günü korunur). Ödemesi
    /// olmayan veresiyeyi "aslında teslimde almıştım"a çevirmek kalanı teslim gününe yazar; teslimde
    /// alınanı "almadım"a çevirmek o ödemeyi siler.
    /// </remarks>
    public void CorrectPaymentMethod(PaymentMethod method)
    {
        if (DeliveredAt is null)
            throw new DomainException("payment.not_delivered", "Kayıt teslim edilmedi — düzeltilecek ödeme yok.");
        // G7: avansla nakit/kart arasında çevirmek geriye dönük kasayı ve bakiyeyi değiştirir.
        if (method == Domain.PaymentMethod.Advance || LastPayment()?.Method == Domain.PaymentMethod.Advance)
            throw new DomainException("payment.advance_fixed", "Avanstan düşülen ödemenin yolu değişmez — geri al, yeniden yaz.");
        if (LastPayment() is not { } last)
        {
            if (PaymentMethod == method)
                throw new DomainException("payment.same_method", "Ödeme zaten böyle yazılı.");
            PaymentMethod = method;
            if (method != Domain.PaymentMethod.Unpaid && Remaining > 0)
                _payments.Add(NewPayment(Remaining, method, DeliveredAt.Value, atDelivery: true));
            return;
        }
        if (last.Method == method)
            throw new DomainException("payment.same_method", "Ödeme zaten böyle yazılı.");
        if (method == Domain.PaymentMethod.Unpaid)
        {
            if (!last.AtDelivery)
                throw new DomainException("payment.use_undo_collection", "Sonradan alınan ödemeyi silmek için \"Ödemeyi geri al\"ı kullan.");
            _payments.Remove(last);
        }
        else last.Correct(method);
        RefreshPaymentStatus();
    }

    /// <summary>"Geri al" — yanlış çipe dokunmak kasaya olmayan parayı yazmasın.</summary>
    /// <remarks>K-078: SON sonradan-alınan ödeme silinir (prototip sonOdemeyiGeriAl); teslimdeki para teslimle gider.</remarks>
    public void UndoCollection()
    {
        if (LastPayment(p => !p.AtDelivery) is not { } p)
            throw new DomainException("payment.nothing_to_undo", "Geri alınacak bir tahsilat yok.");
        _payments.Remove(p);
        RefreshPaymentStatus();
    }

    public void SetWaitingForPart(bool waiting, DateOnly today)
    {
        // Bayrak zaten açıksa tarih DEĞİŞMEZ: ikinci "evet" beklemenin başladığı günü silmesin.
        if (waiting && WaitingForPart) return;
        WaitingForPart = waiting;
        WaitingSince = waiting ? today : null;
        // Bayrak kapanınca müşteriye gösterme seçimi de anlamını yitirir (prototip kBekliyor).
        if (!waiting) ShowWaitingToCustomer = false;
    }
}
