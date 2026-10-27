using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G2b · "Ruhsatı çek" sekmesi (prototip 20-ruhsat ruhsatOku/ruhsatKaydet, K-065, K-069). Fotoğraf API'ye
/// gider; okuma, kaydetme ve plaka çatışması kararı API'de (RegistrationIntake). Okuyucu kapalıysa API'nin
/// "elle yaz" cümlesi. Ekran yalnız sonucu söyler ve plaka sekmesine döner.
/// </summary>
public class RegistrationTabTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public RegistrationTabTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/vehicles/lookup"] = (HttpStatusCode.OK,
            "{\"plate\":\"33 VAH 34\",\"valid\":true,\"hint\":null,\"openRecordId\":null,\"vehicle\":{\"id\":\"33333333-3333-3333-3333-333333333333\",\"plateNo\":\"33 VAH 34\",\"brand\":\"Volkswagen\",\"model\":\"Jetta\",\"hasPartIdentity\":true}}");
    }

    const string Okunan = "{\"plate\":\"33 VAH 34\",\"brand\":\"VOLKSWAGEN\",\"model\":\"JETTA\",\"year\":\"2015\",\"vin\":null,\"color\":\"BEYAZ\"}";

    static string Sonuc(string outcome, string typed = "null", string readPlate = "\"33 VAH 34\"", bool vinRead = true, bool created = true, bool vehicle = true)
        => $"{{\"outcome\":\"{outcome}\",\"typedPlate\":{typed},\"readPlate\":{readPlate},\"read\":{Okunan},\"vinRead\":{(vinRead ? "true" : "false")},"
         + $"\"created\":{(created ? "true" : "false")},\"filled\":[],\"vehicle\":{(vehicle ? "{\"id\":\"33333333-3333-3333-3333-333333333333\",\"plateNo\":\"33 VAH 34\"}" : "null")}}}";

    static InputFileContent Foto() => InputFileContent.CreateFromBinary([1, 2, 3], "ruhsat.jpg", contentType: "image/jpeg");

    IRenderedComponent<KayitAc> RuhsatSekmesi()
    {
        var c = RenderComponent<KayitAc>();
        c.Find("#sekRuhsat").Click();
        return c;
    }

    [Fact]
    public void Two_tabs_plate_first()
    {
        var c = RenderComponent<KayitAc>();
        Assert.Equal("true", c.Find("#sekPlaka").GetAttribute("aria-selected"));
        Assert.Empty(c.FindAll("#sekmeRuhsat"));
        c.Find("#sekRuhsat").Click();
        Assert.Equal("true", c.Find("#sekRuhsat").GetAttribute("aria-selected"));
        Assert.Contains("Kamerayı aç", c.Find("#sekmeRuhsat").TextContent);
        Assert.Contains("Galeriden seç", c.Find("#sekmeRuhsat").TextContent);
    }

    [Fact]
    public void Read_registration_saves_the_vehicle_and_returns_to_the_plate_ready_to_open()
    {
        api.Routes["POST /api/registrations/read"] = (HttpStatusCode.OK, Sonuc("Save"));
        var c = RuhsatSekmesi();
        c.FindComponents<InputFile>()[0].UploadFiles(Foto());
        c.WaitForAssertion(() => Assert.Contains("Ruhsat okundu, araç kaydedildi", c.Find("#rOkuma").TextContent));
        Assert.Equal("true", c.Find("#sekPlaka").GetAttribute("aria-selected"));
        Assert.Equal("33 VAH 34", c.Find("#rPlaka").GetAttribute("value"));
        c.WaitForAssertion(() => Assert.Equal("Kaydı aç", c.Find("#kaydiAc").TextContent.Trim()));
        Assert.Equal("Bearer t-1", api.Calls.Single(x => x.Path == "/api/registrations/read").Auth);
    }

    [Fact]
    public void Missing_vin_is_said()
    {
        api.Routes["POST /api/registrations/read"] = (HttpStatusCode.OK, Sonuc("Save", vinRead: false));
        var c = RuhsatSekmesi();
        c.FindComponents<InputFile>()[1].UploadFiles(Foto());
        c.WaitForAssertion(() => Assert.Contains("şase okunamadı", c.Find("#rOkuma").TextContent));
    }

    [Fact]
    public void Already_registered_vehicle_is_told()
    {
        api.Routes["POST /api/registrations/read"] = (HttpStatusCode.OK, Sonuc("Save", created: false));
        var c = RuhsatSekmesi();
        c.FindComponents<InputFile>()[0].UploadFiles(Foto());
        c.WaitForAssertion(() => Assert.Contains("Ruhsat okundu — bu araç zaten kayıtlı.", c.Find("#rOkuma").TextContent));
    }

    [Fact]
    public void Plate_conflict_asks_which_and_saves_only_after_the_choice()
    {
        api.Routes["POST /api/registrations/read"] = (HttpStatusCode.OK, Sonuc("PlateConflict", typed: "\"33 VAH 43\"", vehicle: false, created: false));
        api.Routes["POST /api/registrations/save"] = (HttpStatusCode.OK, Sonuc("Save"));
        var c = RenderComponent<KayitAc>();
        c.Find("#rPlaka").Input("33vah43");
        c.Find("#sekRuhsat").Click();
        c.FindComponents<InputFile>()[0].UploadFiles(Foto());
        c.WaitForAssertion(() => Assert.Contains("Sen 33 VAH 43 yazdın, ruhsatta 33 VAH 34 okundu. Hangisi doğru?", c.Find("#rPlakaCatisma").TextContent));
        Assert.DoesNotContain(api.Calls, x => x.Path == "/api/registrations/save");
        c.Find("#rPlakaRuhsat").Click();
        c.WaitForAssertion(() => Assert.Empty(c.FindAll("#rPlakaCatisma")));
        // Mutasyonla bulundu: "plate" okunanın içinde de geçiyor — kök alan ayrıca okunur.
        using var govde = System.Text.Json.JsonDocument.Parse(api.Calls.Single(x => x.Path == "/api/registrations/save").Body!);
        Assert.Equal("33 VAH 34", govde.RootElement.GetProperty("plate").GetString());
        Assert.Equal("VOLKSWAGEN", govde.RootElement.GetProperty("read").GetProperty("brand").GetString());
        // Ustanın yazdığı plaka fotoğrafla gider (K-069 kararı API'de verilsin diye).
        Assert.Contains("33vah43", api.Calls.Single(x => x.Path == "/api/registrations/read").Body);
        Assert.Equal("33 VAH 34", c.Find("#rPlaka").GetAttribute("value"));
    }

    [Fact]
    public void Unreadable_plate_keeps_what_was_read_in_the_vehicle_card()
    {
        api.Routes["POST /api/registrations/read"] = (HttpStatusCode.OK, Sonuc("NoPlate", readPlate: "null", vehicle: false, created: false));
        api.Routes["GET /api/vehicles/lookup"] = (HttpStatusCode.OK, "{\"plate\":\"06 XYZ 77\",\"valid\":true,\"hint\":null,\"vehicle\":null,\"openRecordId\":null}");
        var c = RuhsatSekmesi();
        c.FindComponents<InputFile>()[0].UploadFiles(Foto());
        c.WaitForAssertion(() => Assert.Contains("Plaka okunamadı — plakayı yaz, araç öyle kaydedilir.", c.Find("#rOkuma").TextContent));
        Assert.Equal("true", c.Find("#sekPlaka").GetAttribute("aria-selected"));
        c.Find("#rPlaka").Input("06xyz77");
        c.WaitForAssertion(() => Assert.Equal("VOLKSWAGEN", c.Find("#rMarka").GetAttribute("value")));
        Assert.Equal("2015", c.Find("#rYil").GetAttribute("value"));
    }

    [Fact]
    public void Reader_off_is_the_apis_sentence()
    {
        api.Routes["POST /api/registrations/read"] = (HttpStatusCode.ServiceUnavailable,
            "{\"code\":\"registration.reader_off\",\"title\":\"Ruhsat okuma şu an kapalı. Bilgileri elle yaz.\"}");
        var c = RuhsatSekmesi();
        c.FindComponents<InputFile>()[0].UploadFiles(Foto());
        c.WaitForAssertion(() => Assert.Equal("Ruhsat okuma şu an kapalı. Bilgileri elle yaz.", c.Find("#rHata").TextContent.Trim()));
    }

    static string Json(string body) => System.Text.RegularExpressions.Regex.Unescape(body);
}
