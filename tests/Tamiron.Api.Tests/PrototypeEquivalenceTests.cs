using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.Accounting;
using Tamiron.Application.Expenses;
using Tamiron.Application.Home;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// C10 · Muhasebe Faz 1'in "bitti" tanımı (MUHASEBE-KAPSAM §5): prototipin rakamları = API'nin
/// rakamları, 0 FARK. Beklenen değerler PROTOTİPİN KENDİ FONKSİYONLARINDAN gelir —
/// tools/muhasebe-esdeger.js 02-veri.js'i Node'da çalıştırıp Esdeger/prototip-muhasebe.json'a
/// döker. Bu test aynı tohumu yeni bir dükkâna yazar, prototipin "bugün"üyle (03.09.2026)
/// kasa · teslim · veresiye · gider · parça alışı · iş başına fark · CSV (bayt bayt) karşılaştırır.
/// </summary>
public class PrototypeEquivalenceTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    static readonly Dictionary<string, PeriodKind> Periods = new() { ["bugun"] = PeriodKind.Today, ["ay"] = PeriodKind.Month, ["yil"] = PeriodKind.Year };

    static DateOnly Tarih(string s) => DateOnly.ParseExact(s, "dd.MM.yyyy");
    static PaymentMethod Yol(string s) => s switch
    {
        "nakit" => PaymentMethod.Cash, "kart" => PaymentMethod.Card, "havale" => PaymentMethod.Transfer, "almadim" => PaymentMethod.Unpaid,
        _ => throw new ArgumentException(s)
    };
    static ServiceType Tur(string s) => s switch { "bakim" => ServiceType.Maintenance, "ikisi" => ServiceType.Both, _ => ServiceType.Repair };
    static string? Str(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String && v.GetString()!.Length > 0 ? v.GetString() : null;

    public static IEnumerable<object[]> Scenarios()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Esdeger", "prototip-muhasebe.json");
        foreach (var s in JsonDocument.Parse(File.ReadAllText(path)).RootElement.EnumerateArray())
            yield return [s.GetProperty("ad").GetString()!];
    }

    static JsonElement Load(string ad)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Esdeger", "prototip-muhasebe.json");
        return JsonDocument.Parse(File.ReadAllText(path)).RootElement.EnumerateArray().Single(s => s.GetProperty("ad").GetString() == ad).Clone();
    }

    /// <summary>Tohumu yeni bir dükkâna yazar; prototip kayıt kimliği → API kaydı eşlemesini ve oturum anahtarını döner.</summary>
    async Task<(HttpClient api, Dictionary<int, Guid> ids)> Seed(JsonElement s)
    {
        var shop = Guid.NewGuid();
        var ids = new Dictionary<int, Guid>();
        string token;
        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = shop;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            db.Shops.Add(new Shop(shop) { Name = "Eşdeğerlik " + s.GetProperty("ad").GetString() });
            await db.SaveChangesAsync();

            var vehicles = new Dictionary<string, Guid>();
            foreach (var a in s.GetProperty("araclar").EnumerateArray())
            {
                var v = new Vehicle
                {
                    TenantId = shop, PlateNo = a.GetProperty("plaka").GetString()!, Brand = Str(a, "marka"), Model = Str(a, "model"),
                    Year = Str(a, "yil"), Engine = Str(a, "motor"), OwnerName = Str(a, "sahipAd"), OwnerPhone = Str(a, "sahipTel"),
                    ContactName = Str(a, "iletisimAd"), ContactPhone = Str(a, "iletisimTel")
                };
                db.Vehicles.Add(v);
                await db.SaveChangesAsync();
                vehicles[v.PlateNo] = v.Id;
            }

            // Kayıtlar prototipteki SIRAYLA, tek tek: oluşturulma anı sırayı taşır (CSV'de eşit günlerin sırası).
            foreach (var k in s.GetProperty("kayitlar").EnumerateArray())
            {
                var r = new ServiceRecord(shop, vehicles[k.GetProperty("plaka").GetString()!], Str(k, "sikayet") ?? "", Tur(k.GetProperty("tur").GetString()!));
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
                if (Str(k, "teslimTarihi") is { } teslim)
                {
                    var yol = Yol(k.GetProperty("odemeYolu").GetString()!);
                    if (Str(k, "tahsilTarihi") is { } tahsil)
                    {
                        r.Deliver(PaymentMethod.Unpaid, Tarih(teslim), MaintenancePolicy.Default, []);
                        r.CollectPayment(yol, Tarih(tahsil));
                    }
                    else r.Deliver(yol, Tarih(teslim), MaintenancePolicy.Default, []);
                }
                db.ServiceRecords.Add(r);
                await db.SaveChangesAsync();
                ids[k.GetProperty("id").GetInt32()] = r.Id;
            }

            foreach (var g in s.GetProperty("giderler").EnumerateArray())
            {
                db.Expenses.Add(Expense.Create(shop, g.GetProperty("aciklama").GetString()!, g.GetProperty("tutar").GetDecimal(), Tarih(g.GetProperty("tarih").GetString()!)));
                await db.SaveChangesAsync();
            }

            var user = User.Create(shop, "0532 999 " + Random.Shared.Next(10, 99) + " " + Random.Shared.Next(10, 99));
            db.Users.Add(user);
            var (session, tok) = Session.Open(shop, user.Id, "eşdeğerlik", f.Clock.UtcNow);
            db.Sessions.Add(session);
            await db.SaveChangesAsync();
            token = tok;
        }
        var api = f.CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (api, ids);
    }

    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task Api_numbers_equal_prototype_numbers(string scenario)
    {
        var s = Load(scenario);
        f.Clock.Today = Tarih(s.GetProperty("bugun").GetString()!);   // prototipin "bugün"ü: 03.09.2026
        try
        {
            var (api, ids) = await Seed(s);

            var home = (await api.GetFromJsonAsync<HomeSummary>("/api/home", Json))!;
            Assert.Equal(s.GetProperty("bugunTahsilat").GetDecimal(), home.CollectedToday);
            Assert.Equal(s.GetProperty("veresiye").GetDecimal(), home.Receivable);

            foreach (var (ad, period) in Periods)
            {
                var d = s.GetProperty("donemler").GetProperty(ad);
                var cash = (await api.GetFromJsonAsync<CashSummary>($"/api/cash?period={period}", Json))!;
                Assert.Equal((d.GetProperty("nakit").GetDecimal(), d.GetProperty("kart").GetDecimal(), d.GetProperty("havale").GetDecimal()),
                             (cash.Cash, cash.Card, cash.Transfer));
                Assert.Equal(d.GetProperty("tahsilat").GetDecimal(), cash.Collected);
                Assert.Equal(d.GetProperty("teslim").GetInt32(), cash.DeliveredCount);
                Assert.Equal(s.GetProperty("veresiye").GetDecimal(), cash.Receivable);

                Assert.Equal(d.GetProperty("gider").GetDecimal(), (await api.GetFromJsonAsync<ExpenseList>($"/api/expenses?period={period}", Json))!.Total);
                Assert.Equal(d.GetProperty("parcaAlisi").GetDecimal(), (await api.GetFromJsonAsync<PartPurchaseList>($"/api/part-purchases?period={period}", Json))!.Total);

                var profit = (await api.GetFromJsonAsync<JobProfitList>($"/api/profit?period={period}", Json))!;
                var expected = d.GetProperty("isKari").EnumerateArray().ToList();
                Assert.Equal(expected.Count, profit.Items.Count);
                foreach (var e in expected)
                {
                    var item = profit.Items.Single(i => i.RecordId == ids[e.GetProperty("id").GetInt32()]);
                    Assert.Equal((e.GetProperty("alinan").GetDecimal(), e.GetProperty("parca").GetDecimal(), e.GetProperty("disarida").GetDecimal(),
                                  e.GetProperty("bilinmeyen").GetInt32(), e.GetProperty("fark").GetDecimal()),
                                 (item.Received, item.PartsCost, item.Outside, item.UnknownCount, item.Difference));
                }

                // CSV — BAYT BAYT: muhasebeciye giden dosya prototipinkiyle aynı olmalı.
                var bytes = await api.GetByteArrayAsync($"/api/export/cash.csv?period={period}");
                Assert.Equal(d.GetProperty("csv").GetString(), Encoding.UTF8.GetString(bytes));
            }
        }
        finally
        {
            f.Clock.Today = new DateOnly(2026, 10, 22);
        }
    }
}
