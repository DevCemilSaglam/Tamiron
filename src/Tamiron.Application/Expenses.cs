using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Expenses;

// B8 · Gider (prototip 95-muhasebe). Liste DÖNEME tabi (alacak değil, o bakiye).
// ⚠️ Kasa özetinden gider DÜŞÜLMEZ: "Kalan" satırı K-014'te bilerek kaldırıldı —
// eksik giderle çıkan rakam kâr gibi okunur ve kârdan kötüdür. Toplam burada,
// kendi listesinde.
// Silme/geri alma her biri dönemin GÜNCEL listesini döner (period, varsayılan Month).

public sealed record ExpenseDto(Guid Id, DateOnly Date, string Description, decimal Amount);
public sealed record ExpenseList(PeriodKind Period, DateOnly Start, DateOnly End, decimal Total, IReadOnlyList<ExpenseDto> Items);

public sealed record ListExpenses(PeriodKind Period) : IRequest<ExpenseList>;
public sealed record AddExpense(string Description, decimal Amount) : IRequest<ExpenseDto>;
public sealed record DeleteExpense(Guid Id, PeriodKind Period) : IRequest<ExpenseList>;
public sealed record RestoreExpense(Guid Id, PeriodKind Period) : IRequest<ExpenseList>;

internal sealed class ExpenseHandlers(ITamironDb db, IClock clock, ITenantContext tenant, ISender sender) :
    IRequestHandler<ListExpenses, ExpenseList>,
    IRequestHandler<AddExpense, ExpenseDto>,
    IRequestHandler<DeleteExpense, ExpenseList>,
    IRequestHandler<RestoreExpense, ExpenseList>
{
    public async Task<ExpenseList> Handle(ListExpenses q, CancellationToken ct)
    {
        var p = Period.Of(q.Period, clock.Today);
        var items = await db.Expenses.AsNoTracking()
            .Where(e => e.Date >= p.Start && e.Date <= p.End)
            // En yeni üstte; aynı gün eklenenlerde son eklenen üstte (prototip reverse()).
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.DateTimeCreated)
            .Select(e => new ExpenseDto(e.Id, e.Date, e.Description, e.Amount)).ToListAsync(ct);
        return new ExpenseList(q.Period, p.Start, p.End, items.Sum(i => i.Amount), items);
    }

    public async Task<ExpenseDto> Handle(AddExpense c, CancellationToken ct)
    {
        var e = Expense.Create(tenant.TenantId, c.Description, c.Amount, clock.Today);   // gün sunucudan
        db.Expenses.Add(e);
        await db.SaveChangesAsync(ct);
        return new ExpenseDto(e.Id, e.Date, e.Description, e.Amount);
    }

    public async Task<ExpenseList> Handle(DeleteExpense c, CancellationToken ct)
    {
        var e = await db.Expenses.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new NotFoundException("Gider");
        db.Expenses.Remove(e);   // soft-delete (altyapı)
        await db.SaveChangesAsync(ct);
        return await sender.Send(new ListExpenses(c.Period), ct);
    }

    public async Task<ExpenseList> Handle(RestoreExpense c, CancellationToken ct)
    {
        // Süzgeç kapatılınca kiracı süzgeci de kapanır — kiracı AÇIKÇA yazılır.
        var e = await db.Expenses.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == c.Id && x.TenantId == tenant.TenantId && x.DateTimeDeleted != null, ct)
                ?? throw new NotFoundException("Silinmiş gider");
        e.DateTimeDeleted = null;
        await db.SaveChangesAsync(ct);
        return await sender.Send(new ListExpenses(c.Period), ct);
    }
}
