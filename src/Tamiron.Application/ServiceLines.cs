using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.ServiceRecords;

// B3 · Satırlar (prototip 30-kayit). Her komut kaydın GÜNCEL hâlini döner:
// ekran toplamı ve sırayı yeniden hesaplamasın (arayüz karar vermez).

public sealed record LineBody(LineKind Kind, string Name, decimal Price, decimal? Cost = null, PartSupplier? SuppliedBy = null);

public sealed record AddLine(Guid RecordId, LineBody Line) : IRequest<ServiceRecordDto>;
public sealed record UpdateLine(Guid RecordId, Guid LineId, LineBody Line) : IRequest<ServiceRecordDto>;
public sealed record DeleteLine(Guid RecordId, Guid LineId) : IRequest<ServiceRecordDto>;
public sealed record RestoreLine(Guid RecordId, Guid LineId) : IRequest<ServiceRecordDto>;
/// <summary>H3 · Parça kimden alındı + veresiye mi (K-090). Parçacı bu dükkânın olmalı (süzgeç; yoksa 404).</summary>
public sealed record SetLinePurchase(Guid RecordId, Guid LineId, Guid? SupplierId, bool OnCredit) : IRequest<ServiceRecordDto>;
public sealed record PurchaseBody(Guid? SupplierId, bool OnCredit);

internal sealed class LineHandlers(ITamironDb db, ITenantContext tenant) :
    IRequestHandler<AddLine, ServiceRecordDto>,
    IRequestHandler<UpdateLine, ServiceRecordDto>,
    IRequestHandler<DeleteLine, ServiceRecordDto>,
    IRequestHandler<RestoreLine, ServiceRecordDto>,
    IRequestHandler<SetLinePurchase, ServiceRecordDto>
{
    public async Task<ServiceRecordDto> Handle(SetLinePurchase c, CancellationToken ct)
    {
        var r = await Record(c.RecordId, ct);
        if (c.SupplierId is { } sid && !await db.Suppliers.AnyAsync(s => s.Id == sid, ct))
            throw new NotFoundException("Parçacı");
        Line(r, c.LineId).SetPurchase(c.SupplierId, c.OnCredit);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }

    private async Task<ServiceRecord> Record(Guid id, CancellationToken ct)
        => await db.ServiceRecords.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, ct)
           ?? throw new NotFoundException("Servis kaydı");

    private static ServiceLine Line(ServiceRecord r, Guid lineId)
        => r.Lines.FirstOrDefault(l => l.Id == lineId) ?? throw new NotFoundException("Satır");

    public async Task<ServiceRecordDto> Handle(AddLine c, CancellationToken ct)
    {
        var r = await Record(c.RecordId, ct);
        var b = c.Line;
        var line = b.Kind == LineKind.Labor
            ? ServiceLine.Labor(b.Name, b.Price)
            : ServiceLine.Part(b.Name, b.Price, b.SuppliedBy ?? PartSupplier.Shop, b.Cost);
        r.AddLine(line);
        // KDV oranı satırda DONAR (C6): eklendiği günün dükkân varsayılanı.
        line.FreezeVatRate(await db.Shops.Select(s => s.DefaultVatRate).FirstAsync(ct));
        db.ServiceLines.Add(line);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }

    public async Task<ServiceRecordDto> Handle(UpdateLine c, CancellationToken ct)
    {
        var r = await Record(c.RecordId, ct);
        var b = c.Line;
        Line(r, c.LineId).Update(b.Kind, b.Name, b.Price, b.Cost, b.SuppliedBy);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }

    /// <summary>Silme = soft-delete (altyapı çeviriyor); "Geri al" RestoreLine.</summary>
    public async Task<ServiceRecordDto> Handle(DeleteLine c, CancellationToken ct)
    {
        var r = await Record(c.RecordId, ct);
        db.ServiceLines.Remove(Line(r, c.LineId));
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }

    public async Task<ServiceRecordDto> Handle(RestoreLine c, CancellationToken ct)
    {
        var r = await Record(c.RecordId, ct);   // kayıt süzgeçten geçer: kendi dükkânın, silinmemiş
        // Silinmiş satırı görmek için süzgeç KAPATILIYOR — kapatılan süzgeç kiracıyı da
        // kapatır. Kiracı burada AÇIKÇA yeniden yazılır; unutulursa başka dükkânın
        // satırı geri getirilebilirdi.
        var line = await db.ServiceLines.IgnoreQueryFilters().FirstOrDefaultAsync(l =>
                       l.Id == c.LineId && l.ServiceRecordId == r.Id && l.TenantId == tenant.TenantId &&
                       l.DateTimeDeleted != null, ct)
                   ?? throw new NotFoundException("Silinmiş satır");
        line.DateTimeDeleted = null;
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }
}
