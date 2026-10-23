using System.Net;
using System.Text;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>Bellekte oturum deposu (tarayıcının localStorage'ı yerine).</summary>
public sealed class MemoryTokens(string? token = null) : ITokenStore
{
    public string? Token = token;
    public ValueTask<string?> GetAsync() => ValueTask.FromResult(Token);
    public ValueTask SetAsync(string? t) { Token = t; return ValueTask.CompletedTask; }
}

/// <summary>Sahte API: "YÖNTEM /yol" → (durum, gövde). Gelen istekler kaydedilir. Ağa çıkılmaz.</summary>
public sealed class FakeApi : HttpMessageHandler
{
    public readonly Dictionary<string, (HttpStatusCode Status, string Body)> Routes = new();
    public readonly List<(string Path, string? Auth, string? Body)> Calls = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var key = r.Method.Method + " " + r.RequestUri!.AbsolutePath;
        Calls.Add((r.RequestUri.PathAndQuery, r.Headers.Authorization?.ToString(), r.Content is null ? null : await r.Content.ReadAsStringAsync(ct)));
        var (s, b) = Routes.TryGetValue(key, out var v) ? v : (HttpStatusCode.NotFound, "{\"title\":\"yok\"}");
        return new HttpResponseMessage(s) { Content = new StringContent(b, Encoding.UTF8, "application/json") };
    }
}
