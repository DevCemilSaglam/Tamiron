using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.SupplierDebts;

// H3 · Parçacıya borç (K-090, TASARIM-H-MUHASEBE). Borç saklanmaz: veresiye işaretli satırların alışı −
// ödemeler (SupplierDebt.Of). Silinmiş kaydın satırları sayılmaz (kayıt süzgeci). Ödeme gider değil, kasa
// tahsilatı değil — CSV'de "Parçacıya ödeme".

public sealed record DebtLineDto(Guid RecordId, Guid LineId, string PlateNo, string Name, decimal Cost);
public sealed record SupplierPaymentDto(Guid Id, decimal Amount, PaymentMethod Method, DateOnly PaidOn);
public sealed record SupplierDebtItem(Guid SupplierId, string Name, string Phone, decimal Debt,
                                      IReadOnlyList<DebtLineDto> Lines, IReadOnlyList<SupplierPaymentDto> Payments);
public sealed record SupplierDebtList(decimal Total, IReadOnlyList<SupplierDebtItem> Items);

public sealed record ListSupplierDebts : IRequest<SupplierDebtList>;
public sealed record PaySupplier(Guid SupplierId, decimal Amount, PaymentMethod Method) : IRequest<SupplierDebtList>;
public sealed record UndoSupplierPayment(Guid PaymentId) : IRequest<SupplierDebtList>;
public sealed record PayBody(decimal Amount, PaymentMethod Method);

internal sealed class SupplierDebtHandlers(ITamironDb db, ITenantContext tenant, IClock clock) :
    IRequestHandler<ListSupplierDebts, SupplierDebtList>,
    IRequestHandler<PaySupplier, SupplierDebtList>,
    IRequestHandler<UndoSupplierPayment, SupplierDebtList>
{
    /// <summary>Veresiye satırlar (silinmiş kaydınki sayılmaz) ve ödemeler.</summary>
    private async Task<(List<ServiceLine> Lines, List<SupplierPayment> Payments)> Movements(CancellationToken ct)
    {
        var lines = await db.ServiceLines.AsNoTracking()
            .Where(l => l.OnCredit && db.ServiceRecords.Any(r => r.Id == l.ServiceRecordId)).ToListAsync(ct);
        var payments = await db.SupplierPayments.AsNoTracking().ToListAsync(ct);
        return (lines, payments);
    }

    private async Task<decimal> DebtOf(Guid supplierId, CancellationToken ct)
    {
        var (lines, payments) = await Movements(ct);
        return SupplierDebt.Of(lines, payments, supplierId);
    }

    public async Task<SupplierDebtList> Handle(ListSupplierDebts q, CancellationToken ct)
    {
        var (lines, payments) = await Movements(ct);
        var ids = lines.Select(l => l.SupplierId!.Value).Concat(payments.Select(p => p.SupplierId)).Distinct().ToList();
        // Silinmiş parçacı da adıyla görünür — geçmiş borç kime olduğunu kaybetmez (K-076.1). Kiracı AÇIKÇA.
        var sups = await db.Suppliers.IgnoreQueryFilters().AsNoTracking()
            .Where(s => ids.Contains(s.Id) && s.TenantId == tenant.TenantId).ToDictionaryAsync(s => s.Id, ct);
        var recIds = lines.Select(l => l.ServiceRecordId).Distinct().ToList();
        var plates = await db.ServiceRecords.AsNoTracking().Where(r => recIds.Contains(r.Id))
            .Join(db.Vehicles, r => r.VehicleId, v => v.Id, (r, v) => new { r.Id, v.PlateNo }).ToDictionaryAsync(x => x.Id, x => x.PlateNo, ct);

        var items = ids.Where(sups.ContainsKey).Select(id => new SupplierDebtItem(id, sups[id].Name, sups[id].Phone,
                SupplierDebt.Of(lines, payments, id),
                lines.Where(l => l.SupplierId == id).OrderBy(l => l.DateTimeCreated)
                     .Select(l => new DebtLineDto(l.ServiceRecordId, l.Id, plates.GetValueOrDefault(l.ServiceRecordId, ""), l.Name, l.Cost ?? 0)).ToList(),
                payments.Where(p => p.SupplierId == id).OrderByDescending(p => p.PaidOn).ThenByDescending(p => p.DateTimeCreated)
                        .Select(p => new SupplierPaymentDto(p.Id, p.Amount, p.Method, p.PaidOn)).ToList()))
            // En çok borç üstte; borcu kapanmış olan altta kalır — son ödemesi geri alınabilsin.
            .OrderByDescending(i => i.Debt).ThenBy(i => i.Name).ToList();
        return new SupplierDebtList(items.Sum(i => i.Debt), items);
    }

    public async Task<SupplierDebtList> Handle(PaySupplier c, CancellationToken ct)
    {
        if (!await db.Suppliers.AnyAsync(s => s.Id == c.SupplierId, ct)) throw new NotFoundException("Parçacı");
        db.SupplierPayments.Add(SupplierPayment.Pay(tenant.TenantId, c.SupplierId, c.Amount, c.Method, clock.Today,
                                                    await DebtOf(c.SupplierId, ct)));
        await db.SaveChangesAsync(ct);
        return await Handle(new ListSupplierDebts(), ct);
    }

    public async Task<SupplierDebtList> Handle(UndoSupplierPayment c, CancellationToken ct)
    {
        var p = await db.SupplierPayments.FirstOrDefaultAsync(x => x.Id == c.PaymentId, ct) ?? throw new NotFoundException("Ödeme");
        p.Undo();
        await db.SaveChangesAsync(ct);
        return await Handle(new ListSupplierDebts(), ct);
    }
}
