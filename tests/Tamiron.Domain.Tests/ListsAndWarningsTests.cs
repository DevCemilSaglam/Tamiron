using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B7 · Arama (prototip norm + indexOf), tür adları (TUR_AD) ve teslim eksikleri
/// (kEksikler: "eksikleri SÖYLER, yolu KAPATMAZ").
/// </summary>
public class ListsAndWarningsTests
{
    [Theory]
    [InlineData("şoför", "Şoför Ahmet", true)]
    [InlineData("SOFOR", "Şoför Ahmet", true)]          // Türkçe katlama iki yönde
    [InlineData("ismail", "İsmail Özdemir", true)]
    [InlineData("abc 1", "34 ABC 123", true)]
    [InlineData("", "her şey", true)]
    [InlineData("balata", "Fren diski", false)]
    public void Search_folds_turkish(string q, string text, bool expected) => Assert.Equal(expected, TextSearch.Matches(q, text));

    [Fact]
    public void Search_matches_any_part_ignoring_nulls()
        => Assert.True(TextSearch.Matches("demir", "06 TMR 07", null, "Demir Nakliyat"));

    [Theory]
    [InlineData(ServiceType.Repair, "Tamirat")]
    [InlineData(ServiceType.Maintenance, "Bakım")]
    [InlineData(ServiceType.Both, "Tamirat + Bakım")]
    public void Type_labels(ServiceType t, string label) => Assert.Equal(label, ServiceTypeGuess.Label(t));

    [Fact]
    public void Warnings_list_what_is_missing_customer_part_needs_no_price()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren");
        r.AddLine(ServiceLine.Part("Balata", 0));                                   // tutarı yok
        r.AddLine(ServiceLine.Part("Disk", 0, PartSupplier.Customer));               // müşteri getirdi: tutar beklenmez
        r.AddLine(ServiceLine.Labor("İşçilik", 500));
        r.SetWaitingForPart(true, new DateOnly(2026, 10, 22));
        Assert.Equal(new[]
        {
            "1 satırın tutarı yazılmamış",
            "parça hâlâ \"bekliyor\" işaretli",
            "telefon yok — özet gönderilemez"
        }, r.DeliveryWarnings(hasPhone: false));
    }

    [Fact]
    public void Delivered_record_has_no_warnings()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren");
        r.AddLine(ServiceLine.Part("Balata", 0));
        r.Deliver(PaymentMethod.Cash, new DateOnly(2026, 10, 22), MaintenancePolicy.Default, []);
        Assert.Empty(r.DeliveryWarnings(hasPhone: false));
    }

    [Fact]
    public void Contact_phone_counts_as_phone()
    {
        Assert.True(new Vehicle { ContactPhone = "0542 331 90 07" }.HasPhone);
        Assert.False(new Vehicle { OwnerName = "Ayşe Kaya" }.HasPhone);
    }
}
