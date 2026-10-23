using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Summaries;

// D3 · Müşteri özet sayfası (K-023). Müşteri oturum açmaz: link = kod. Kod tek başına
// kaydı bulur, o yüzden okuma kiracı süzgecini BİLEREK atlar ve kiracıyı kayıttan alır.

public sealed record SummaryLinkDto(string Code, string Url);
public sealed record SummaryLineDto(string Label, string? Brand, string? Code, bool CustomerBrought, decimal? Amount);
public sealed record CustomerSummaryDto(string ShopName, string? ShopAddress, string? ShopPhone,
                                        string PlateNo, string? Brand, string? Model, string? Year,
                                        string TypeLabel, string? Km, string Complaint,
                                        IReadOnlyList<SummaryLineDto> Lines, decimal Total,
                                        bool Delivered, string StatusText);

public sealed record GetSummaryLink(Guid RecordId) : IRequest<SummaryLinkDto>;
public sealed record GetCustomerSummary(string Code) : IRequest<CustomerSummaryDto>;

internal sealed class SummaryHandlers(ITamironDb db) :
    IRequestHandler<GetSummaryLink, SummaryLinkDto>,
    IRequestHandler<GetCustomerSummary, CustomerSummaryDto>
{
    /// <summary>Linki USTA ister (oturumlu, kendi kaydı). Bir kez üretilen kod değişmez.</summary>
    public async Task<SummaryLinkDto> Handle(GetSummaryLink c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.FirstOrDefaultAsync(x => x.Id == c.RecordId, ct) ?? throw new NotFoundException("Servis kaydı");
        if (r.SummaryCode is null)
        {
            r.EnsureSummaryCode();
            await db.SaveChangesAsync(ct);
        }
        return new(r.SummaryCode!, CustomerSummary.Link(r.SummaryCode!));
    }

    /// <summary>
    /// Müşterinin okuması: canlı (iş sürerken güncellenir, K-023.1), silinmiş kayıt/satır
    /// görünmez. Bulunamayan kod 404 — var olup olmadığı da söylenmez.
    /// </summary>
    public async Task<CustomerSummaryDto> Handle(GetCustomerSummary q, CancellationToken ct)
    {
        var code = (q.Code ?? "").Trim().ToLowerInvariant();
        var r = await db.ServiceRecords.IgnoreQueryFilters().AsNoTracking()
                    .Include(x => x.Lines.Where(l => l.DateTimeDeleted == null))
                    .FirstOrDefaultAsync(x => x.SummaryCode == code && x.DateTimeDeleted == null, ct)
                ?? throw new NotFoundException("Özet");
        var shop = await db.Shops.IgnoreQueryFilters().AsNoTracking().FirstAsync(s => s.Id == r.TenantId, ct);
        var v = await db.Vehicles.IgnoreQueryFilters().AsNoTracking().FirstAsync(x => x.Id == r.VehicleId && x.TenantId == r.TenantId, ct);

        var lines = r.Lines.OrderBy(l => l.Position).ThenBy(l => l.DateTimeCreated)
            // Yarım satır müşteriye gösterilmez (K-038.2): adı da fiyatı da yoksa yok sayılır.
            .Where(l => !string.IsNullOrWhiteSpace(l.Name) || l.Price != 0)
            .Select(l => new SummaryLineDto(
                l.Name,
                l.Kind == LineKind.Part ? l.PartBrand : null,
                l.Kind == LineKind.Part && l.PartBrand is not null ? l.PartCode : null,
                l.Kind == LineKind.Part && l.SuppliedBy == PartSupplier.Customer,
                CustomerSummary.Amount(l)))
            .ToList();

        return new(shop.Name, shop.Address, shop.Phone, v.PlateNo, v.Brand, v.Model, v.Year,
            ServiceTypeGuess.Label(r.Type), r.Km, r.Complaint, lines, r.Total,
            r.DeliveredAt is not null, CustomerSummary.StatusText(r));
    }
}
