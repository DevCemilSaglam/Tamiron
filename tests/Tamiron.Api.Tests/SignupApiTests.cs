using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tamiron.Application.Auth;
using Tamiron.Application.Shops;

namespace Tamiron.Api.Tests;

/// <summary>
/// D6 · Yeni dükkân kaydı (K-076.6, K-074.5'i kapatır): telefon + SMS kodu + dükkân adı →
/// dükkân, kullanıcı ve süresiz oturum tek adımda. Kayıtlı numara kayıt olamaz, giriş yapar.
/// </summary>
public class SignupApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    string LastCode(string phone) => Regex.Match(f.Sms.Sent.Last(s => s.Phone == phone).Text, @"\b\d{6}\b").Value;

    static async Task<string?> Code(HttpResponseMessage res)
        => (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

    [Fact]
    public async Task New_number_gets_a_code_and_opens_its_own_shop()
    {
        var anon = f.CreateClient();
        Assert.Equal(HttpStatusCode.Accepted, (await anon.PostAsJsonAsync("/api/auth/signup/code", new { phone = "05441230001" }, Json)).StatusCode);
        Assert.Contains("Tamiron kayıt kodun", f.Sms.Sent.Last().Text);

        var res = await anon.PostAsJsonAsync("/api/auth/signup",
            new { phone = "05441230001", code = LastCode("0544 123 00 01"), shopName = "  Yıldız Oto  ", userName = "Kemal Usta", deviceName = "telefon" }, Json);
        res.EnsureSuccessStatusCode();
        var login = (await res.Content.ReadFromJsonAsync<LoginResult>(Json))!;
        Assert.Equal(("Yıldız Oto", "Kemal Usta"), (login.ShopName, login.UserName));

        var api = f.CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var shop = (await api.GetFromJsonAsync<ShopSettings>("/api/shop", Json))!;
        Assert.Equal(("Yıldız Oto", "0544 123 00 01"), (shop.Name, shop.Phone));   // dükkân telefonu = ustanın numarası (imza)
        // Boş dükkân: başka dükkânın kaydı görünmez.
        Assert.Equal("{\"inShopCount\":0,\"count\":0,\"items\":[]}", await api.GetStringAsync("/api/records"));

        // Artık kayıtlı: ikinci kayıt yok, normal giriş var.
        Assert.Equal("auth.phone_registered", await Code(await anon.PostAsJsonAsync("/api/auth/signup/code", new { phone = "05441230001" }, Json)));
        Assert.Equal(HttpStatusCode.Accepted, (await anon.PostAsJsonAsync("/api/auth/code", new { phone = "05441230001" }, Json)).StatusCode);
    }

    [Fact]
    public async Task Registered_number_cannot_sign_up()
        => Assert.Equal("auth.phone_registered",
            await Code(await f.CreateClient().PostAsJsonAsync("/api/auth/signup/code", new { phone = "05320000004" }, Json)));

    [Fact]
    public async Task Shop_name_is_required_and_does_not_burn_an_attempt()
    {
        var anon = f.CreateClient();
        await anon.PostAsJsonAsync("/api/auth/signup/code", new { phone = "05441230002" }, Json);
        var code = LastCode("0544 123 00 02");
        for (var i = 0; i < 6; i++)   // deneme hakkı 5; ad eksikliği deneme sayılsaydı 6. kod reddedilirdi
            Assert.Equal("shop.name_required", await Code(await anon.PostAsJsonAsync("/api/auth/signup",
                new { phone = "05441230002", code, shopName = " " }, Json)));
        (await anon.PostAsJsonAsync("/api/auth/signup", new { phone = "05441230002", code, shopName = "Kaya Oto" }, Json)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Wrong_code_creates_nothing()
    {
        var anon = f.CreateClient();
        await anon.PostAsJsonAsync("/api/auth/signup/code", new { phone = "05441230003" }, Json);
        var real = LastCode("0544 123 00 03");
        var wrong = real == "000000" ? "111111" : "000000";
        Assert.Equal("auth.code_wrong", await Code(await anon.PostAsJsonAsync("/api/auth/signup",
            new { phone = "05441230003", code = wrong, shopName = "Sahte Oto" }, Json)));
        // Kayıt olmadı: hâlâ "kayıtlı değil".
        Assert.Equal("auth.unknown_phone", await Code(await anon.PostAsJsonAsync("/api/auth/code", new { phone = "05441230003" }, Json)));
    }
}
