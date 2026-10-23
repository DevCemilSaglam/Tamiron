using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// D2 · Parça sorma (VERI-MODELI §5–6). Beklenen davranış prototipin
/// <c>psParcaListesi</c>, mesaj gövdesi ve <c>psGonder</c> kapıları (40-parca-sor.html).
/// </summary>
public class PartQuestionTests
{
    static readonly Vehicle Egea = new() { PlateNo = "34 ABC 123", Brand = "Fiat", Model = "Egea", Year = "2021", Engine = "1.4 Fire" };

    [Fact]
    public void Only_shop_parts_without_price_are_asked()
    {
        var lines = new[]
        {
            ServiceLine.Part("Balata", 0),                                   // sorulur
            ServiceLine.Part("Disk", 1500),                                  // fiyatı belli
            ServiceLine.Part("Yağ", 0, PartSupplier.Customer),              // müşteri getiriyor
            ServiceLine.Labor("İşçilik", 0),                                 // işçilik
            ServiceLine.Part("Filtre", 0, cost: 300),                        // alışı belli ama fiyatı yok: yine sorulur
        };
        Assert.Equal(new[] { "Balata", "Filtre" }, PartQuestion.AskableLines(lines).Select(l => l.Name));
    }

    [Fact]
    public void Message_matches_the_prototype_body()
    {
        var text = PartQuestion.Compose(Egea, [ServiceLine.Part("Balata", 0), ServiceLine.Part("Disk", 0)]);
        Assert.Equal("Fiat Egea 2021 · 1.4 Fire\n34 ABC 123\n\n· Balata\n· Disk\n\nFiyat verebilir misiniz?", text);
    }

    [Fact]
    public void Vin_goes_into_the_message_and_empty_identity_parts_are_skipped()
    {
        var v = new Vehicle { PlateNo = "06 XY 42", Brand = "Renault", Vin = "VF1RFB00858123456" };
        Assert.Equal("Renault\nŞase: VF1RFB00858123456\n06 XY 42\n\n· Balata\n\nFiyat verebilir misiniz?",
            PartQuestion.Compose(v, [ServiceLine.Part("Balata", 0)]));
    }

    [Fact]
    public void Gates_speak_in_prototype_order()
    {
        var balata = new[] { ServiceLine.Part("Balata", 0) };
        // 1 · sorulacak parça yok
        Assert.Equal("price_request.no_parts", Assert.Throws<DomainException>(() => PartQuestion.EnsureCanAsk(Egea, [], 1)).Code);
        // 2 · araç tanınmıyor (K-068) — eksik alanlar söylenir
        var bos = new Vehicle { PlateNo = "34 ABC 123", Brand = "Fiat" };
        var e = Assert.Throws<DomainException>(() => PartQuestion.EnsureCanAsk(bos, balata, 0));
        Assert.Equal("price_request.vehicle_identity", e.Code);
        Assert.Contains("model, yıl, motor", e.Message);
        // 3 · kime soracağı seçilmedi
        Assert.Equal("price_request.no_supplier", Assert.Throws<DomainException>(() => PartQuestion.EnsureCanAsk(Egea, balata, 0)).Code);
        PartQuestion.EnsureCanAsk(Egea, balata, 2);
    }

    [Fact]
    public void Reply_writes_supplier_price_as_cost_and_marks_the_source()
    {
        var line = ServiceLine.Part("Balata", 0);
        var reply = PriceReply.Create(Guid.NewGuid(), "balata 850 bosch 0986", 850, "Bosch", "0986", DateTime.UtcNow);
        line.ApplyReply(reply, customerPrice: 1200);
        Assert.Equal((850m, 1200m, "Bosch", "0986", reply.Id), (line.Cost!.Value, line.Price, line.PartBrand, line.PartCode, line.SelectedReplyId));
    }

    [Fact]
    public void Reply_without_price_keeps_what_the_line_already_has()
    {
        var line = ServiceLine.Part("Balata", 900, cost: 600);
        line.PartBrand = "TRW";
        line.ApplyReply(PriceReply.Create(Guid.NewGuid(), "yarın bakarım", null, null, null, DateTime.UtcNow), customerPrice: null);
        Assert.Equal((600m, 900m, "TRW"), (line.Cost!.Value, line.Price, line.PartBrand));
    }

    [Fact]
    public void Empty_or_negative_reply_is_rejected()
    {
        Assert.Equal("reply.empty", Assert.Throws<DomainException>(() => PriceReply.Create(Guid.NewGuid(), "  ", null, null, null, DateTime.UtcNow)).Code);
        Assert.Equal("line.negative_amount", Assert.Throws<DomainException>(() => PriceReply.Create(Guid.NewGuid(), "x", -1, null, null, DateTime.UtcNow)).Code);
        var line = ServiceLine.Part("Balata", 0);
        Assert.Equal("line.negative_amount", Assert.Throws<DomainException>(() =>
            line.ApplyReply(PriceReply.Create(Guid.NewGuid(), "x", 5, null, null, DateTime.UtcNow), -3)).Code);
    }

    [Fact]
    public void Reply_cannot_be_applied_to_labor()
    {
        var labor = ServiceLine.Labor("İşçilik", 0);
        Assert.Equal("reply.not_a_part", Assert.Throws<DomainException>(() =>
            labor.ApplyReply(PriceReply.Create(Guid.NewGuid(), "500", 500, null, null, DateTime.UtcNow), null)).Code);
    }
}
