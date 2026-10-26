using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Vehicles;

// B6 · Araç (prototip 60-arac, 20-ruhsat). Kayıtlar araca KİMLİKLE bağlı:
// plaka değişince kayıtların plakası kendiliğinden değişir.

public sealed record VehicleDto(
    Guid Id, string PlateNo, string? Brand, string? Model, string? Year, string? Engine, string? Vin,
    string? Color, string? FuelType, string? EngineNo, string? Displacement, string? PowerKw, string? Seats,
    DateOnly? FirstRegistrationDate, string? FuelTankLiters,
    string? OwnerName, string? OwnerPhone, string? ContactName, string? ContactPhone,
    string? TaxOrNationalId, string? Address, string? TaxOffice, string? Note,
    DateOnly? MarketingConsentAt, bool HasPartIdentity, IReadOnlyList<string> MissingForParts)
{
    public static VehicleDto From(Vehicle v) => new(
        v.Id, v.PlateNo, v.Brand, v.Model, v.Year, v.Engine, v.Vin, v.Color, v.FuelType, v.EngineNo, v.Displacement,
        v.PowerKw, v.Seats, v.FirstRegistrationDate, v.FuelTankLiters, v.OwnerName, v.OwnerPhone, v.ContactName,
        v.ContactPhone, v.TaxOrNationalId, v.Address, v.TaxOffice, v.Note, v.MarketingConsentAt,
        v.HasPartIdentity, v.MissingForParts);
}

/// <summary>
/// G2 · Kayıt açma ekranının plaka sorusu (prototip ruhsatDurum): plaka geçerli mi (ipucu prototipin
/// cümlesi), araç kayıtlı mı, serviste AÇIK kaydı var mı ("Açık kayda git"). Ekran karar vermez.
/// </summary>
public sealed record PlateLookup(string Plate, bool Valid, string? Hint, VehicleDto? Vehicle, Guid? OpenRecordId);
public sealed record LookupPlate(string? Plate) : IRequest<PlateLookup>;

public sealed record GetVehicle(Guid Id) : IRequest<VehicleDto>;
public sealed record FindVehicleByPlate(string Plate) : IRequest<VehicleDto>;

/// <summary>
/// Kısmi güncelleme: null = DOKUNMA, "" = TEMİZLE. Biçim ve geçerlilik domain'de
/// (VehicleFields). Tarih ISO ("2021-03-02") ya da "" (temizle).
/// </summary>
public sealed record PatchVehicle(Guid Id,
    string? PlateNo = null, string? Brand = null, string? Model = null, string? Year = null, string? Engine = null,
    string? Vin = null, string? Color = null, string? FuelType = null, string? EngineNo = null,
    string? Displacement = null, string? PowerKw = null, string? Seats = null, string? FirstRegistrationDate = null,
    string? FuelTankLiters = null, string? OwnerName = null, string? OwnerPhone = null, string? ContactName = null,
    string? ContactPhone = null, string? TaxOrNationalId = null, string? Address = null, string? TaxOffice = null,
    string? Note = null) : IRequest<VehicleDto>;

internal sealed class VehicleHandlers(ITamironDb db, IClock clock) :
    IRequestHandler<GetVehicle, VehicleDto>,
    IRequestHandler<FindVehicleByPlate, VehicleDto>,
    IRequestHandler<PatchVehicle, VehicleDto>,
    IRequestHandler<LookupPlate, PlateLookup>
{
    public async Task<PlateLookup> Handle(LookupPlate q, CancellationToken ct)
    {
        var p = Plate.Normalize(q.Plate);
        if (!Plate.IsValid(p))
            return new(p, false, string.IsNullOrWhiteSpace(p) ? null : "Plaka eksik — ör. 34 ABC 123", null, null);
        var v = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.PlateNo == p, ct);
        if (v is null) return new(p, true, null, null, null);
        // Açık kayıt = teslim edilmemiş (K-004). Birden çoksa en yenisi (K-076.4 ile aynı seçim).
        var open = await db.ServiceRecords.AsNoTracking().Where(r => r.VehicleId == v.Id && r.DeliveredAt == null)
            .OrderByDescending(r => r.DateTimeCreated).Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        return new(p, true, null, VehicleDto.From(v), open);
    }

    public async Task<VehicleDto> Handle(GetVehicle q, CancellationToken ct)
        => VehicleDto.From(await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == q.Id, ct)
                           ?? throw new NotFoundException("Araç"));

    public async Task<VehicleDto> Handle(FindVehicleByPlate q, CancellationToken ct)
    {
        var p = Plate.Normalize(q.Plate);
        return VehicleDto.From(await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.PlateNo == p, ct)
                               ?? throw new NotFoundException("Araç"));
    }

    public async Task<VehicleDto> Handle(PatchVehicle c, CancellationToken ct)
    {
        var v = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new NotFoundException("Araç");

        if (c.PlateNo is not null)
        {
            var p = Plate.Normalize(c.PlateNo);
            if (!Plate.IsValid(p))
                throw new DomainException("plate.invalid", "Önce plakayı yaz — ör. 34 ABC 123.");
            // Ruhsat düzeltmesindeki kural (K-065): plaka ancak başka araçla ÇAKIŞMIYORSA değişir.
            if (p != v.PlateNo && await db.Vehicles.AnyAsync(x => x.PlateNo == p && x.Id != v.Id, ct))
                throw new DomainException("plate.taken", $"{p} başka bir araçta kayıtlı.");
            v.PlateNo = p;
        }

        static void Set(string? input, Func<string, string?> norm, Action<string?> set)
        {
            if (input is not null) set(norm(input));
        }
        Set(c.Brand, VehicleFields.Text, x => v.Brand = x);
        Set(c.Model, VehicleFields.Text, x => v.Model = x);
        Set(c.Year, x => VehicleFields.Year(x, clock.Today.Year), x => v.Year = x);
        Set(c.Engine, VehicleFields.Text, x => v.Engine = x);
        Set(c.Vin, VehicleFields.Code, x => v.Vin = x);
        Set(c.EngineNo, VehicleFields.Code, x => v.EngineNo = x);
        Set(c.Color, VehicleFields.Text, x => v.Color = x);
        Set(c.FuelType, VehicleFields.Text, x => v.FuelType = x);
        Set(c.Displacement, VehicleFields.Text, x => v.Displacement = x);
        Set(c.PowerKw, VehicleFields.Text, x => v.PowerKw = x);
        Set(c.Seats, VehicleFields.Text, x => v.Seats = x);
        Set(c.FuelTankLiters, VehicleFields.Text, x => v.FuelTankLiters = x);
        Set(c.OwnerName, VehicleFields.Text, x => v.OwnerName = x);
        Set(c.OwnerPhone, VehicleFields.Phone, x => v.OwnerPhone = x);
        Set(c.ContactName, VehicleFields.Text, x => v.ContactName = x);
        Set(c.ContactPhone, VehicleFields.Phone, x => v.ContactPhone = x);
        Set(c.TaxOrNationalId, VehicleFields.Code, x => v.TaxOrNationalId = x);
        Set(c.Address, VehicleFields.Text, x => v.Address = x);
        Set(c.TaxOffice, VehicleFields.Text, x => v.TaxOffice = x);
        Set(c.Note, VehicleFields.Text, x => v.Note = x);
        if (c.FirstRegistrationDate is not null)
            v.FirstRegistrationDate = c.FirstRegistrationDate.Length == 0 ? null
                : DateOnly.TryParse(c.FirstRegistrationDate, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d
                : throw new DomainException("vehicle.date_invalid", "İlk tescil tarihini gün.ay.yıl olarak yaz.");

        await db.SaveChangesAsync(ct);
        return VehicleDto.From(v);
    }
}
