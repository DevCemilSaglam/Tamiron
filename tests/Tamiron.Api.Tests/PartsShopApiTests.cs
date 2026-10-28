using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.PriceRequests;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Suppliers;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>
/// H6 · Yedek Parça (K-091, TASARIM-H-YEDEK-PARCA). Parçacı kendi numarasıyla girer; tamirhanelerin o numaraya
/// sorduğu sorular ona görünür — müşteri bilgisi olmadan. Cevap PARÇACININ kiracısında durur; tamirhane yalnız
/// kendi sorusuna geleni okur, satıra kendiliğinden işlenmez (Kol B): usta "Bunu kullan" der. Türler birbirinin
/// uçlarını kullanamaz.
/// </summary>
public class PartsShopApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        Assert.True(res.IsSuccessStatusCode, $"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    static async Task<string?> Code(HttpResponseMessage res) => (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

    record Kutu(List<Gelen> Items);
    record Gelen(Guid RequestId, string ShopName, string? ShopPhone, string PlateNo, string? Brand, string? Model, string? Year,
                 string? Engine, string? Vin, DateTime SentAt, List<GelenSatir> Lines);
    record GelenSatir(Guid LineId, string Name, List<Cevap> Answers);
    record Cevap(Guid Id, Guid LineId, decimal? Price, string? Brand, string? Code, string? Note, DateTime AnsweredAt, bool Used);

    static string Tel() => "0536" + Random.Shared.NextInt64(1_000_000, 9_999_999);

    /// <summary>Tamirhane: kimliği tamam araç, iki parça, parçacıya (numara) sorulmuş soru.</summary>
    async Task<(HttpClient Usta, ServiceRecordDto Kayit, PriceRequestDto Soru)> Sor(string plate, string parcaciTel, string sahip = "Gizli Müşteri")
    {
        var usta = f.For(await f.NewShopAsync());
        var r = await Read<ServiceRecordDto>(await usta.PostAsJsonAsync("/api/records", new { plate, complaint = "Fren" }, Json));
        await Read<JsonElement>(await usta.PatchAsJsonAsync($"/api/vehicles/{r.VehicleId}",
            new { brand = "Fiat", model = "Egea", year = "2021", engine = "1.4 Fire", ownerName = sahip, ownerPhone = "05337770011" }, Json));
        r = await Read<ServiceRecordDto>(await usta.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Balata", price = 0 }, Json));
        r = await Read<ServiceRecordDto>(await usta.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Disk", price = 0 }, Json));
        var sup = await Read<SupplierDto>(await usta.PostAsJsonAsync("/api/suppliers", new { name = "Öz Parça", phone = parcaciTel }, Json));
        var soru = (await Read<List<PriceRequestDto>>(await usta.PostAsJsonAsync($"/api/records/{r.Id}/price-requests",
            new { supplierIds = new[] { sup.Id } }, Json)))[0];
        return (usta, r, soru);
    }

    async Task<HttpClient> Parcaci(string tel) => f.For(await f.NewShopAsync(ShopKind.YedekParca, tel));

    [Fact]
    public async Task Supplier_sees_questions_sent_to_its_number_without_customer_details()
    {
        var tel = Tel();
        var (_, r, soru) = await Sor("34 YP 101", tel);
        var p = await Parcaci(tel);
        var kutu = await Read<Kutu>(await p.GetAsync("/api/parts/inbox"));
        var g = Assert.Single(kutu.Items);
        Assert.Equal(soru.Id, g.RequestId);
        Assert.Equal("34 YP 101", g.PlateNo);
        Assert.Equal(("Fiat", "Egea", "2021", "1.4 Fire"), (g.Brand, g.Model, g.Year, g.Engine));
        Assert.Equal(new[] { "Balata", "Disk" }, g.Lines.Select(l => l.Name));
        var ham = await (await p.GetAsync("/api/parts/inbox")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("Gizli Müşteri", ham);   // müşteri adı yok
        Assert.DoesNotContain("777 00 11", ham);       // müşteri telefonu yok
    }

    [Fact]
    public async Task Other_numbers_and_deleted_records_are_not_seen()
    {
        var tel = Tel();
        await Sor("34 YP 102", Tel());   // başka parçacıya
        var (usta, r, _) = await Sor("34 YP 103", tel);
        var p = await Parcaci(tel);
        Assert.Single((await Read<Kutu>(await p.GetAsync("/api/parts/inbox"))).Items);
        Assert.Equal(HttpStatusCode.NoContent, (await usta.DeleteAsync($"/api/records/{r.Id}")).StatusCode);
        Assert.Empty((await Read<Kutu>(await p.GetAsync("/api/parts/inbox"))).Items);
    }

    [Fact]
    public async Task Deleting_the_supplier_hides_its_questions()
    {
        // Mutasyonla bulundu: usta parçacıyı sildiğinde o numara soruları görmeye devam ediyordu.
        var tel = Tel();
        var (usta, _, soru) = await Sor("34 YP 108", tel);
        var p = await Parcaci(tel);
        Assert.Single((await Read<Kutu>(await p.GetAsync("/api/parts/inbox"))).Items);
        Assert.Equal(HttpStatusCode.NoContent, (await usta.DeleteAsync($"/api/suppliers/{soru.SupplierId}")).StatusCode);
        Assert.Empty((await Read<Kutu>(await p.GetAsync("/api/parts/inbox"))).Items);
    }

    [Fact]
    public async Task Answer_goes_to_the_shop_but_is_not_applied_until_the_master_uses_it()
    {
        var tel = Tel();
        var (usta, r, soru) = await Sor("34 YP 104", tel);
        var p = await Parcaci(tel);
        var kutu = await Read<Kutu>(await p.PostAsJsonAsync($"/api/parts/requests/{soru.Id}/answers", new
        {
            lines = new[] { new { lineId = r.Lines[0].Id, price = (decimal?)1200, brand = "Bosch", code = "0986494", note = (string?)null } }
        }, Json));
        Assert.Equal(1200m, kutu.Items[0].Lines[0].Answers.Single().Price);   // parçacı ne yazdığını görür

        var sorular = await Read<List<JsonElement>>(await usta.GetAsync($"/api/records/{r.Id}/price-requests"));
        var cevap = sorular[0].GetProperty("supplierAnswers").EnumerateArray().Single();
        Assert.Equal("Bosch", cevap.GetProperty("brand").GetString());
        Assert.False(cevap.GetProperty("used").GetBoolean());
        var kayit = await Read<ServiceRecordDto>(await usta.GetAsync($"/api/records/{r.Id}"));
        Assert.Null(kayit.Lines[0].Cost);   // Kol B: kendiliğinden işlenmez

        kayit = await Read<ServiceRecordDto>(await usta.PostAsJsonAsync($"/api/supplier-answers/{cevap.GetProperty("id").GetGuid()}/use",
            new { customerPrice = 1500, onCredit = true }, Json));
        var l = kayit.Lines[0];
        Assert.Equal((1200m, 1500m, true), (l.Cost, l.Price, l.OnCredit));
        Assert.Equal(soru.SupplierId, l.SupplierId);
        sorular = await Read<List<JsonElement>>(await usta.GetAsync($"/api/records/{r.Id}/price-requests"));
        Assert.True(sorular[0].GetProperty("supplierAnswers")[0].GetProperty("used").GetBoolean());
        Assert.Contains("Bosch", sorular[0].GetProperty("replies")[0].GetProperty("rawText").GetString());   // ham metin korunur
    }

    [Fact]
    public async Task Supplier_cannot_answer_a_line_not_asked_nor_a_question_not_sent_to_it()
    {
        var tel = Tel();
        var (usta, r, soru) = await Sor("34 YP 105", tel);
        var baskasi = await Sor("34 YP 106", Tel());
        var p = await Parcaci(tel);
        var yabanciSoru = await p.PostAsJsonAsync($"/api/parts/requests/{baskasi.Soru.Id}/answers",
            new { lines = new[] { new { lineId = baskasi.Kayit.Lines[0].Id, price = 1 } } }, Json);
        Assert.Equal(HttpStatusCode.NotFound, yabanciSoru.StatusCode);
        var yabanciSatir = await p.PostAsJsonAsync($"/api/parts/requests/{soru.Id}/answers",
            new { lines = new[] { new { lineId = baskasi.Kayit.Lines[0].Id, price = 1 } } }, Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, yabanciSatir.StatusCode);
        Assert.Equal("answer.line_not_asked", await Code(yabanciSatir));
    }

    [Fact]
    public async Task A_shop_cannot_use_an_answer_to_another_shops_question()
    {
        var tel = Tel();
        var (_, r, soru) = await Sor("34 YP 107", tel);
        var p = await Parcaci(tel);
        await Read<Kutu>(await p.PostAsJsonAsync($"/api/parts/requests/{soru.Id}/answers",
            new { lines = new[] { new { lineId = r.Lines[0].Id, price = 900 } } }, Json));
        var cevapId = (await Read<Kutu>(await p.GetAsync("/api/parts/inbox"))).Items[0].Lines[0].Answers[0].Id;
        var yabanci = f.For(await f.NewShopAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await yabanci.PostAsJsonAsync($"/api/supplier-answers/{cevapId}/use", new { }, Json)).StatusCode);
        Assert.DoesNotContain(cevapId.ToString(), await (await yabanci.GetAsync($"/api/records/{r.Id}/price-requests")).Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Each_kind_uses_only_its_own_app()
    {
        var p = await Parcaci(Tel());
        var res = await p.GetAsync("/api/records");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.Equal("app.wrong_kind", await Code(res));
        var usta = f.For(await f.NewShopAsync());
        var res2 = await usta.GetAsync("/api/parts/inbox");
        Assert.Equal(HttpStatusCode.Forbidden, res2.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await p.GetAsync("/api/auth/sessions")).StatusCode);   // oturum uçları ortak
    }

    [Fact]
    public async Task Sign_up_chooses_the_kind_and_login_says_which_app()
    {
        var tel = Tel();
        Assert.Equal(HttpStatusCode.Accepted, (await f.CreateClient().PostAsJsonAsync("/api/auth/signup/code", new { phone = tel }, Json)).StatusCode);
        var kod = System.Text.RegularExpressions.Regex.Match(f.Sms.Sent.Last(x => x.Phone.Replace(" ", "") == tel).Text, @"\d{6}").Value;
        var giris = await Read<JsonElement>(await f.CreateClient().PostAsJsonAsync("/api/auth/signup",
            new { phone = tel, code = kod, shopName = "Öz Parça", kind = "YedekParca" }, Json));
        Assert.Equal("YedekParca", giris.GetProperty("kind").GetString());
    }
}
