using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.Auth;

namespace Tamiron.Api.Tests;

/// <summary>
/// K-074 · SMS koduyla giriş; oturum yalnız çıkışla biter. Dükkân OTURUMDAN gelir —
/// X-Tenant-Id başlığı artık hiçbir şey açmaz.
/// </summary>
public class AuthApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };

    string LastCode(string phone) => Regex.Match(f.Sms.Sent.Last(s => s.Phone == phone).Text, @"\b\d{6}\b").Value;

    async Task<string> Login(string phone, string device)
    {
        var anon = f.CreateClient();
        Assert.Equal(HttpStatusCode.Accepted, (await anon.PostAsJsonAsync("/api/auth/code", new { phone }, Json)).StatusCode);
        var res = await anon.PostAsJsonAsync("/api/auth/verify", new { phone, code = LastCode(VehiclePhone(phone)), deviceName = device }, Json);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<LoginResult>(Json))!.Token;
    }

    static string VehiclePhone(string raw) => Tamiron.Domain.VehicleFields.Phone(raw)!;

    HttpClient With(string token)
    {
        var c = f.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task Code_by_sms_then_session_that_does_not_expire()
    {
        var token = await Login("05320000001", "Ahmet'in telefonu");
        var sms = f.Sms.Sent.Last();
        Assert.Equal("0532 000 00 01", sms.Phone);
        Assert.Contains("Tamiron giriş kodun", sms.Text);

        var api = With(token);
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/api/records")).StatusCode);

        f.Clock.Today = f.Clock.Today.AddYears(1);   // bir yıl sonra: hâlâ içeride (K-074.2)
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/api/records")).StatusCode);
        f.Clock.Today = new DateOnly(2026, 10, 22);
    }

    [Fact]
    public async Task Logout_ends_this_device_logout_all_ends_every_device()
    {
        // Kendi numarası: üç giriş = üç kod, hız sınırının tam sınırı (testler birbirinin kotasını yemesin).
        var telefon = await Login("05320000005", "telefon");
        var tablet = await Login("05320000005", "tablet");
        var bilgisayar = await Login("05320000005", "bilgisayar");

        var oturumlar = await With(tablet).GetFromJsonAsync<List<SessionInfo>>("/api/auth/sessions", Json);
        Assert.Contains(oturumlar!, s => s.DeviceName == "tablet" && s.Current);
        Assert.Contains(oturumlar!, s => s.DeviceName == "telefon" && !s.Current);

        (await With(telefon).PostAsync("/api/auth/logout", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await With(telefon).GetAsync("/api/records")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await With(tablet).GetAsync("/api/records")).StatusCode);   // öteki cihaz içeride

        (await With(tablet).PostAsync("/api/auth/logout-all", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await With(tablet).GetAsync("/api/records")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await With(bilgisayar).GetAsync("/api/records")).StatusCode);
    }

    [Fact]
    public async Task Unknown_phone_gets_no_sms()
    {
        var before = f.Sms.Sent.Count;
        var res = await f.CreateClient().PostAsJsonAsync("/api/auth/code", new { phone = "05559998877" }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
        Assert.Equal("auth.unknown_phone", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(before, f.Sms.Sent.Count);
    }

    [Fact]
    public async Task Fourth_code_within_ten_minutes_is_refused()
    {
        var anon = f.CreateClient();
        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.Accepted, (await anon.PostAsJsonAsync("/api/auth/code", new { phone = "05320000002" }, Json)).StatusCode);
        var res = await anon.PostAsJsonAsync("/api/auth/code", new { phone = "05320000002" }, Json);
        Assert.Equal("auth.too_many_codes", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Wrong_codes_lock_the_code()
    {
        var anon = f.CreateClient();
        await anon.PostAsJsonAsync("/api/auth/code", new { phone = "05320000003" }, Json);
        var right = LastCode("0532 000 00 03");
        var wrong = right == "000000" ? "111111" : "000000";
        for (var i = 0; i < 5; i++)
            await anon.PostAsJsonAsync("/api/auth/verify", new { phone = "05320000003", code = wrong }, Json);
        var res = await anon.PostAsJsonAsync("/api/auth/verify", new { phone = "05320000003", code = right }, Json);
        Assert.Equal("auth.code_locked", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task No_token_garbage_token_or_old_tenant_header_is_401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().GetAsync("/api/records")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await With("uydurma-anahtar").GetAsync("/api/records")).StatusCode);
        var eski = f.CreateClient();
        eski.DefaultRequestHeaders.Add("X-Tenant-Id", ApiFactory.ShopA.ToString());   // K-073.6'nın geçici yolu KAPANDI
        Assert.Equal(HttpStatusCode.Unauthorized, (await eski.GetAsync("/api/records")).StatusCode);
    }

    [Fact]
    public async Task Without_sms_provider_code_request_is_503_not_silent()
    {
        var noSms = f.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.RemoveAll<Tamiron.Application.ISmsSender>();
            s.AddSingleton<Tamiron.Application.ISmsSender, Tamiron.Infrastructure.NotConfiguredSmsSender>();
        }));
        var res = await noSms.CreateClient().PostAsJsonAsync("/api/auth/code", new { phone = "05320000004" }, Json);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, res.StatusCode);
    }
}
