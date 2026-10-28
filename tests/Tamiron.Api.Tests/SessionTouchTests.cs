using System.Net;

namespace Tamiron.Api.Tests;

/// <summary>
/// H7'de tarayıcıda bulundu: oturumun "son görülme"si saatte bir yazılır. Saat dolunca AYNI ANDA giden iki istek
/// (kabuk: rozet + ana sayfa) ikisi de yazmaya kalkıyor, biri eşzamanlılık çatışması alıyor ve bir OKUMA isteği
/// 409 dönüyordu. Son görülmeyi öteki istek zaten yazdı — çatışan istek işine devam etmeli.
/// Kendi fabrikası var: saati ileri alır, öteki testlerin saatine dokunmaz.
/// </summary>
public class SessionTouchTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Parallel_requests_after_an_hour_all_succeed()
    {
        var api = f.For(await f.NewShopAsync());
        for (var tur = 1; tur <= 5; tur++)
        {
            f.Clock.Today = f.Clock.Today.AddDays(1);   // son görülme eskidi: her istek yazmak ister
            var yanitlar = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => api.GetAsync("/api/home")));
            Assert.All(yanitlar, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        }
    }
}
