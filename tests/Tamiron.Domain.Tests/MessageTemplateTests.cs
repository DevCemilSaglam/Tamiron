using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// D4 · Mesaj önerileri. Beklenen davranış prototipin MESAJ_SABLONLARI.uygunKayit,
/// mesajMetniUret ve hazirMesajMetni'si (02-veri.js, 03-nesne.js).
/// </summary>
public class MessageTemplateTests
{
    static readonly DateOnly Today = new(2026, 10, 22);
    static readonly Shop Dukkan = new() { Name = "Usta Oto", Phone = "0532 000 00 00" };
    static readonly Vehicle Egea = new() { PlateNo = "34 ABC 123", Brand = "Fiat", Model = "Egea", OwnerName = "Mehmet Kaya", OwnerPhone = "05321234567" };
    const string Imza = "\n\nUsta Oto\n0532 000 00 00";

    static ServiceRecord Rec(string complaint = "Fren") => new(Guid.NewGuid(), Egea.Id, complaint);
    static MessageContext Ctx(ServiceRecord? current, ServiceRecord? last = null, ServiceRecord? maint = null)
        => new(Egea, Egea.DefaultTarget(), Dukkan, current, last ?? current, maint);

    // ---- Hangi şablon bu işe uygun (kayıt bazlı, araç bazlı DEĞİL) ----

    [Fact]
    public void Open_job_suggests_summary_and_ready()
        => Assert.Equal(new[] { "ozet", "hazir", "serbest" }, MessageTemplates.Applicable(Rec(), []));

    [Fact]
    public void Summary_already_sent_is_not_suggested_again_waiting_part_adds_waiting()
    {
        var r = Rec();
        r.SetWaitingForPart(true, Today);
        Assert.Equal(new[] { "bekliyor", "hazir", "serbest" }, MessageTemplates.Applicable(r, ["ozet"]));
    }

    [Fact]
    public void Delivered_job_suggests_delivered_only_if_no_message_went_for_it()
    {
        var r = Rec();
        r.Deliver(PaymentMethod.Cash, Today, MaintenancePolicy.Default, []);
        Assert.Equal(new[] { "teslim", "serbest" }, MessageTemplates.Applicable(r, []));
        Assert.Equal(new[] { "serbest" }, MessageTemplates.Applicable(r, ["hazir"]));   // HERHANGİ bir mesaj gittiyse
    }

    [Fact]
    public void Delivered_maintenance_with_pending_reminder_suggests_reminder()
    {
        var r = Rec("Periyodik bakım");
        r.SetKm("86400");
        r.Deliver(PaymentMethod.Cash, Today, MaintenancePolicy.Default, []);
        Assert.Contains("hatirlatma", MessageTemplates.Applicable(r, ["teslim"]));
        r.MarkReminderSent();
        Assert.DoesNotContain("hatirlatma", MessageTemplates.Applicable(r, ["teslim"]));
    }

    [Fact]
    public void No_job_only_free_message()
        => Assert.Equal(new[] { "serbest" }, MessageTemplates.Applicable(null, []));

    // ---- Metinler ----

    [Fact]
    public void Waiting_and_ready_texts()
    {
        var c = Ctx(Rec());
        Assert.Equal("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) için parça bekliyoruz. Parça gelir gelmez işe başlayıp size haber vereceğiz." + Imza,
            MessageTemplates.Text("bekliyor", c));
        Assert.Equal("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) hazır, teslim alabilirsiniz." + Imza, MessageTemplates.Text("hazir", c));
        Assert.Equal("Mehmet Kaya Bey/Hanım, " + Imza, MessageTemplates.Text("serbest", c));
    }

    [Fact]
    public void Summary_text_carries_count_total_and_link()
    {
        var r = Rec();
        r.AddLine(ServiceLine.Part("Balata", 1200));
        r.AddLine(ServiceLine.Part("Yağ", 900, PartSupplier.Customer));   // sayılır, toplama girmez
        r.AddLine(ServiceLine.Labor("İşçilik", 500));
        var t = MessageTemplates.Text("ozet", Ctx(r));
        Assert.Equal("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) için 3 kalem işlem yapıldı.\nToplam: 1.700 ₺\n\n" +
                     "Ayrıntılı özet: tamiron.app/o/" + r.SummaryCode + Imza, t);
        Assert.NotNull(r.SummaryCode);   // link istenince kod doğar
    }

    [Fact]
    public void Summary_without_any_record_says_so_without_signature()
        => Assert.Equal("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) için henüz bir kayıt yok.", MessageTemplates.Text("ozet", Ctx(null)));

    [Fact]
    public void Delivered_text_has_total_link_and_thanks()
    {
        var r = Rec();
        r.AddLine(ServiceLine.Labor("İşçilik", 1234.5m));
        r.Deliver(PaymentMethod.Cash, Today, MaintenancePolicy.Default, []);
        var t = MessageTemplates.Text("teslim", Ctx(r));   // önce metin: link istenince kod doğar
        Assert.Equal("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) teslim edildi.\nToplam: 1.234,5 ₺\nAyrıntılı özet: tamiron.app/o/" +
                     r.SummaryCode + "\n\nBizi tercih ettiğiniz için teşekkür ederiz." + Imza, t);
    }

    [Fact]
    public void Delivered_text_with_zero_total_omits_total_line()
    {
        var r = Rec();
        r.Deliver(PaymentMethod.Cash, Today, MaintenancePolicy.Default, []);
        Assert.StartsWith("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) teslim edildi.\nAyrıntılı özet: ", MessageTemplates.Text("teslim", Ctx(r)));
    }

    [Fact]
    public void Reminder_text_tells_last_maintenance()
    {
        var b = Rec("Periyodik bakım");
        b.SetKm("86400");
        b.Deliver(PaymentMethod.Cash, new DateOnly(2026, 4, 20), MaintenancePolicy.Default, []);
        Assert.Equal("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) için bakım zamanı geldi. Son bakımı 20.04.2026 · 86.400 km idi. Uygun olduğunuzda bekleriz." + Imza,
            MessageTemplates.Text("hatirlatma", Ctx(b, b, b)));
        Assert.Equal("Mehmet Kaya Bey/Hanım, Fiat Egea (34 ABC 123) için bakım zamanı geldi. Uygun olduğunuzda bekleriz." + Imza,
            MessageTemplates.Text("hatirlatma", Ctx(null)));
    }

    [Fact]
    public void Ready_made_texts_and_firm_greeting()
    {
        Assert.Equal("Mehmet Kaya Bey/Hanım, Bayramınız mübarek olsun. Sağlıklı, huzurlu ve kazasız günler dileriz." + Imza,
            MessageTemplates.Text("bayram", Ctx(null)));
        var firma = new Vehicle { PlateNo = "06 FR 1", Brand = "Ford", Model = "Transit", OwnerName = "Demir Nakliyat Ltd. Şti.", OwnerPhone = "05330000000" };
        Assert.Equal("Demir Nakliyat Ltd. Şti., nasılsınız? Aracınızla ilgili bir ihtiyacınız olursa bekleriz; iyi günler dileriz." + Imza,
            MessageTemplates.Text("selam", new MessageContext(firma, firma.DefaultTarget(), Dukkan, null, null, null)));
        Assert.Equal("Merhaba, Bizi tercih ettiğiniz için teşekkür ederiz. İyi günlerde kullanın." + Imza,
            MessageTemplates.Text("tesekkur", new MessageContext(Egea, null, Dukkan, null, null, null)));
    }

    [Fact]
    public void Names_and_unknown_template()
    {
        Assert.Equal("İşin özeti", MessageTemplates.Name("ozet"));
        Assert.Equal("Bayram mesajı", MessageTemplates.Name("bayram"));
        Assert.Equal("message.unknown_template", Assert.Throws<DomainException>(() => MessageTemplates.Text("yok", Ctx(null))).Code);
    }
}
