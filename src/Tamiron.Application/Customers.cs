using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Customers;

// B10 · Müşteriler (prototip 90-musteriler, 91-musteri) ve ticari ileti izni (K-071.3).
// Müşteri türetilir: her istekte dükkânın araçlarından gruplanır.

public sealed record CustomerListItem(string Key, string Name, string? Phone, string DisplayPlate,
                                      int VehicleCount, bool InShop, DateOnly? ConsentAt);
public sealed record CustomerList(int Count, IReadOnlyList<CustomerListItem> Items);
public sealed record CustomerVehicle(Guid Id, string PlateNo, string? Brand, string? Model, bool InShop);
public sealed record CustomerDetail(string Key, string Name, string? Phone, DateOnly? ConsentAt,
                                    IReadOnlyList<CustomerVehicle> Vehicles);

public sealed record ListCustomers(string? Q = null) : IRequest<CustomerList>;
public sealed record GetCustomer(string Key) : IRequest<CustomerDetail>;

/// <summary>
/// İzni ver / kaldır. Gün SUNUCUDAN; istemci tarih göndermez (izin tarihi ispattır).
/// Her değişiklik izine yazılır (D5).
/// </summary>
public sealed record SetConsent(string Key, bool Granted) : IRequest<CustomerDetail>;

/// <summary>
/// "Geri al" — prototip gibi kaldırılan izin ESKİ TARİHİYLE döner; tarih izden gelir,
/// istemciden değil (D5 · K-073.10'u kapatır).
/// </summary>
public sealed record UndoConsent(string Key) : IRequest<CustomerDetail>;

public sealed record ConsentEventDto(DateTime At, string PlateNo, string Phone, bool Granted,
                                     DateOnly? PreviousDate, DateOnly? ConsentDate, bool IsUndo, string? By);
/// <summary>İzin geçmişi — en yeni üstte. "Bu numaraya ne zaman izin verildi" sorusunun ispatı.</summary>
public sealed record GetConsentHistory(string Key) : IRequest<List<ConsentEventDto>>;

internal sealed class CustomerHandlers(ITamironDb db, IClock clock, ICurrentSession session) :
    IRequestHandler<ListCustomers, CustomerList>,
    IRequestHandler<GetCustomer, CustomerDetail>,
    IRequestHandler<SetConsent, CustomerDetail>,
    IRequestHandler<UndoConsent, CustomerDetail>,
    IRequestHandler<GetConsentHistory, List<ConsentEventDto>>
{
    private async Task<(List<Customer> all, HashSet<Guid> inShop)> Load(bool tracking, CancellationToken ct)
    {
        var q = tracking ? db.Vehicles : db.Vehicles.AsNoTracking();
        // Araç eklenme sırası korunur: müşterinin "ilk aracı" ilk kaydedilendir.
        var vehicles = await q.OrderBy(v => v.DateTimeCreated).ToListAsync(ct);
        var inShop = (await db.ServiceRecords.AsNoTracking().Where(r => r.DeliveredAt == null)
                                             .Select(r => r.VehicleId).ToListAsync(ct)).ToHashSet();
        return (Domain.Customers.From(vehicles), inShop);
    }

    private static CustomerDetail Detail(Customer c, HashSet<Guid> inShop) => new(
        c.Key, c.Name, c.Phone, c.ConsentAt,
        c.Vehicles.Select(v => new CustomerVehicle(v.Id, v.PlateNo, v.Brand, v.Model, inShop.Contains(v.Id))).ToList());

    public async Task<CustomerList> Handle(ListCustomers q, CancellationToken ct)
    {
        var (all, inShop) = await Load(false, ct);
        bool InShop(Customer c) => c.Vehicles.Any(v => inShop.Contains(v.Id));
        var items = all
            // Arama ad, telefon, plaka ve markada (prototip musteriListesi).
            .Where(c => TextSearch.Matches(q.Q, [c.Name, c.Phone, .. c.Vehicles.Select(v => v.PlateNo + " " + v.Brand)]))
            // Serviste olanlar üstte (Cemil, 14.09) — From() zaten alfabe sırasında, OrderBy kararlıdır.
            .OrderBy(c => InShop(c) ? 0 : 1)
            .Select(c => new CustomerListItem(c.Key, c.Name, c.Phone, c.DisplayVehicle(v => inShop.Contains(v.Id)).PlateNo,
                                              c.Vehicles.Count, InShop(c), c.ConsentAt))
            .ToList();
        return new CustomerList(items.Count, items);
    }

    public async Task<CustomerDetail> Handle(GetCustomer q, CancellationToken ct)
    {
        var (all, inShop) = await Load(false, ct);
        return Detail(all.FirstOrDefault(c => c.Key == q.Key) ?? throw new NotFoundException("Müşteri"), inShop);
    }

    public async Task<CustomerDetail> Handle(SetConsent c, CancellationToken ct)
    {
        var (all, inShop) = await Load(true, ct);
        var cust = all.FirstOrDefault(x => x.Key == c.Key) ?? throw new NotFoundException("Müşteri");
        db.ConsentEvents.AddRange(cust.SetConsent(c.Granted, clock.Today, session.UserId));
        await db.SaveChangesAsync(ct);
        return Detail(cust, inShop);
    }

    public async Task<CustomerDetail> Handle(UndoConsent c, CancellationToken ct)
    {
        var (all, inShop) = await Load(true, ct);
        var cust = all.FirstOrDefault(x => x.Key == c.Key) ?? throw new NotFoundException("Müşteri");
        db.ConsentEvents.AddRange(cust.UndoConsent(await History(cust, ct), clock.Today, session.UserId));
        await db.SaveChangesAsync(ct);
        return Detail(cust, inShop);
    }

    public async Task<List<ConsentEventDto>> Handle(GetConsentHistory q, CancellationToken ct)
    {
        var (all, _) = await Load(false, ct);
        var cust = all.FirstOrDefault(x => x.Key == q.Key) ?? throw new NotFoundException("Müşteri");
        var plates = cust.Vehicles.ToDictionary(v => v.Id, v => v.PlateNo);
        var events = await History(cust, ct);
        var userIds = events.Select(e => e.UserId).Distinct().ToList();
        var names = await db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Name ?? u.Phone, ct);   // adı yazılmamışsa telefonu
        return events.AsEnumerable().Reverse().Select(e => new ConsentEventDto(e.DateTimeCreated, plates[e.VehicleId], e.Phone,
            e.Granted, e.PreviousDate, e.ConsentDate, e.IsUndo, names.GetValueOrDefault(e.UserId))).ToList();
    }

    /// <summary>Müşterinin araçlarının izleri, eskiden yeniye (aynı anda yazılanlar eklenme sırasıyla).</summary>
    private async Task<List<ConsentEvent>> History(Customer cust, CancellationToken ct)
    {
        var ids = cust.Vehicles.Select(v => v.Id).ToList();
        return await db.ConsentEvents.AsNoTracking().Where(e => ids.Contains(e.VehicleId))
            .OrderBy(e => e.DateTimeCreated).ToListAsync(ct);
    }
}
