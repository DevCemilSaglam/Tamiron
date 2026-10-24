using Tamiron.Ui.Core.Ses;

namespace Tamiron.Ui.Tests;

/// <summary>
/// F7 · "2 bin aldım" (K-078.7) — tohum dünyasının KURAMADIĞI iki durum. Korpus prototipin tohumuyla
/// üretilir; orada veresiyeli tek araç (06 TMR 07) hem veresiyesi son kaydı hem plakası 0'la biten araç.
/// Mutasyonla bulundu: bu iki kural bozulsa korpus görmüyordu. Beklenen değerler prototipin kendi
/// çıktısı (aynı özel dünyayla Islem.cozumle, 24.10).
/// </summary>
public class SesOdemeAlTests
{
    static SesKayit Kayit(string id, string plaka, decimal kalan, int odeme)
        => new(id, plaka, Teslim: true, Satirlar: ["İş"], OdemeYolu: kalan > 0 ? "almadim" : "nakit", Kalan: kalan, OdemeSayisi: odeme);

    [Fact]
    public void Plate_targets_the_vehicles_record_with_remaining_even_if_it_is_not_the_last()
    {
        // Veresiye (8) önce, ödenmiş iş (99) sonra: hedef 8 — prototip veresiyeleri süzüp sonuncuyu alır.
        var dunya = new SesDunya([new SesArac("06 TMR 07", "Fiat", "Egea")],
                                 [Kayit("8", "06 TMR 07", 3450m, 0), Kayit("99", "06 TMR 07", 0m, 1)]);
        var o = new SesCozumleyici(dunya).Cozumle("06 TMR 07 için 2 bin aldım", "genel");
        Assert.Equal(("odeme-al", "8", 2000m), (o["tur"]!.GetValue<string>(), o["kayit"]!.GetValue<string>(), o["tutar"]!.GetValue<decimal>()));
    }

    [Fact]
    public void Plate_digits_are_not_the_amount()
    {
        var dunya = new SesDunya([new SesArac("34 VR 123", "Renault", "Clio", "Veli")], [Kayit("98", "34 VR 123", 900m, 0)]);
        var cz = new SesCozumleyici(dunya);
        // "34 VR 123 aldım": plakanın 123'ü fiilden hemen önce — tutar DEĞİL (prototip: servis-ac).
        Assert.Equal("servis-ac", cz.Cozumle("34 VR 123 aldım", "genel")["tur"]!.GetValue<string>());
        var o = cz.Cozumle("34 VR 123 için 300 aldım", "genel");
        Assert.Equal(("odeme-al", 300m), (o["tur"]!.GetValue<string>(), o["tutar"]!.GetValue<decimal>()));
    }
}
