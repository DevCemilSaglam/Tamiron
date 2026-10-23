using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B10 · Müşteri araçtan TÜRETİLİR (A-M1) — prototip <c>musteriler</c> / <c>musteriAraci</c> /
/// <c>musteriIzni</c> / <c>iletiIzniYaz</c>. Anahtar: katlanmış ad + telefon.
/// </summary>
public class CustomerTests
{
    static Vehicle V(string plate, string? owner, string? phone = null) => new() { PlateNo = plate, OwnerName = owner, OwnerPhone = phone };

    [Fact]
    public void Same_name_and_phone_is_one_customer_with_vehicles_in_order()
    {
        var list = Customers.From([V("34 A 1", "Mehmet Kaya", "0532 215 30 00"), V("34 A 2", "mehmet kaya ", "0532 215 30 00")]);
        var m = Assert.Single(list);
        Assert.Equal("Mehmet Kaya", m.Name);                       // ilk yazılış
        Assert.Equal(new[] { "34 A 1", "34 A 2" }, m.Vehicles.Select(v => v.PlateNo));
    }

    [Fact]
    public void Same_name_different_phone_are_two_customers_nameless_is_none()
    {
        var list = Customers.From([V("34 A 1", "Ali", "0532 000 00 01"), V("34 A 2", "Ali", "0532 000 00 02"), V("34 A 3", null), V("34 A 4", "  ")]);
        Assert.Equal(2, list.Count);
        Assert.NotEqual(list[0].Key, list[1].Key);
    }

    [Fact]
    public void Sorted_by_turkish_alphabet()
    {
        var list = Customers.From([V("1", "Çetin"), V("2", "Zeki"), V("3", "Can"), V("4", "İsmail"), V("5", "Ilgaz")]);
        Assert.Equal(new[] { "Can", "Çetin", "Ilgaz", "İsmail", "Zeki" }, list.Select(c => c.Name));
    }

    [Fact]
    public void Display_vehicle_is_the_one_in_shop_else_first()
    {
        var a = V("34 A 1", "Ali"); var b = V("34 A 2", "Ali");
        var c = Assert.Single(Customers.From([a, b]));
        Assert.Same(a, c.DisplayVehicle(inShop: _ => false));
        Assert.Same(b, c.DisplayVehicle(inShop: v => v == b));
    }

    [Fact]
    public void Consent_is_written_to_all_vehicles_and_read_from_any()
    {
        var a = V("34 A 1", "Ali"); var b = V("34 A 2", "Ali");
        var c = Assert.Single(Customers.From([a, b]));
        Assert.Null(c.ConsentAt);                                   // varsayılan YOK
        c.SetConsent(true, new DateOnly(2026, 10, 22), Guid.Empty);
        Assert.Equal((new DateOnly(2026, 10, 22), new DateOnly(2026, 10, 22)), (a.MarketingConsentAt, b.MarketingConsentAt));
        c.SetConsent(false, new DateOnly(2026, 10, 23), Guid.Empty);
        Assert.Null(a.MarketingConsentAt); Assert.Null(b.MarketingConsentAt);
    }

    [Fact]
    public void Granting_again_keeps_the_original_date()
    {
        // İspat: izin verildiği GÜN. Zaten izni olana ikinci "evet" tarihi ezmez.
        var a = V("34 A 1", "Ali");
        var c = Assert.Single(Customers.From([a]));
        c.SetConsent(true, new DateOnly(2026, 6, 14), Guid.Empty);
        c.SetConsent(true, new DateOnly(2026, 10, 22), Guid.Empty);
        Assert.Equal(new DateOnly(2026, 6, 14), a.MarketingConsentAt);
    }
}
