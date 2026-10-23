using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Registrations;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>
/// D8 · Ruhsat okuma ucu: okununca araç kaydedilir (K-065), çatışmada usta seçer (K-069),
/// okuyucu yoksa 503 + "elle yaz". Okuyucu SAHTE — fotoğraf hiçbir yere gitmez.
/// Bir sınıfta: sahte okuyucunun "sıradaki" değeri testler arasında paylaşılır.
/// </summary>
public class RegistrationApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static MultipartFormDataContent Photo(string? plate = null, string type = "image/jpeg", int bytes = 100)
    {
        var form = new MultipartFormDataContent();
        var img = new ByteArrayContent(new byte[bytes]);
        img.Headers.ContentType = new MediaTypeHeaderValue(type);
        form.Add(img, "photo", "ruhsat.jpg");
        if (plate is not null) form.Add(new StringContent(plate), "plate");
        return form;
    }

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Read_saves_new_vehicle_then_conflict_waits_then_save_fills_only_empty()
    {
        var api = f.For(ApiFactory.ShopB);
        f.Reader.Next = new RegistrationRead(Plate: "34 RS 101", Brand: "FIAT", Model: "EGEA", Year: "2021", Engine: "1.4", Vin: "ZFA35600006123456");

        // 1 · Yeni araç: okunduğu AN kaydedilir.
        var r = await Read<RegistrationResult>(await api.PostAsync("/api/registrations/read", Photo()));
        Assert.Equal((RegistrationOutcome.Save, true, true), (r.Outcome, r.Created, r.VinRead));
        Assert.Equal(("34 RS 101", "FIAT"), (r.Vehicle!.PlateNo, r.Vehicle.Brand));
        Assert.True(r.Vehicle.HasPartIdentity);

        // 2 · Usta "34 RS 108" yazmıştı, ruhsatta 101 okundu → çatışma, HİÇBİR ŞEY kaydedilmez.
        var calls = f.Reader.Calls;
        var c = await Read<RegistrationResult>(await api.PostAsync("/api/registrations/read", Photo("34 rs 108")));
        Assert.Equal((RegistrationOutcome.PlateConflict, "34 RS 108", "34 RS 101"), (c.Outcome, c.TypedPlate, c.ReadPlate));
        Assert.Null(c.Vehicle);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync("/api/vehicles?plate=34%20RS%20108")).StatusCode);

        // 3 · Usta kendi yazdığını seçti: fotoğraf YENİDEN okunmaz, okunanlar gönderilir.
        var s = await Read<RegistrationResult>(await api.PostAsJsonAsync("/api/registrations/save", new { plate = "34 RS 108", read = c.Read }, Json));
        Assert.Equal(("34 RS 108", true), (s.Vehicle!.PlateNo, s.Created));
        Assert.Equal(calls + 1, f.Reader.Calls);

        // 4 · Kayıtlı araç yeniden okundu: usta markayı düzeltmişti, yenilmez; boş renk dolar.
        await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{r.Vehicle.Id}", new { brand = "Fiat" }, Json));
        f.Reader.Next = f.Reader.Next with { Color = "Beyaz" };
        var again = await Read<RegistrationResult>(await api.PostAsync("/api/registrations/read", Photo()));
        Assert.Equal((false, "Fiat", "Beyaz"), (again.Created, again.Vehicle!.Brand, again.Vehicle.Color));
        Assert.Equal(new[] { "renk" }, again.Filled);
    }

    [Fact]
    public async Task Unreadable_plate_and_no_typed_plate_saves_nothing()
    {
        f.Reader.Next = new RegistrationRead(Plate: null, Brand: "RENAULT");
        var r = await Read<RegistrationResult>(await f.For(ApiFactory.ShopB).PostAsync("/api/registrations/read", Photo()));
        Assert.Equal(RegistrationOutcome.NoPlate, r.Outcome);
        Assert.Null(r.Vehicle);
        Assert.Equal("RENAULT", r.Read.Brand);   // okunanlar ekranda kalır, usta plakayı yazınca kaydedilir
    }

    [Fact]
    public async Task Reader_off_is_503_with_manual_hint_and_bad_photo_is_422()
    {
        var api = f.For(ApiFactory.ShopB);
        f.Reader.Next = null;
        var res = await api.PostAsync("/api/registrations/read", Photo());
        Assert.Equal(HttpStatusCode.ServiceUnavailable, res.StatusCode);
        Assert.Equal("registration.reader_off", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

        f.Reader.Next = new RegistrationRead(Plate: "34 RS 102");
        var calls = f.Reader.Calls;
        var pdf = await api.PostAsync("/api/registrations/read", Photo(type: "application/pdf"));
        Assert.Equal("registration.photo_invalid", (await pdf.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        var big = await api.PostAsync("/api/registrations/read", Photo(bytes: 10 * 1024 * 1024 + 1));
        Assert.Equal("registration.photo_too_big", (await big.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(calls, f.Reader.Calls);   // geçersiz fotoğraf okuyucuya GİTMEZ (para)
    }

    [Fact]
    public async Task Needs_a_session()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().PostAsync("/api/registrations/read", Photo())).StatusCode);
}
