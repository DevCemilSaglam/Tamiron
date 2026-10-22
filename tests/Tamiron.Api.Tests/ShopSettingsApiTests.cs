using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Accounting;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Shops;

namespace Tamiron.Api.Tests;

/// <summary>
/// C5 · Dükkân ayarları — prototip 99-ayarlar: bakım periyodu (yalnız 3 · 6 · 12 ay) ve
/// "alış fiyatı sorulsun mu" (A-M43). Ad/adres/telefon okunur (özet, imza); gizlilik
/// notu bir kez gösterilir (A-M33).
/// </summary>
public class ShopSettingsApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Read_defaults_and_change_period_which_reminders_then_use()
    {
        var api = f.For(ApiFactory.ShopB);
        var s = await Read<ShopSettings>(await api.GetAsync("/api/shop"));
        Assert.Equal(("Öteki Dükkân", 6, 10_000, false), (s.Name, s.MaintenanceMonths, s.MaintenanceKm, s.AskPartCost));

        s = await Read<ShopSettings>(await api.PatchAsJsonAsync("/api/shop", new { maintenanceMonths = 12 }, Json));
        Assert.Equal(12, s.MaintenanceMonths);
        Assert.False(s.AskPartCost);                               // dokunulmadı

        // Teslimdeki hatırlatma dükkânın YENİ periyodunu kullanır.
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 AYR 01", complaint = "bakım" }, Json));
        var d = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
        Assert.Equal(f.Clock.Today.AddMonths(12), d.ReminderDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(24)]
    public async Task Only_3_6_12_months(int months)
    {
        var res = await f.For(ApiFactory.ShopB).PatchAsJsonAsync("/api/shop", new { maintenanceMonths = months }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
        Assert.Equal("shop.period_invalid", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Ask_part_cost_reaches_the_profit_screen()
    {
        var api = f.For(ApiFactory.ShopA);
        await Read<ShopSettings>(await api.PatchAsJsonAsync("/api/shop", new { askPartCost = true }, Json));
        Assert.True((await Read<JobProfitList>(await api.GetAsync("/api/profit?period=Year"))).AskPartCost);
        // Yalnız periyot gönderilince alış sorusu AÇIK kalır (null = dokunma) — kapalıyken
        // sınansaydı "false'a çevir" hatası görünmezdi.
        var s = await Read<ShopSettings>(await api.PatchAsJsonAsync("/api/shop", new { maintenanceMonths = 3 }, Json));
        Assert.True(s.AskPartCost);
    }

    [Fact]
    public async Task Privacy_notice_is_marked_once_first_time_kept()
    {
        var api = f.For(ApiFactory.ShopA);
        var ilk = await Read<ShopSettings>(await api.PostAsync("/api/shop/privacy-notice-shown", null));
        Assert.NotNull(ilk.PrivacyNoticeShownAt);
        var ikinci = await Read<ShopSettings>(await api.PostAsync("/api/shop/privacy-notice-shown", null));
        Assert.Equal(ilk.PrivacyNoticeShownAt, ikinci.PrivacyNoticeShownAt);
    }
}
