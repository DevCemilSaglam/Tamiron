using Tamiron.Application;
using Tamiron.Domain;

namespace Tamiron.Api;

/// <summary>
/// GEÇİCİ kiracı çözümü (K-073.6): X-Tenant-Id başlığı. Oturum açma
/// yöntemi kararlaşınca kiracı oturumdan gelir ve bu sınıf silinir.
/// </summary>
public sealed class HeaderTenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
}

public sealed class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx, HeaderTenantContext tenant)
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            if (!Guid.TryParse(ctx.Request.Headers["X-Tenant-Id"], out var id) || id == Guid.Empty)
            {
                await Results.Problem(statusCode: 401, title: "Dükkân belirtilmedi.").ExecuteAsync(ctx);
                return;
            }
            tenant.TenantId = id;
        }
        await next(ctx);
    }
}

/// <summary>
/// Kural ihlali 422 + makine kodu + Türkçe mesaj; bulunamadı 404.
/// Başka dükkânın kaydı da 404'tür: varlığı bile sızmasın.
/// </summary>
public sealed class ErrorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (DomainException e)
        {
            await Results.Problem(statusCode: 422, title: e.Message,
                extensions: new Dictionary<string, object?> { ["code"] = e.Code }).ExecuteAsync(ctx);
        }
        catch (NotFoundException e)
        {
            await Results.Problem(statusCode: 404, title: e.Message).ExecuteAsync(ctx);
        }
    }
}
