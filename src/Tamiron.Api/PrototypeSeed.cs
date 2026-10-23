using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api;

/// <summary>
/// Prototipin tohum verisini (02-veri.js: ARACLAR · KAYITLAR · GIDERLER, araçla JSON'a dökülmüş)
/// bir dükkâna yazar — TEK eşleyici (D16, K-076.15). İki kullanıcı: geliştirme tohumu (DevSeed)
/// ve muhasebe eşdeğerlik testi. Eşdeğerlik testi bu eşleyiciyi 0 farkla ölçtüğü için geliştirme
/// tohumunun rakamları da prototiple aynıdır.
/// </summary>
public static class PrototypeSeed
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    /// <param name="shiftDays">Bütün tarihler bu kadar kaydırılır: geliştirmede prototipin "bugün"ü
    /// gerçek bugüne denk gelsin (yoksa "bugünkü kasa" hep boş görünür). Testte 0.</param>
    /// <param name="keepOpenedOn">Kaydın açılış günü prototiptekiyle aynı olsun (geliştirme). Kapalıyken
    /// oluşturulma anı yalnız SIRAYI taşır (eşdeğerlik testi: CSV'de eşit günlerin sırası).</param>
    /// <returns>Prototip kayıt kimliği → API kaydı.</returns>
    public static async Task<Dictionary<int, Guid>> WriteAsync(TamironDbContext db, Guid shopId, JsonElement seed, int shiftDays = 0, bool keepOpenedOn = false)
    {
        DateOnly D(string s) => DateOnly.ParseExact(s, "dd.MM.yyyy", Tr).AddDays(shiftDays);
        DateOnly? Dn(JsonElement o, string k) => Str(o, k) is { } s ? D(s) : null;

        var vehicles = new Dictionary<string, Guid>();
        foreach (var a in seed.GetProperty("araclar").EnumerateArray())
        {
            var v = new Vehicle
            {
                TenantId = shopId, PlateNo = a.GetProperty("plaka").GetString()!, Brand = Str(a, "marka"), Model = Str(a, "model"),
                Year = Str(a, "yil"), Engine = Str(a, "motor"), Vin = Str(a, "vin"), Color = Str(a, "renk"), FuelType = Str(a, "yakit"),
                EngineNo = Str(a, "motorNo"), Displacement = Str(a, "silindir"), PowerKw = Str(a, "guc"), Seats = Str(a, "koltuk"),
                FuelTankLiters = Str(a, "depo"), FirstRegistrationDate = Str(a, "ilkTescil") is { } it && DateOnly.TryParseExact(it, "dd.MM.yyyy", Tr, DateTimeStyles.None, out var d) ? d : null,
                OwnerName = Str(a, "sahipAd"), OwnerPhone = Str(a, "sahipTel"),
                ContactName = Str(a, "iletisimAd"), ContactPhone = Str(a, "iletisimTel"),
                TaxOrNationalId = Str(a, "tcVergiNo"), Address = Str(a, "adres"), TaxOffice = Str(a, "vergiDairesi"), Note = Str(a, "not"),
                MarketingConsentAt = a.TryGetProperty("iletiIzni", out var iz) && iz.ValueKind == JsonValueKind.Object ? Dn(iz, "tarih") : null,
            };
            db.Vehicles.Add(v);
            await db.SaveChangesAsync();
            vehicles[v.PlateNo] = v.Id;
        }

        // Kayıtlar prototipteki SIRAYLA, tek tek: oluşturulma anı sırayı taşır.
        var ids = new Dictionary<int, Guid>();
        foreach (var k in seed.GetProperty("kayitlar").EnumerateArray())
        {
            var r = new ServiceRecord(shopId, vehicles[k.GetProperty("plaka").GetString()!], Str(k, "sikayet") ?? "", Type(k.GetProperty("tur").GetString()!));
            r.SetKm(Str(k, "km"));
            foreach (var l in k.GetProperty("satirlar").EnumerateArray())
            {
                var ad = l.GetProperty("ad").GetString()!;
                var fiyat = l.GetProperty("fiyat").GetDecimal();
                decimal? alis = l.TryGetProperty("alis", out var al) && al.ValueKind == JsonValueKind.Number ? al.GetDecimal() : null;
                r.AddLine(l.GetProperty("tip").GetString() == "parca"
                    ? ServiceLine.Part(ad, fiyat, Str(l, "kimGetirdi") == "musteri" ? PartSupplier.Customer : PartSupplier.Shop, alis)
                    : ServiceLine.Labor(ad, fiyat));
            }
            if (Bool(k, "parcaBekliyor"))
            {
                r.SetWaitingForPart(true, Dn(k, "bekliyorTarihi") ?? D(Str(k, "acilis")!));
                r.SetShowWaitingToCustomer(Bool(k, "bekliyorMusteriye"));
            }
            if (Dn(k, "teslimTarihi") is { } teslim)
            {
                var yol = Method(k.GetProperty("odemeYolu").GetString()!);
                if (Dn(k, "tahsilTarihi") is { } tahsil)
                {
                    r.Deliver(PaymentMethod.Unpaid, teslim, MaintenancePolicy.Default, []);
                    r.CollectPayment(yol, tahsil);
                }
                else r.Deliver(yol, teslim, MaintenancePolicy.Default, []);
                // Hatırlatma prototipte YAZILI (tohumun kendi değeri) — hesaplanan değil.
                r.ReminderDate = Dn(k, "hatirlatmaTarihi");
                r.ReminderSent = Bool(k, "hatirlatmaGonderildi");
            }
            db.ServiceRecords.Add(r);
            await db.SaveChangesAsync();
            if (keepOpenedOn && Str(k, "acilis") is { } acilis)
            {
                // Türkiye öğlesi (UTC 09:00) + sıra: aynı günün kayıtları sırasını korur.
                r.DateTimeCreated = D(acilis).ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc).AddMilliseconds(ids.Count);
                await db.SaveChangesAsync();
            }
            ids[k.GetProperty("id").GetInt32()] = r.Id;
        }

        foreach (var g in seed.GetProperty("giderler").EnumerateArray())
        {
            db.Expenses.Add(Expense.Create(shopId, g.GetProperty("aciklama").GetString()!, g.GetProperty("tutar").GetDecimal(), D(g.GetProperty("tarih").GetString()!)));
            await db.SaveChangesAsync();
        }
        return ids;
    }

    /// <summary>Prototipin "bugün"ünü <paramref name="today"/>'e taşıyan kaydırma.</summary>
    public static int ShiftTo(JsonElement seed, DateOnly today)
        => today.DayNumber - DateOnly.ParseExact(seed.GetProperty("bugun").GetString()!, "dd.MM.yyyy", Tr).DayNumber;

    private static string? Str(JsonElement o, string k)
        => o.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String && v.GetString()!.Length > 0 ? v.GetString() : null;

    private static bool Bool(JsonElement o, string k) => o.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.True;

    private static ServiceType Type(string t) => t switch { "bakim" => ServiceType.Maintenance, "ikisi" => ServiceType.Both, _ => ServiceType.Repair };

    private static PaymentMethod Method(string y) => y switch
    {
        "nakit" => PaymentMethod.Cash, "kart" => PaymentMethod.Card, "havale" => PaymentMethod.Transfer, "almadim" => PaymentMethod.Unpaid,
        // Bilinmeyen ödeme yolu sessizce "alınmadı" olmasın: tohum bozuksa açılışta patlasın.
        _ => throw new ArgumentException("Bilinmeyen ödeme yolu: " + y)
    };
}
