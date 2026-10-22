using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Shops;

// C5 · Dükkân ayarları (prototip 99-ayarlar). Prototip ekranı yalnız İKİ şeyi değiştirir:
// bakım periyodu (3 · 6 · 12 ay) ve alış fiyatının sorulması (A-M43). Ad, adres, telefon
// okunur (müşteri özeti, mesaj imzası); düzenlenmesi prototipte YOK — eklenmedi (yeni özellik).
// Km periyodu prototipte de sabit (10.000) — okunur.

public sealed record ShopSettings(string Name, string? Address, string? Phone, int MaintenanceMonths, int MaintenanceKm,
                                  bool AskPartCost, DateTime? PrivacyNoticeShownAt);

public sealed record GetShopSettings : IRequest<ShopSettings>;
/// <summary>Kısmi: null = dokunma.</summary>
public sealed record PatchShopSettings(int? MaintenanceMonths = null, bool? AskPartCost = null) : IRequest<ShopSettings>;
public sealed record MarkPrivacyNoticeShown : IRequest<ShopSettings>;

internal sealed class ShopHandlers(ITamironDb db, IClock clock) :
    IRequestHandler<GetShopSettings, ShopSettings>,
    IRequestHandler<PatchShopSettings, ShopSettings>,
    IRequestHandler<MarkPrivacyNoticeShown, ShopSettings>
{
    // Dükkân süzgeci (Id == kiracı) kendi satırından başkasını göstermez.
    private Task<Shop> Shop(CancellationToken ct) => db.Shops.FirstAsync(ct);

    private static ShopSettings Dto(Shop s) => new(s.Name, s.Address, s.Phone, s.DefaultMaintenanceMonths,
                                                   s.DefaultMaintenanceKm, s.AskPartCost, s.PrivacyNoticeShownAt);

    public async Task<ShopSettings> Handle(GetShopSettings q, CancellationToken ct) => Dto(await Shop(ct));

    public async Task<ShopSettings> Handle(PatchShopSettings c, CancellationToken ct)
    {
        var s = await Shop(ct);
        if (c.MaintenanceMonths is { } m) s.SetMaintenanceMonths(m);
        if (c.AskPartCost is { } a) s.AskPartCost = a;
        await db.SaveChangesAsync(ct);
        return Dto(s);
    }

    public async Task<ShopSettings> Handle(MarkPrivacyNoticeShown c, CancellationToken ct)
    {
        var s = await Shop(ct);
        s.MarkPrivacyNoticeShown(clock.UtcNow);
        await db.SaveChangesAsync(ct);
        return Dto(s);
    }
}
