using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.Lists;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// D16 · Tam geliştirme tohumu (K-076.15): prototipin 4 aracı, 8 kaydı, 2 gideri; tarihler
/// prototipin "bugün"ü bugüne kayacak şekilde. Rakamlar prototiple aynı — "bugünkü kasa" boş değil.
/// </summary>
public class DevFullSeedTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    static JsonElement Proto => JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Esdeger", "prototip-muhasebe.json")))
        .RootElement.EnumerateArray().Single(s => s.GetProperty("ad").GetString() == "tohum").Clone();
    static DateOnly P(string s) => DateOnly.ParseExact(s, "dd.MM.yyyy", Tr);

    [Fact]
    public async Task Full_seed_is_the_prototype_shifted_to_today()
    {
        var shop = Guid.NewGuid();
        string token;
        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = shop;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            await DevSeed.FullAsync(db, shop, f.Clock.Today);
            var u = User.Create(shop, "0532 888 " + Random.Shared.Next(10, 99) + " " + Random.Shared.Next(10, 99));
            db.Users.Add(u);
            var (sess, tok) = Session.Open(shop, u.Id, "tohum", f.Clock.UtcNow);
            db.Sessions.Add(sess);
            await db.SaveChangesAsync();
            token = tok;

            Assert.Equal((4, 8, 2), (await db.Vehicles.CountAsync(), await db.ServiceRecords.CountAsync(), await db.Expenses.CountAsync()));
            var shift = f.Clock.Today.DayNumber - P(Proto.GetProperty("bugun").GetString()!).DayNumber;
            var mehmet = await db.Vehicles.SingleAsync(v => v.PlateNo == "34 ABC 123");
            Assert.Equal(new DateOnly(2026, 6, 14).AddDays(shift), mehmet.MarketingConsentAt);   // izin de kayar
            Assert.Equal("VF1RFB00858123456", mehmet.Vin);                                      // ruhsat alanları da geldi
        }

        var api = f.CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Prototipin "bugün" tahsilatı = API'nin bugünkü kasası (kaydırma doğru).
        var cash = await api.GetFromJsonAsync<JsonElement>("/api/cash?period=Today");
        Assert.Equal(Proto.GetProperty("bugunTahsilat").GetDecimal(), cash.GetProperty("collected").GetDecimal());
        Assert.True(cash.GetProperty("collected").GetDecimal() > 0);

        // Açılış günleri prototiptekiyle aynı (kaydırılmış): "kaç gündür serviste" doğru görünsün.
        var list = (await api.GetFromJsonAsync<RecordList>("/api/records", new JsonSerializerOptions(JsonSerializerDefaults.Web)
            { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }))!;
        var shiftDays = f.Clock.Today.DayNumber - P(Proto.GetProperty("bugun").GetString()!).DayNumber;
        var expected = Proto.GetProperty("kayitlar").EnumerateArray().Select(k => P(k.GetProperty("acilis").GetString()!).AddDays(shiftDays)).OrderBy(d => d);
        Assert.Equal(expected, list.Items.Select(i => i.OpenedOn).OrderBy(d => d));
        Assert.Equal(Proto.GetProperty("kayitlar").EnumerateArray().Count(k => k.GetProperty("parcaBekliyor").GetBoolean()),
            list.Items.Count(i => i.WaitingForPart));
    }
}
