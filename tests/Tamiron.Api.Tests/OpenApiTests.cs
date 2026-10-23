using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Tamiron.Api.Tests;

/// <summary>
/// D18 · OpenAPI belgesi (K-076.17): geliştirme/test ortamında /openapi/v1.json; BÜTÜN /api uçları
/// belgede (biri eksikse istemci yazan onu bilemez); Bearer anahtar tanımlı. Üretimde KAPALI.
/// </summary>
public class OpenApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Document_lists_every_api_endpoint_and_the_bearer_scheme()
    {
        var res = await f.CreateClient().GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;

        var documented = doc.GetProperty("paths").EnumerateObject()
            .SelectMany(p => p.Value.EnumerateObject().Select(m => m.Name.ToUpperInvariant() + " " + p.Name)).ToHashSet();
        var mapped = f.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText!.StartsWith("/api"))
            .SelectMany(e => (e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? []).Select(m => m + " " + Normalize(e.RoutePattern.RawText!)))
            .ToHashSet();
        Assert.True(mapped.Count > 60, $"uç sayısı beklenenden az: {mapped.Count}");
        Assert.Empty(mapped.Except(documented));

        Assert.Equal("bearer", doc.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer").GetProperty("scheme").GetString());
    }

    [Fact]
    public async Task Production_does_not_serve_it()
    {
        using var prod = f.WithWebHostBuilder(b => b.UseEnvironment("Production"));
        Assert.Equal(HttpStatusCode.NotFound, (await prod.CreateClient().GetAsync("/openapi/v1.json")).StatusCode);
    }

    /// <summary>"/api/records/{id:guid}" → "/api/records/{id}" (belgede kısıt yazılmaz).</summary>
    static string Normalize(string route) => System.Text.RegularExpressions.Regex.Replace(route, @"\{(\w+)(:[^}]+)?\}", "{$1}");
}
