using System.Globalization;

namespace Tamiron.Domain;

/// <summary>
/// Müşteri — ayrı tablo DEĞİL, araçtan TÜRETİLİR (VERI-MODELI: "Customer YOK", A-M1).
/// Bir sahip birden çok araca sahipse tek satır olur, araçları altında toplanır.
/// Anahtar: katlanmış ad + "|" + telefon (prototip <c>norm(ad) + '|' + tel</c>).
/// Ad değişirse anahtar değişir — bu bir hata değil, bir durum (detay 404 döner).
/// </summary>
public sealed class Customer
{
    public string Key { get; }
    public string Name { get; }
    public string? Phone { get; }
    public IReadOnlyList<Vehicle> Vehicles { get; }

    internal Customer(string key, string name, string? phone, IReadOnlyList<Vehicle> vehicles)
        => (Key, Name, Phone, Vehicles) = (key, name, phone, vehicles);

    /// <summary>Satırda gösterilen araç: serviste olan ilk, yoksa ilki. Mesaj da ona gider.</summary>
    public Vehicle DisplayVehicle(Func<Vehicle, bool> inShop) => Vehicles.FirstOrDefault(inShop) ?? Vehicles[0];

    /// <summary>Ticari ileti izni — MÜŞTERİNİN, aracın değil; herhangi bir aracında yazılıysa vardır.</summary>
    public DateOnly? ConsentAt => Vehicles.Select(v => v.MarketingConsentAt).FirstOrDefault(d => d is not null);

    /// <summary>
    /// İzin bütün araçlara BİRLİKTE yazılır. Tarih ispattır: zaten izni olana ikinci
    /// "evet" eski tarihi EZMEZ; kaldırınca hepsi boşalır (K-071.3).
    /// </summary>
    public void SetConsent(bool granted, DateOnly today)
    {
        var existing = ConsentAt;
        foreach (var v in Vehicles)
            v.MarketingConsentAt = granted ? existing ?? today : null;
    }
}

public static class Customers
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public static string KeyOf(string name, string? phone) => ServiceTypeGuess.Fold(name).Trim() + "|" + (phone ?? "");

    /// <summary>Sahibi yazılmamış araç listede yok. Sıra: ad, Türkçe alfabe.</summary>
    public static List<Customer> From(IEnumerable<Vehicle> vehicles)
        => vehicles.Where(v => !string.IsNullOrWhiteSpace(v.OwnerName))
            .GroupBy(v => KeyOf(v.OwnerName!, v.OwnerPhone))
            .Select(g => new Customer(g.Key, g.First().OwnerName!.Trim(), g.First().OwnerPhone, g.ToList()))
            .OrderBy(c => c.Name, StringComparer.Create(Tr, ignoreCase: false))
            .ToList();
}
