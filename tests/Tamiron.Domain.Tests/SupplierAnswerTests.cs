using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// H6 · Parçacının kendi cevabı (K-091, VERI-MODELI §6b). Satır başına fiyat · marka · OEM kodu · not; hepsi boş
/// olamaz ("yok" da cevaptır — nota yazılır). Dükkân türü kayıtta seçilir; var olanlar tamirhanedir.
/// </summary>
public class SupplierAnswerTests
{
    static readonly Guid Parcaci = Guid.NewGuid();
    static readonly DateTime An = new(2026, 10, 28, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Answer_keeps_what_the_supplier_wrote()
    {
        var a = SupplierAnswer.Create(Parcaci, Guid.NewGuid(), Guid.NewGuid(), 1200, " Bosch ", " 0986494 ", null, An);
        Assert.Equal(1200m, a.Price);
        Assert.Equal("Bosch", a.Brand);
        Assert.Equal("0986494", a.Code);
        Assert.Null(a.Note);
        Assert.Equal(Parcaci, a.TenantId);   // parçacının verisi, parçacının kiracısında
    }

    [Fact]
    public void Not_available_is_an_answer_too()
    {
        var a = SupplierAnswer.Create(Parcaci, Guid.NewGuid(), Guid.NewGuid(), null, null, null, "Yok", An);
        Assert.Equal("Yok", a.Note);
        Assert.Null(a.Price);
    }

    [Fact]
    public void Empty_or_negative_answer_is_refused()
    {
        Assert.Equal("answer.empty", Assert.Throws<DomainException>(() =>
            SupplierAnswer.Create(Parcaci, Guid.NewGuid(), Guid.NewGuid(), null, " ", "", null, An)).Code);
        Assert.Equal("line.negative_amount", Assert.Throws<DomainException>(() =>
            SupplierAnswer.Create(Parcaci, Guid.NewGuid(), Guid.NewGuid(), -1, null, null, null, An)).Code);
    }

    [Fact]
    public void Answer_reads_as_one_line_for_the_shop()
    {
        // Ustanın kaydına düşen ham metin (VERI-MODELI §6: RawText kaynağı korur) — T-13 sırası.
        var a = SupplierAnswer.Create(Parcaci, Guid.NewGuid(), Guid.NewGuid(), 1200, "Bosch", "0986494", "yarın gelir", An);
        Assert.Equal("1.200 ₺ · Bosch · 0986494 · yarın gelir", a.Text);
    }

    [Fact]
    public void Shop_kind_is_chosen_at_sign_up_and_defaults_to_workshop()
    {
        Assert.Equal(ShopKind.Tamirhane, Shop.Register("Usta Oto", "0532 000 00 01").Kind);
        Assert.Equal(ShopKind.YedekParca, Shop.Register("Öz Parça", "0533 100 00 01", ShopKind.YedekParca).Kind);
        Assert.Equal(ShopKind.Tamirhane, new Shop().Kind);
    }
}
