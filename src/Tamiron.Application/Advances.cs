using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Advances;

// G7 · Müşteri avansı (K-079.1, TASARIM-G-AVANS). Bakiye SAHİBE göre: aynı sahip anahtarı (ad + telefon,
// A-M1 — müşteriler listesiyle aynı) olan araçların hareketleri + o araçların işlerinde avanstan düşülenler.

public sealed record AdvanceDto(Guid Id, AdvanceKind Kind, decimal Amount, PaymentMethod Method, DateOnly On, string PlateNo);
public sealed record AdvanceView(decimal Balance, IReadOnlyList<AdvanceDto> Items);

public sealed record GetAdvance(Guid VehicleId) : IRequest<AdvanceView>;
public sealed record ReceiveAdvance(Guid VehicleId, decimal Amount, PaymentMethod Method) : IRequest<AdvanceView>;
public sealed record RefundAdvance(Guid VehicleId, decimal Amount, PaymentMethod Method) : IRequest<AdvanceView>;
public sealed record UndoAdvance(Guid AdvanceId) : IRequest<AdvanceView>;

internal static class AdvanceQueries
{
    /// <summary>Aracın sahibinin araçları (sahip yazılmamışsa yalnız kendisi).</summary>
    public static async Task<List<Vehicle>> OwnersVehicles(ITamironDb db, Guid vehicleId, CancellationToken ct)
    {
        var v = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == vehicleId, ct) ?? throw new NotFoundException("Araç");
        if (string.IsNullOrWhiteSpace(v.OwnerName)) return [v];
        var key = Domain.Customers.KeyOf(v.OwnerName, v.OwnerPhone);
        // Anahtar Türkçe katlanmış ad — SQL'de kurulamaz; dükkânın araçları küçük, bellekte süzülür.
        return (await db.Vehicles.AsNoTracking().Where(x => x.OwnerName != null).ToListAsync(ct))
            .Where(x => Domain.Customers.KeyOf(x.OwnerName!, x.OwnerPhone) == key).ToList();
    }

    public static async Task<(decimal Balance, List<Advance> Movements, List<Vehicle> Vehicles)> Load(ITamironDb db, Guid vehicleId, CancellationToken ct)
    {
        var vehicles = await OwnersVehicles(db, vehicleId, ct);
        var ids = vehicles.Select(v => v.Id).ToList();
        var movements = await db.Advances.Where(a => ids.Contains(a.VehicleId)).ToListAsync(ct);
        var used = await db.Payments.AsNoTracking()
            .Where(p => p.Method == PaymentMethod.Advance
                     && db.ServiceRecords.Any(r => r.Id == p.ServiceRecordId && ids.Contains(r.VehicleId)))
            .SumAsync(p => p.Amount, ct);
        return (AdvanceBalance.Of(movements, used), movements, vehicles);
    }

    public static async Task<decimal> BalanceFor(ITamironDb db, Guid vehicleId, CancellationToken ct) => (await Load(db, vehicleId, ct)).Balance;

    public static AdvanceView View(decimal balance, IEnumerable<Advance> movements, IEnumerable<Vehicle> vehicles)
    {
        var plates = vehicles.ToDictionary(v => v.Id, v => v.PlateNo);
        return new(balance, movements.Where(a => a.DateTimeDeleted is null)
            .OrderBy(a => a.On).ThenBy(a => a.DateTimeCreated)
            .Select(a => new AdvanceDto(a.Id, a.Kind, a.Amount, a.Method, a.On, plates.GetValueOrDefault(a.VehicleId, ""))).ToList());
    }
}

internal sealed class AdvanceHandlers(ITamironDb db, ITenantContext tenant, IClock clock) :
    IRequestHandler<GetAdvance, AdvanceView>,
    IRequestHandler<ReceiveAdvance, AdvanceView>,
    IRequestHandler<RefundAdvance, AdvanceView>,
    IRequestHandler<UndoAdvance, AdvanceView>
{
    public async Task<AdvanceView> Handle(GetAdvance q, CancellationToken ct)
    {
        var (b, m, v) = await AdvanceQueries.Load(db, q.VehicleId, ct);
        return AdvanceQueries.View(b, m, v);
    }

    public async Task<AdvanceView> Handle(ReceiveAdvance c, CancellationToken ct)
    {
        await AdvanceQueries.OwnersVehicles(db, c.VehicleId, ct);   // araç var mı (kiracı süzgeciyle)
        db.Advances.Add(Advance.Receive(tenant.TenantId, c.VehicleId, c.Amount, c.Method, clock.Today));
        await db.SaveChangesAsync(ct);
        return await Handle(new GetAdvance(c.VehicleId), ct);
    }

    public async Task<AdvanceView> Handle(RefundAdvance c, CancellationToken ct)
    {
        var balance = await AdvanceQueries.BalanceFor(db, c.VehicleId, ct);
        db.Advances.Add(Advance.Refund(tenant.TenantId, c.VehicleId, c.Amount, c.Method, clock.Today, balance));
        await db.SaveChangesAsync(ct);
        return await Handle(new GetAdvance(c.VehicleId), ct);
    }

    public async Task<AdvanceView> Handle(UndoAdvance c, CancellationToken ct)
    {
        var a = await db.Advances.FirstOrDefaultAsync(x => x.Id == c.AdvanceId, ct) ?? throw new NotFoundException("Avans");
        a.Undo(await AdvanceQueries.BalanceFor(db, a.VehicleId, ct));
        await db.SaveChangesAsync(ct);
        return await Handle(new GetAdvance(a.VehicleId), ct);
    }
}
