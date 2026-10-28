using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tamiron.Ui.Core;

// D20 · Arayüzün API'yle tek konuşma yeri (K-076.19). Karar ARAYÜZDE DEĞİL: istemci API'nin
// söylediğini gösterir (CLAUDE.md "iş mantığı API'de"). DTO'lar burada küçük kopyalar —
// Application'a başvurmak tarayıcıya EF Core taşırdı.

public sealed record LoginResult(string Token, string ShopName, string? UserName);

public sealed record RecordListItem(Guid Id, Guid VehicleId, string PlateNo, string Type, string TypeLabel,
                                    string Complaint, DateOnly OpenedOn, DateOnly? DeliveredAt,
                                    string? PaymentMethod, bool WaitingForPart, decimal Total,
                                    string? Brand, string? Model, string? OwnerName,
                                    string? Km = null, DateOnly? ReminderDate = null);

public sealed record RecordList(int InShopCount, int Count, IReadOnlyList<RecordListItem> Items);

/// <summary>G9 · Araç ekranı (API VehicleDto'nun ekranın okuduğu kısmı).</summary>
public sealed record VehicleDetail(Guid Id, string PlateNo, string? Brand, string? Model, string? Year, string? Engine, string? Vin,
                                   string? Color, string? FuelType, string? EngineNo, string? Displacement, string? PowerKw, string? Seats,
                                   DateOnly? FirstRegistrationDate, string? FuelTankLiters, string? OwnerName, string? OwnerPhone,
                                   string? ContactName, string? ContactPhone, string? TaxOrNationalId, string? Address, string? TaxOffice,
                                   string? Note);

/// <summary>Araç düzeltmesi (API PatchVehicle): null = dokunma, "" = temizle. Biçim ve geçerlilik API'de.</summary>
public sealed record VehiclePatch(string? Brand = null, string? Model = null, string? Year = null, string? Engine = null,
                                  string? Color = null, string? FuelType = null, string? Vin = null, string? OwnerName = null,
                                  string? OwnerPhone = null, string? ContactName = null, string? ContactPhone = null, string? Note = null);

public sealed record VehicleListItem(Guid Id, string PlateNo, string? Brand, string? Model, string? OwnerName, string? ContactName, bool InShop);
public sealed record VehicleList(int Count, IReadOnlyList<VehicleListItem> Items);

// G9b · Müşteriler — tablo yok, araçlardan türetilir (API). Anahtar: katlanmış ad + "|" + telefon.
public sealed record CustomerListItem(string Key, string Name, string? Phone, string DisplayPlate, int VehicleCount, bool InShop, DateOnly? ConsentAt);
public sealed record CustomerList(int Count, IReadOnlyList<CustomerListItem> Items);
public sealed record CustomerVehicle(Guid Id, string PlateNo, string? Brand, string? Model, bool InShop);
public sealed record CustomerDetail(string Key, string Name, string? Phone, DateOnly? ConsentAt, IReadOnlyList<CustomerVehicle> Vehicles);

// G9c · Mesaj. GÖNDERİM YOK — API ne gittiğini KAYDEDER, gönderimi ustanın kendi uygulaması yapar.
// G10 · Parçacılar ve parça sorma. Mesajı API GÖNDERMEZ — WhatsApp'ı ustanın telefonu açar.
public sealed record Supplier(Guid Id, string Name, string Phone);
public sealed record AskedLine(Guid Id, string Name, Guid? SelectedReplyId);
public sealed record PriceQuestionPreview(IReadOnlyList<AskedLine> Lines, bool HasPartIdentity, IReadOnlyList<string> Missing, string Text);
public sealed record PriceReply(Guid Id, string RawText, decimal? ParsedPrice, string? ParsedBrand, string? ParsedCode, DateTime ReceivedAt);
public sealed record PriceRequest(Guid Id, Guid SupplierId, string SupplierName, string SupplierPhone, string SentText, DateTime SentAt,
                                  IReadOnlyList<AskedLine> Lines, IReadOnlyList<PriceReply> Replies);

// G11b · Tarihi gelmiş, gönderilmemiş bakım hatırlatmaları (B12) — çekmecenin bildirimleri ve menü rozeti.
public sealed record DueReminder(Guid RecordId, Guid VehicleId, string PlateNo, string? OwnerName, string? Phone, DateOnly ReminderDate, int DaysOverdue);

// H1 · İş başına fark (API ListJobProfits): alınan − parça alışı; bilinmeyen alış "hesap dışı" (K-057).
public sealed record JobProfitItem(Guid RecordId, string PlateNo, string Type, string TypeLabel, string? Brand, DateOnly DeliveredAt,
                                   decimal Received, decimal PartsCost, decimal Outside, int UnknownCount, decimal Difference);
public sealed record JobProfitList(string Period, DateOnly Start, DateOnly End, decimal Received, decimal PartsCost, decimal Outside,
                                   int UnknownCount, decimal Difference, bool AskPartCost, IReadOnlyList<JobProfitItem> Items);

// H4 · Parçacıya borç (K-090): borç saklanmaz — veresiye alışlar − ödemeler (API).
public sealed record DebtLine(Guid RecordId, Guid LineId, string PlateNo, string Name, decimal Cost);
public sealed record SupplierPaymentItem(Guid Id, decimal Amount, string Method, DateOnly PaidOn);
public sealed record SupplierDebtItem(Guid SupplierId, string Name, string Phone, decimal Debt, IReadOnlyList<DebtLine> Lines, IReadOnlyList<SupplierPaymentItem> Payments);
public sealed record SupplierDebtList(decimal Total, IReadOnlyList<SupplierDebtItem> Items);

public sealed record MessageSetup(IReadOnlyList<MessageTarget> Targets, Guid? CurrentRecordId);
public sealed record MessageTarget(string Kind, string Name, string? Phone, bool IsDefault);
public sealed record MessageSuggestion(string Template, string Name, string Text);
public sealed record MessageItem(Guid Id, Guid VehicleId, Guid? ServiceRecordId, DateTime SentAt, string RecipientName,
                                 string RecipientPhone, string Channel, string Template, bool IsBulk, string Body);

/// <summary>G2 · Plaka sorusunun cevabı (API LookupPlate): geçerli mi, ipucu, kayıtlı araç, açık kaydı.</summary>
public sealed record PlateLookup(string Plate, bool Valid, string? Hint, VehicleBrief? Vehicle, Guid? OpenRecordId);

public sealed record VehicleBrief(Guid Id, string PlateNo, string? Brand, string? Model, string? OwnerName, bool HasPartIdentity);

/// <summary>Kayıtlı olmayan araç için ekranda yazılanlar — kayıtla AYNI istekte gider (API NewVehicleInfo).</summary>
public sealed record NewVehicleInfo(string? Brand, string? Model, string? Year, string? Engine, string? Color,
                                    string? FuelType, string? Vin, string? OwnerName, string? OwnerPhone);

public sealed record OpenedRecord(Guid Id, Guid VehicleId, string PlateNo);

/// <summary>G3 · Servis kaydı (API ServiceRecordDto'nun ekranın okuduğu kısmı).</summary>
public sealed record RecordDetail(Guid Id, Guid VehicleId, string PlateNo, string Type, string Complaint,
                                  DateOnly? DeliveredAt, bool WaitingForPart, bool ShowWaitingToCustomer,
                                  decimal Total, IReadOnlyList<LineItem> Lines, IReadOnlyList<string> Warnings,
                                  decimal Paid, decimal Remaining, string? PaymentMethod = null, DateOnly? ReminderDate = null,
                                  IReadOnlyList<PaymentItem>? Payments = null);

/// <summary>Alınan ödeme (K-078). AtDelivery: teslimde alındı — teslimle gider, "geri al"la değil.</summary>
public sealed record PaymentItem(Guid Id, decimal Amount, string Method, DateOnly PaidOn, bool AtDelivery);

public sealed record LineItem(Guid Id, string Kind, string Name, decimal Price, decimal? Cost, string? SuppliedBy, bool CountsInTotal,
                              Guid? SupplierId = null, bool OnCredit = false);

/// <summary>API'ye giden satır (LineBody). Kind: "Part" | "Labor"; SuppliedBy: null | "Shop" | "Customer".</summary>
public sealed record LineInput(string Kind, string Name, decimal Price, decimal? Cost, string? SuppliedBy);

public sealed record VehicleCard(Guid Id, string PlateNo, string? Brand, string? Model, string? OwnerName, string? OwnerPhone,
                                 string? ContactName, string? ContactPhone, string? Year = null, string? Engine = null);

/// <summary>G4 · Teslimden önce: kurulacak bakım hatırlatması (API DeliveryPreview). Basis: "Month" | "Km".</summary>
public sealed record DeliveryPreview(Guid RecordId, ReminderPlan? Reminder);
public sealed record ReminderPlan(DateOnly Date, string Basis, DateOnly MonthDate, int? KmTarget, DateOnly? KmDate);

public sealed record ShopSettings(string Name, int MaintenanceMonths, int MaintenanceKm, bool AskPartCost, string? Address = null, string? Phone = null);

/// <summary>G6 · Muhasebe tek çağrı (C8). Period: "Today" | "Month" | "Year".</summary>
public sealed record AccountingOverview(CashSummary Cash, ExpenseList Expenses, ReceivableList Receivables);
public sealed record CashSummary(string Period, DateOnly Start, DateOnly End, int DeliveredCount,
                                 decimal Cash, decimal Card, decimal Transfer, decimal Collected, decimal Receivable);
public sealed record ExpenseList(string Period, decimal Total, IReadOnlyList<ExpenseItem> Items);
public sealed record ExpenseItem(Guid Id, DateOnly Date, string Description, decimal Amount);
public sealed record ReceivableList(decimal Total, IReadOnlyList<ReceivableItem> Items);
public sealed record ReceivableItem(Guid RecordId, Guid VehicleId, string PlateNo, string? OwnerName, string Description,
                                    DateOnly? DeliveredAt, decimal Total, decimal Paid, decimal Remaining);

/// <summary>
/// G2b · Ruhsat okuma sonucu (API RegistrationResult). Outcome: "Save" | "PlateConflict" | "NoPlate".
/// <see cref="Read"/> HAM tutulur: çatışmada seçilen plakayla API'ye olduğu gibi geri gider (alan kaybolmasın).
/// </summary>
public sealed record RegistrationResult(string Outcome, string? TypedPlate, string? ReadPlate, JsonElement Read,
                                        bool VinRead, bool Created, RegistrationVehicle? Vehicle)
{
    public string? Field(string name) => Read.ValueKind == JsonValueKind.Object && Read.TryGetProperty(name, out var v)
                                         && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}

public sealed record RegistrationVehicle(Guid Id, string PlateNo);

/// <summary>G8 · Sahibin avans bakiyesi ve hareketleri (API AdvanceView). Kind: "Received" | "Refunded".</summary>
public sealed record AdvanceView(decimal Balance, IReadOnlyList<AdvanceItem> Items);
public sealed record AdvanceItem(Guid Id, string Kind, decimal Amount, string Method, DateOnly On, string PlateNo);

public sealed record HomeSummary(int InShopCount, int RecordCount, int VehicleCount, int CustomerCount,
                                 decimal CollectedToday, decimal Receivable, int DueReminderCount);

/// <summary>API'nin Türkçe hatası (Problem Details: code + title). Ekran <see cref="Exception.Message"/>'ı olduğu gibi gösterir.</summary>
public sealed class ApiException(HttpStatusCode status, string? code, string message) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
    public string? Code { get; } = code;
}

/// <summary>Oturum anahtarı — süresiz (K-074); cihazda saklanır, yalnız çıkışla silinir.</summary>
public interface ITokenStore
{
    ValueTask<string?> GetAsync();
    ValueTask SetAsync(string? token);
}

public sealed class TamironApi(HttpClient http, ITokenStore tokens)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public async Task RequestCodeAsync(string phone) => await Send(HttpMethod.Post, "api/auth/code", new { phone }, auth: false);

    public async Task<LoginResult> VerifyAsync(string phone, string code, string? device)
    {
        var r = await Read<LoginResult>(await Send(HttpMethod.Post, "api/auth/verify", new { phone, code, deviceName = device }, auth: false));
        await tokens.SetAsync(r.Token);
        return r;
    }

    public async Task<bool> HasSessionAsync() => !string.IsNullOrEmpty(await tokens.GetAsync());

    /// <summary>G1 · Yeni dükkân (D6): kayıtlı olmayan numaraya kod; kod + dükkân adı → oturum.</summary>
    public async Task RequestSignupCodeAsync(string phone) => await Send(HttpMethod.Post, "api/auth/signup/code", new { phone }, auth: false);

    public async Task<LoginResult> SignUpAsync(string phone, string code, string shopName, string? device)
    {
        var r = await Read<LoginResult>(await Send(HttpMethod.Post, "api/auth/signup", new { phone, code, shopName, deviceName = device }, auth: false));
        await tokens.SetAsync(r.Token);
        return r;
    }

    /// <summary>Çıkış bu cihazın oturumunu bitirir (K-074). Sunucuya ulaşılamasa da anahtar burada silinir.</summary>
    public async Task LogoutAsync()
    {
        try { (await Send(HttpMethod.Post, "api/auth/logout", null, auth: true)).Dispose(); }
        finally { await tokens.SetAsync(null); }
    }

    public async Task<PlateLookup> LookupPlateAsync(string plate)
        => await Read<PlateLookup>(await Send(HttpMethod.Get, "api/vehicles/lookup?plate=" + Uri.EscapeDataString(plate), null, auth: true));

    public async Task<OpenedRecord> OpenRecordAsync(string plate, NewVehicleInfo? vehicle)
        => await Read<OpenedRecord>(await Send(HttpMethod.Post, "api/records", new { plate, complaint = "", vehicle }, auth: true));

    public async Task<RecordDetail> RecordAsync(Guid id) => await Read<RecordDetail>(await Send(HttpMethod.Get, $"api/records/{id}", null, auth: true));
    public async Task<VehicleCard> VehicleAsync(Guid id) => await Read<VehicleCard>(await Send(HttpMethod.Get, $"api/vehicles/{id}", null, auth: true));
    public async Task<RecordDetail> AddLineAsync(Guid id, LineInput l) => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/lines", l, auth: true));
    public async Task<RecordDetail> UpdateLineAsync(Guid id, Guid lineId, LineInput l) => await Read<RecordDetail>(await Send(HttpMethod.Put, $"api/records/{id}/lines/{lineId}", l, auth: true));
    public async Task<RecordDetail> DeleteLineAsync(Guid id, Guid lineId) => await Read<RecordDetail>(await Send(HttpMethod.Delete, $"api/records/{id}/lines/{lineId}", null, auth: true));
    public async Task<RecordDetail> RestoreLineAsync(Guid id, Guid lineId) => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/lines/{lineId}/restore", null, auth: true));

    /// <summary>Kısmi güncelleme (API RecordPatch): null = dokunma.</summary>
    public async Task<RecordDetail> PatchRecordAsync(Guid id, string? complaint = null, string? type = null, bool? waitingForPart = null, bool? showWaitingToCustomer = null)
        => await Read<RecordDetail>(await Send(HttpMethod.Patch, $"api/records/{id}", new { complaint, type, waitingForPart, showWaitingToCustomer }, auth: true));

    public async Task<DeliveryPreview> DeliveryPreviewAsync(Guid id) => await Read<DeliveryPreview>(await Send(HttpMethod.Get, $"api/records/{id}/delivery-preview", null, auth: true));
    // G11 · Ayar başına ayrı istek: yalnız değişen alan gider (API kısmi — null = dokunma).
    public async Task<ShopSettings> SetMaintenanceMonthsAsync(int months)
        => await Read<ShopSettings>(await Send(HttpMethod.Patch, "api/shop", new { maintenanceMonths = months }, auth: true));
    public async Task<ShopSettings> SetAskPartCostAsync(bool ask)
        => await Read<ShopSettings>(await Send(HttpMethod.Patch, "api/shop", new { askPartCost = ask }, auth: true));
    public async Task<ShopSettings> ShopAsync() => await Read<ShopSettings>(await Send(HttpMethod.Get, "api/shop", null, auth: true));

    /// <summary>Teslim (K-004). <paramref name="amount"/> dolu = "Bir kısmını aldım" (K-078); kural API'de.</summary>
    public async Task<RecordDetail> DeliverAsync(Guid id, string method, decimal? amount)
        => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/deliver", new { method, amount }, auth: true));

    /// <summary>Sonradan tahsilat (K-078): tutar + yol; fazlası API'de reddedilir.</summary>
    public async Task<RecordDetail> CollectAsync(Guid id, string method, decimal amount)
        => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/collect", new { method, amount }, auth: true));
    public async Task<RecordDetail> UndoCollectAsync(Guid id) => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/collect/undo", null, auth: true));
    /// <summary>E1 · Ödeme yolunu düzelt (K-077.2): son ödemenin yolu; gün korunur.</summary>
    public async Task<RecordDetail> CorrectPaymentAsync(Guid id, string method)
        => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/payment", new { method }, auth: true));

    public async Task<JobProfitList> ProfitAsync(string period)
        => await Read<JobProfitList>(await Send(HttpMethod.Get, "api/profit?period=" + period, null, auth: true));
    public async Task<AccountingOverview> AccountingAsync(string period)
        => await Read<AccountingOverview>(await Send(HttpMethod.Get, $"api/accounting?period={period}", null, auth: true));
    public async Task AddExpenseAsync(string description, decimal amount)
        => (await Send(HttpMethod.Post, "api/expenses", new { description, amount }, auth: true)).Dispose();
    public async Task<ExpenseList> DeleteExpenseAsync(Guid id, string period)
        => await Read<ExpenseList>(await Send(HttpMethod.Delete, $"api/expenses/{id}?period={period}", null, auth: true));
    public async Task<ExpenseList> RestoreExpenseAsync(Guid id, string period)
        => await Read<ExpenseList>(await Send(HttpMethod.Post, $"api/expenses/{id}/restore?period={period}", null, auth: true));

    /// <summary>Muhasebeci dosyası (B9): API'nin ürettiği CSV baytları ve dosya adı.</summary>
    public async Task<(string FileName, byte[] Content)> CashCsvAsync(string period)
    {
        using var res = await Send(HttpMethod.Get, $"api/export/cash.csv?period={period}", null, auth: true);
        var name = res.Content.Headers.ContentDisposition?.FileNameStar ?? res.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                   ?? $"tamiron-kasa-{period}.csv";
        return (name, await res.Content.ReadAsByteArrayAsync());
    }

    /// <summary>Ruhsat fotoğrafı (D8): çok parçalı "photo" + ustanın yazdığı plaka (K-069).</summary>
    public async Task<RegistrationResult> ReadRegistrationAsync(byte[] image, string mediaType, string? typedPlate)
    {
        var form = new MultipartFormDataContent();
        var foto = new ByteArrayContent(image);
        foto.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        form.Add(foto, "photo", "ruhsat");
        if (!string.IsNullOrWhiteSpace(typedPlate)) form.Add(new StringContent(typedPlate), "plate");
        return await Read<RegistrationResult>(await Send(HttpMethod.Post, "api/registrations/read", form, auth: true));
    }

    public async Task<RegistrationResult> SaveRegistrationAsync(string plate, JsonElement read)
        => await Read<RegistrationResult>(await Send(HttpMethod.Post, "api/registrations/save", new { plate, read }, auth: true));

    public async Task<AdvanceView> AdvanceAsync(Guid vehicleId) => await Read<AdvanceView>(await Send(HttpMethod.Get, $"api/vehicles/{vehicleId}/advance", null, auth: true));
    public async Task<AdvanceView> ReceiveAdvanceAsync(Guid vehicleId, decimal amount, string method)
        => await Read<AdvanceView>(await Send(HttpMethod.Post, $"api/vehicles/{vehicleId}/advance", new { amount, method }, auth: true));
    public async Task<AdvanceView> RefundAdvanceAsync(Guid vehicleId, decimal amount, string method)
        => await Read<AdvanceView>(await Send(HttpMethod.Post, $"api/vehicles/{vehicleId}/advance/refund", new { amount, method }, auth: true));
    public async Task<AdvanceView> UndoAdvanceAsync(Guid advanceId) => await Read<AdvanceView>(await Send(HttpMethod.Post, $"api/advances/{advanceId}/undo", null, auth: true));

    public async Task<HomeSummary> HomeAsync() => await Read<HomeSummary>(await Send(HttpMethod.Get, "api/home", null, auth: true));

    public async Task<RecordList> RecordsAsync(string? q = null, bool inShop = false, Guid? vehicleId = null)
    {
        var url = "api/records?inShop=" + (inShop ? "true" : "false") + (string.IsNullOrWhiteSpace(q) ? "" : "&q=" + Uri.EscapeDataString(q))
                + (vehicleId is { } v ? "&vehicleId=" + v : "");
        return await Read<RecordList>(await Send(HttpMethod.Get, url, null, auth: true));
    }

    // G9 · Araç ve araçlar.
    public async Task<VehicleDetail> VehicleDetailAsync(Guid id) => await Read<VehicleDetail>(await Send(HttpMethod.Get, $"api/vehicles/{id}", null, auth: true));
    public async Task<VehicleDetail> PatchVehicleAsync(Guid id, VehiclePatch p)
        => await Read<VehicleDetail>(await Send(HttpMethod.Patch, $"api/vehicles/{id}", p, auth: true));
    public async Task<VehicleList> VehiclesAsync(string? q = null)
        => await Read<VehicleList>(await Send(HttpMethod.Get, "api/vehicles/list" + (string.IsNullOrWhiteSpace(q) ? "" : "?q=" + Uri.EscapeDataString(q)), null, auth: true));
    public async Task<ReceivableList> ReceivablesAsync(Guid vehicleId)
        => await Read<ReceivableList>(await Send(HttpMethod.Get, $"api/receivables?vehicleId={vehicleId}", null, auth: true));

    // G9b · Müşteriler. İzin günü sunucudan — istemci tarih göndermez (izin tarihi ispattır).
    public async Task<CustomerList> CustomersAsync(string? q = null)
        => await Read<CustomerList>(await Send(HttpMethod.Get, "api/customers" + (string.IsNullOrWhiteSpace(q) ? "" : "?q=" + Uri.EscapeDataString(q)), null, auth: true));
    public async Task<CustomerDetail> CustomerAsync(string key)
        => await Read<CustomerDetail>(await Send(HttpMethod.Get, "api/customers/detail?key=" + Uri.EscapeDataString(key), null, auth: true));
    public async Task<CustomerDetail> SetConsentAsync(string key, bool granted)
        => await Read<CustomerDetail>(await Send(HttpMethod.Put, "api/customers/consent", new { key, granted }, auth: true));
    public async Task<List<DueReminder>> DueRemindersAsync() => await Read<List<DueReminder>>(await Send(HttpMethod.Get, "api/reminders/due", null, auth: true));

    // G10 · Parçacılar (ad + telefon; numara dükkânda tekil — API) ve parça sorma.
    public async Task<List<Supplier>> SuppliersAsync() => await Read<List<Supplier>>(await Send(HttpMethod.Get, "api/suppliers", null, auth: true));
    public async Task<Supplier> AddSupplierAsync(string name, string phone)
        => await Read<Supplier>(await Send(HttpMethod.Post, "api/suppliers", new { name, phone }, auth: true));
    public async Task<Supplier> UpdateSupplierAsync(Guid id, string name, string phone)
        => await Read<Supplier>(await Send(HttpMethod.Put, $"api/suppliers/{id}", new { name, phone }, auth: true));
    public async Task DeleteSupplierAsync(Guid id) => (await Send(HttpMethod.Delete, $"api/suppliers/{id}", null, auth: true)).Dispose();
    public async Task<PriceQuestionPreview> PriceQuestionAsync(Guid recordId)
        => await Read<PriceQuestionPreview>(await Send(HttpMethod.Get, $"api/records/{recordId}/price-question", null, auth: true));
    public async Task<List<PriceRequest>> PriceRequestsAsync(Guid recordId)
        => await Read<List<PriceRequest>>(await Send(HttpMethod.Get, $"api/records/{recordId}/price-requests", null, auth: true));
    public async Task<List<PriceRequest>> AskPricesAsync(Guid recordId, IReadOnlyList<Guid> supplierIds)
        => await Read<List<PriceRequest>>(await Send(HttpMethod.Post, $"api/records/{recordId}/price-requests", new { supplierIds }, auth: true));
    public async Task<RecordDetail> AddPriceReplyAsync(Guid requestId, Guid lineId, string? rawText, decimal? price, decimal? customerPrice, bool onCredit = false)
        => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/price-requests/{requestId}/replies", new { lineId, rawText, price, customerPrice, onCredit }, auth: true));

    // H4 · Parçacıya borç (K-090): satırda kimden alındı + veresiye; borç listesi, öde, geri al.
    public async Task<RecordDetail> SetLinePurchaseAsync(Guid id, Guid lineId, Guid? supplierId, bool onCredit)
        => await Read<RecordDetail>(await Send(HttpMethod.Patch, $"api/records/{id}/lines/{lineId}/purchase", new { supplierId, onCredit }, auth: true));
    public async Task<SupplierDebtList> SupplierDebtsAsync() => await Read<SupplierDebtList>(await Send(HttpMethod.Get, "api/supplier-debts", null, auth: true));
    public async Task<SupplierDebtList> PaySupplierAsync(Guid supplierId, decimal amount, string method)
        => await Read<SupplierDebtList>(await Send(HttpMethod.Post, $"api/suppliers/{supplierId}/payments", new { amount, method }, auth: true));
    public async Task<SupplierDebtList> UndoSupplierPaymentAsync(Guid paymentId)
        => await Read<SupplierDebtList>(await Send(HttpMethod.Post, $"api/supplier-payments/{paymentId}/undo", null, auth: true));
    public async Task<ReceivableList> AllReceivablesAsync() => await Read<ReceivableList>(await Send(HttpMethod.Get, "api/receivables", null, auth: true));

    // G9c · Mesaj: kurulum (kime, hangi iş), öneriler (hedefe göre hitap), kayıt, arşiv.
    public async Task<MessageSetup> MessageSetupAsync(Guid vehicleId)
        => await Read<MessageSetup>(await Send(HttpMethod.Get, $"api/vehicles/{vehicleId}/message-setup", null, auth: true));
    public async Task<List<MessageSuggestion>> MessageSuggestionsAsync(Guid vehicleId, string? target)
        => await Read<List<MessageSuggestion>>(await Send(HttpMethod.Get, $"api/vehicles/{vehicleId}/message-suggestions" + (target is null ? "" : "?target=" + target), null, auth: true));
    public async Task<MessageItem> RecordMessageAsync(Guid vehicleId, string channel, string? template, string body, Guid? serviceRecordId, string? target)
        => await Read<MessageItem>(await Send(HttpMethod.Post, "api/messages", new { vehicleId, channel, template, body, serviceRecordId, target }, auth: true));
    public async Task<List<MessageItem>> MessagesAsync(Guid? vehicleId = null)
        => await Read<List<MessageItem>>(await Send(HttpMethod.Get, "api/messages" + (vehicleId is { } v ? "?vehicleId=" + v : ""), null, auth: true));
    public async Task<CustomerDetail> UndoConsentAsync(string key)
        => await Read<CustomerDetail>(await Send(HttpMethod.Post, "api/customers/consent/undo", new { key }, auth: true));

    private async Task<HttpResponseMessage> Send(HttpMethod m, string url, object? body, bool auth)
    {
        using var req = new HttpRequestMessage(m, url);
        // Fotoğraf gibi çok parçalı gövde olduğu gibi gider; gerisi JSON.
        if (body is HttpContent icerik) req.Content = icerik;
        else if (body is not null) req.Content = JsonContent.Create(body, options: Json);
        if (auth && await tokens.GetAsync() is { Length: > 0 } t) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", t);
        HttpResponseMessage res;
        try { res = await http.SendAsync(req); }
        catch (HttpRequestException) { throw new ApiException(0, "network", "Sunucuya ulaşılamadı. İnternet bağlantını kontrol et."); }
        if (res.IsSuccessStatusCode) return res;

        // Oturum düştüyse (çıkış başka cihazdan yapıldı) anahtar silinir — ekran girişe döner.
        if (res.StatusCode == HttpStatusCode.Unauthorized && auth) await tokens.SetAsync(null);
        string? code = null, title = null;
        try
        {
            var p = await res.Content.ReadFromJsonAsync<JsonElement>(Json);
            if (p.TryGetProperty("code", out var c)) code = c.GetString();
            if (p.TryGetProperty("title", out var ti)) title = ti.GetString();
        }
        catch (JsonException) { }
        throw new ApiException(res.StatusCode, code, title ?? "Bir şeyler ters gitti. Tekrar dene.");
    }

    private static async Task<T> Read<T>(HttpResponseMessage res)
    {
        using (res) return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }
}

/// <summary>Ekranın biçimleri — prototipin <c>tutar</c> ve tarih yazımıyla aynı.</summary>
public static class Format
{
    /// <summary>
    /// Ayırıcılar AÇIKÇA yazılır, tr-TR kültür verisinden okunmaz: Blazor WASM tarayıcıya yalnız
    /// tarayıcı dilinin kültür verisini yükler; İngilizce tarayıcıda "3.450" → "3,450" oluyordu
    /// (tarayıcıda ölçülerek bulundu — sunucudaki testler tam veriyle koştuğu için göremezdi).
    /// </summary>
    private static readonly NumberFormatInfo Tr = new() { NumberGroupSeparator = ".", NumberDecimalSeparator = ",", NegativeSign = "-" };
    /// <summary>JS toLocaleString('tr-TR'): binlik nokta, gereksiz ondalık yok.</summary>
    public static string Money(decimal n) => n.ToString("#,0.###", Tr);
    /// <summary>Ustanın yazdığı tutar: "1.200" · "1200" · "1.234,5" · "" (boş = 0). Okunamazsa null.</summary>
    public static decimal? ParseMoney(string? s)
    {
        var t = (s ?? "").Replace("₺", "").Replace(" ", "").Replace(".", "").Replace(',', '.');
        if (t.Length == 0) return 0;
        return decimal.TryParse(t, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d) && d >= 0 ? d : null;
    }
    /// <summary>Ödeme yolunun Türkçesi (prototip ODEME_ETIKET).</summary>
    public static string PaymentLabel(string? method) => method switch
    {
        "Cash" => "Nakit", "Card" => "Kart", "Transfer" => "Havale", "Unpaid" => "Almadım", "Advance" => "Avanstan", _ => "",
    };

    static readonly string[] Aylar = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];
    /// <summary>Prototip AYLAR — dönem çipinin adı ("Ekim").</summary>
    public static string MonthName(DateTime d) => Aylar[d.Month - 1];

    /// <summary>Prototip donemAdi: "Bugün" · ayın adı · yıl — rakam kendi tarihini taşısın ("bu ay" demez).</summary>
    public static string PeriodName(string period, DateTime today) => period switch
    {
        "Month" => MonthName(today),
        "Year" => today.Year.ToString(CultureInfo.InvariantCulture),
        _ => "Bugün",
    };

    public static string Date(DateOnly d) => d.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    // G9c · Gönderimi USTANIN uygulaması yapar (WHATSAPP.md A yolu): biz metni hazır hâlde açarız.
    static string Digits(string tel) => new(tel.Where(char.IsAsciiDigit).ToArray());
    /// <summary>wa.me ülke koduyla ister: "0532…" / "532…" / "+90 532…" → "90532…".</summary>
    public static string WhatsAppLink(string tel, string text)
    {
        var d = Digits(tel);
        var n = d.StartsWith("90") && d.Length == 12 ? d : d.StartsWith('0') ? "90" + d[1..] : "90" + d;
        return $"https://wa.me/{n}?text={Uri.EscapeDataString(text)}";
    }
    public static string SmsLink(string tel, string text) => $"sms:{Digits(tel)}?body={Uri.EscapeDataString(text)}";
    public static string TelLink(string tel) => $"tel:{Digits(tel)}";
}
