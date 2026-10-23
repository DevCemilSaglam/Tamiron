using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tamiron.Api.Tests;

/// <summary>D7 · Sağlayıcı reddederse usta 503 + Türkçe "tekrar dene" görür; kod destek için yanıtta.</summary>
public class SmsFailureApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Provider_rejection_is_503_with_code()
    {
        f.Sms.FailWith = "sms.provider_header";
        try
        {
            var res = await f.CreateClient().PostAsJsonAsync("/api/auth/code", new { phone = "05320000003" });
            Assert.Equal(HttpStatusCode.ServiceUnavailable, res.StatusCode);
            var body = await res.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("sms.provider_header", body.GetProperty("code").GetString());
            Assert.Equal("SMS şu an gönderilemedi. Birazdan tekrar dene.", body.GetProperty("title").GetString());
        }
        finally { f.Sms.FailWith = null; }
    }
}
