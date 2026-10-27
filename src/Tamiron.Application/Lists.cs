using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Lists;

// B7 · Listeler (prototip 80-kayitlar, 70-araclar). Sıra her listede AYNI:
// serviste olanlar üstte (Cemil, 04.09) — usta üç listede üç ayrı sıra öğrenmesin.
// Arama Türkçe katlamalı (TextSearch); SQL'in harf karşılaştırması "ayse"yi
// "Ayşe"de bulmaz. Bir dükkânın kayıtları için bellekte süzmek ucuz; büyürse
// arama sütunu (katlanmış metin) eklenir.

public sealed record RecordListItem(Guid Id, Guid VehicleId, string PlateNo, ServiceType Type, string TypeLabel,
                                    string Complaint, DateOnly OpenedOn, DateOnly? DeliveredAt,
                                    PaymentMethod? PaymentMethod, bool WaitingForPart, decimal Total,
                                    string? Brand, string? Model, string? OwnerName,
                                    string? Km = null, DateOnly? ReminderDate = null);

public sealed record RecordList(int InShopCount, int Count, IReadOnlyList<RecordListItem> Items);

/// <param name="VehicleId">G9 · Araç ekranının geçmişi: aracın KİMLİĞİYLE (plaka metin araması "06 AB 123"te "06 AB 1234"ü de bulurdu).</param>
public sealed record ListRecords(string? Q = null, bool InShop = false, Guid? VehicleId = null) : IRequest<RecordList>;

public sealed record VehicleListItem(Guid Id, string PlateNo, string? Brand, string? Model,
                                     string? OwnerName, string? ContactName, bool InShop);

public sealed record VehicleList(int Count, IReadOnlyList<VehicleListItem> Items);

public sealed record ListVehicles(string? Q = null) : IRequest<VehicleList>;

internal sealed class ListHandlers(ITamironDb db, IClock clock) :
    IRequestHandler<ListRecords, RecordList>,
    IRequestHandler<ListVehicles, VehicleList>
{
    public async Task<RecordList> Handle(ListRecords q, CancellationToken ct)
    {
        var records = await db.ServiceRecords.AsNoTracking().Include(r => r.Lines).ToListAsync(ct);
        var vehicles = await db.Vehicles.AsNoTracking().ToDictionaryAsync(v => v.Id, ct);

        var items = records
            // Süzgeç önce: "yalnız servistekiler" dendiyse arama boş olsa da liste daralır.
            .Where(r => !q.InShop || r.DeliveredAt is null)
            .Where(r => q.VehicleId is null || r.VehicleId == q.VehicleId)
            .Where(r =>
            {
                vehicles.TryGetValue(r.VehicleId, out var v);
                return TextSearch.Matches(q.Q, v?.PlateNo, r.Complaint, ServiceTypeGuess.Label(r.Type),
                                          v?.Brand, v?.Model, v?.OwnerName);
            })
            .OrderBy(r => r.DeliveredAt is null ? 0 : 1)
            .ThenByDescending(r => r.DateTimeCreated)
            .Select(r =>
            {
                vehicles.TryGetValue(r.VehicleId, out var v);
                return new RecordListItem(r.Id, r.VehicleId, v?.PlateNo ?? "", r.Type, ServiceTypeGuess.Label(r.Type),
                    r.Complaint, clock.LocalDate(r.DateTimeCreated), r.DeliveredAt, r.PaymentMethod, r.WaitingForPart,
                    r.Total, v?.Brand, v?.Model, v?.OwnerName, r.Km, r.ReminderDate);
            }).ToList();

        return new RecordList(items.Count(i => i.DeliveredAt is null), items.Count, items);
    }

    public async Task<VehicleList> Handle(ListVehicles q, CancellationToken ct)
    {
        var inShop = (await db.ServiceRecords.AsNoTracking().Where(r => r.DeliveredAt == null)
                                             .Select(r => r.VehicleId).ToListAsync(ct)).ToHashSet();
        var items = (await db.Vehicles.AsNoTracking().ToListAsync(ct))
            // Arama ikinci iletişimde de: usta plakayı değil, arayan kişinin adını hatırlıyor olabilir.
            .Where(v => TextSearch.Matches(q.Q, v.PlateNo, v.Brand, v.Model, v.OwnerName, v.ContactName))
            .OrderBy(v => inShop.Contains(v.Id) ? 0 : 1)
            .ThenBy(v => v.DateTimeCreated)
            .Select(v => new VehicleListItem(v.Id, v.PlateNo, v.Brand, v.Model, v.OwnerName, v.ContactName, inShop.Contains(v.Id)))
            .ToList();
        return new VehicleList(items.Count, items);
    }
}
