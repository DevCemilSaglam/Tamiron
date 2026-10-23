using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Application.Vehicles;
using Tamiron.Domain;

namespace Tamiron.Application.Registrations;

// D8 · Ruhsat okuma (prototip 20-ruhsat). Araç okunduğu AN kaydedilir (K-065); plaka
// çatışmasında usta seçer (K-069). Kayıt açmak AYRI adım: POST /records.

public sealed record RegistrationResult(RegistrationOutcome Outcome, string? TypedPlate, string? ReadPlate,
                                        RegistrationRead Read, bool VinRead, bool Created,
                                        IReadOnlyList<string> Filled, VehicleDto? Vehicle);

/// <summary>Fotoğrafı okut. <paramref name="TypedPlate"/>: ustanın okutmadan ÖNCE yazdığı plaka (K-069).</summary>
public sealed record ReadRegistration(byte[] Image, string MediaType, string? TypedPlate) : IRequest<RegistrationResult>;

/// <summary>
/// Okunanı seçilen plakayla kaydet — çatışma çözülünce ya da plaka okunamayıp usta yazınca.
/// Fotoğraf YENİDEN okutulmaz (para): okunanlar istemcide durur, usta düzeltmiş olabilir.
/// </summary>
public sealed record SaveRegistration(string Plate, RegistrationRead Read) : IRequest<RegistrationResult>;

internal sealed class RegistrationHandlers(ITamironDb db, ITenantContext tenant, IClock clock, IRegistrationReader reader) :
    IRequestHandler<ReadRegistration, RegistrationResult>,
    IRequestHandler<SaveRegistration, RegistrationResult>
{
    public const int MaxBytes = 10 * 1024 * 1024;
    private static readonly string[] Types = ["image/jpeg", "image/png", "image/webp", "image/heic", "image/heif"];

    public async Task<RegistrationResult> Handle(ReadRegistration c, CancellationToken ct)
    {
        if (c.Image.Length == 0 || !Types.Contains(c.MediaType))
            throw new DomainException("registration.photo_invalid", "Ruhsatın fotoğrafını çek ya da galeriden seç.");
        if (c.Image.Length > MaxBytes)
            throw new DomainException("registration.photo_too_big", "Fotoğraf çok büyük. Yeniden çek.");

        var read = await reader.ReadAsync(c.Image, c.MediaType, ct);
        var d = RegistrationIntake.Decide(c.TypedPlate, read);
        if (d.Outcome != RegistrationOutcome.Save) return new(d.Outcome, d.TypedPlate, d.ReadPlate, read, VinRead(read), false, [], null);
        return await Save(d.Plate!, read, d, ct);
    }

    public async Task<RegistrationResult> Handle(SaveRegistration c, CancellationToken ct)
    {
        var p = Plate.Normalize(c.Plate);
        if (!Plate.IsValid(p)) throw new DomainException("plate.invalid", "Önce plakayı yaz — ör. 34 ABC 123.");
        return await Save(p, c.Read, new(RegistrationOutcome.Save, p, p, null), ct);
    }

    private async Task<RegistrationResult> Save(string plate, RegistrationRead read, RegistrationDecision d, CancellationToken ct)
    {
        var v = await db.Vehicles.FirstOrDefaultAsync(x => x.PlateNo == plate, ct);
        var created = v is null;
        IReadOnlyList<string> filled;
        if (v is null)
        {
            v = RegistrationIntake.NewVehicle(plate, read, clock.Today.Year);
            v.TenantId = tenant.TenantId;
            db.Vehicles.Add(v);
            filled = [];
        }
        else filled = RegistrationIntake.FillEmpty(v, read, clock.Today.Year);
        await db.SaveChangesAsync(ct);
        return new(RegistrationOutcome.Save, d.TypedPlate, d.ReadPlate, read, VinRead(read), created, filled, VehicleDto.From(v));
    }

    /// <summary>Balon "şase okunamadı" diyebilsin (prototip ruhsatKaydet).</summary>
    private static bool VinRead(RegistrationRead r) => new Vehicle { Vin = r.Vin }.HasValidVin;
}
