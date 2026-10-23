using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// D3 · Müşteri özet sayfası (K-023). Beklenen davranış prototipin <c>ozetGovdeHtml</c>'i
/// (03-nesne.js): durum cümlesi, müşteri parçasında rakam yok, alış hiç yok.
/// </summary>
public class CustomerSummaryTests
{
    static readonly DateOnly Today = new(2026, 10, 22);
    static ServiceRecord Rec() => new(Guid.NewGuid(), Guid.NewGuid(), "Fren");

    [Fact]
    public void In_shop_sentence()
        => Assert.Equal("Aracınız serviste. Hazır olduğunda haber vereceğiz.", CustomerSummary.StatusText(Rec()));

    [Fact]
    public void Waiting_for_part_is_told_only_if_the_usta_chose_so()
    {
        var r = Rec();
        r.SetWaitingForPart(true, Today);
        Assert.Equal("Aracınız serviste. Hazır olduğunda haber vereceğiz.", CustomerSummary.StatusText(r));   // A-M15
        r.SetShowWaitingToCustomer(true);
        Assert.Equal("Aracınız serviste, parça bekleniyor. Parça gelir gelmez işe başlayıp size haber vereceğiz.", CustomerSummary.StatusText(r));
    }

    [Theory]
    [InlineData(PaymentMethod.Cash, "Aracınız 22.10.2026 tarihinde teslim edildi. Ödeme: Nakit.")]
    [InlineData(PaymentMethod.Transfer, "Aracınız 22.10.2026 tarihinde teslim edildi. Ödeme: Havale.")]
    [InlineData(PaymentMethod.Unpaid, "Aracınız 22.10.2026 tarihinde teslim edildi. Ödeme alınmadı.")]
    public void Delivered_sentence_tells_the_payment(PaymentMethod m, string expected)
    {
        var r = Rec();
        r.Deliver(m, Today, MaintenancePolicy.Default, []);
        Assert.Equal(expected, CustomerSummary.StatusText(r));
    }

    [Fact]
    public void Customer_part_and_unpriced_line_show_no_amount()
    {
        Assert.Null(CustomerSummary.Amount(ServiceLine.Part("Yağ", 900, PartSupplier.Customer)));   // K-037: rakam yok
        Assert.Null(CustomerSummary.Amount(ServiceLine.Part("Balata", 0)));                         // fiyatı gelmemiş: "—"
        Assert.Equal(500m, CustomerSummary.Amount(ServiceLine.Labor("İşçilik", 500)));
    }

    [Fact]
    public void Code_is_long_random_and_unambiguous()
    {
        var codes = Enumerable.Range(0, 200).Select(_ => CustomerSummary.NewCode()).ToList();
        Assert.Equal(200, codes.Distinct().Count());
        Assert.All(codes, c =>
        {
            Assert.Equal(16, c.Length);
            Assert.DoesNotContain(c, ch => "01ilo".Contains(ch) || char.IsUpper(ch));   // okunurken karışan harf yok
        });
    }

    [Fact]
    public void Record_keeps_its_code_once_given()
    {
        var r = Rec();
        Assert.Null(r.SummaryCode);
        var c = r.EnsureSummaryCode();
        Assert.Equal(c, r.EnsureSummaryCode());
        Assert.Equal("tamiron.app/o/" + c, CustomerSummary.Link(c));
    }
}
