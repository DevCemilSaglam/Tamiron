namespace Tamiron.Domain;

/// <summary>
/// Ruhsattan okunanlar (prototip ruhsatOku'nun eşlemesi). Hepsi boş olabilir. Vergi dairesi
/// YOK: ruhsatta yazmıyor, okunamayan alanı doldurmak uydurmaktır (K-050). Sahip adı ve
/// telefonu da yok — prototipin okuma eşlemesinde yer almıyor; usta yazar ya da söyler.
/// </summary>
public sealed record RegistrationRead(
    string? Plate = null, string? Brand = null, string? Model = null, string? Year = null, string? Engine = null,
    string? Color = null, string? FuelType = null, string? Vin = null, string? EngineNo = null,
    string? Displacement = null, string? PowerKw = null, string? Seats = null, string? FuelTankLiters = null,
    DateOnly? FirstRegistrationDate = null, string? TaxOrNationalId = null, string? Address = null);

public enum RegistrationOutcome { Save = 1, PlateConflict = 2, NoPlate = 3 }

public sealed record RegistrationDecision(RegistrationOutcome Outcome, string? Plate, string? TypedPlate, string? ReadPlate);

/// <summary>Ruhsat okununca ne olur — prototip 20-ruhsat ruhsatOku / ruhsatKaydet.</summary>
public static class RegistrationIntake
{
    /// <summary>
    /// K-069: usta plakayı yazmışsa ve okunan FARKLIYSA ustanınki kalır, araç seçim yapılana
    /// kadar kaydedilmez. Okunan plaka geçersizse yazılan kullanılır; ikisi de yoksa araç
    /// kaydedilmez (K-065.3 — plakasız kayıt yanlış araca bağlanır).
    /// </summary>
    public static RegistrationDecision Decide(string? typed, RegistrationRead read)
    {
        var t = Plate.Normalize(typed);
        var r = Plate.Normalize(read.Plate);
        bool tv = Plate.IsValid(t), rv = Plate.IsValid(r);
        string? T = tv ? t : null, R = rv ? r : null;
        if (tv && rv && t != r) return new(RegistrationOutcome.PlateConflict, null, T, R);
        var plate = R ?? T;
        return plate is null ? new(RegistrationOutcome.NoPlate, null, T, R) : new(RegistrationOutcome.Save, plate, T, R);
    }

    public static Vehicle NewVehicle(string plate, RegistrationRead read, int thisYear)
    {
        var v = new Vehicle { PlateNo = plate };
        FillEmpty(v, read, thisYear);
        return v;
    }

    /// <summary>
    /// K-065.3: kayıtlı araç yeniden okunursa üstüne YAZILMAZ, yalnız boş alanları dolar —
    /// ustanın düzelttiği bilgi modelin yeni okumasına yenilmez. Okunan biçimsiz değer
    /// (ör. imkânsız yıl) kaydedilmez, düşer. Dönen: dolan alanların Türkçe adları.
    /// </summary>
    public static IReadOnlyList<string> FillEmpty(Vehicle v, RegistrationRead r, int thisYear)
    {
        var filled = new List<string>();
        void F(string name, string? current, string? value, Action<string> set)
        {
            if (string.IsNullOrWhiteSpace(current) && value is not null) { set(value); filled.Add(name); }
        }
        F("marka", v.Brand, VehicleFields.Text(r.Brand), x => v.Brand = x);
        F("model", v.Model, VehicleFields.Text(r.Model), x => v.Model = x);
        F("yıl", v.Year, SafeYear(r.Year, thisYear), x => v.Year = x);
        F("motor", v.Engine, VehicleFields.Text(r.Engine), x => v.Engine = x);
        F("renk", v.Color, VehicleFields.Text(r.Color), x => v.Color = x);
        F("yakıt", v.FuelType, VehicleFields.Text(r.FuelType), x => v.FuelType = x);
        F("şase", v.Vin, VehicleFields.Code(r.Vin), x => v.Vin = x);
        F("motor no", v.EngineNo, VehicleFields.Code(r.EngineNo), x => v.EngineNo = x);
        F("silindir hacmi", v.Displacement, VehicleFields.Text(r.Displacement), x => v.Displacement = x);
        F("güç", v.PowerKw, VehicleFields.Text(r.PowerKw), x => v.PowerKw = x);
        F("koltuk", v.Seats, VehicleFields.Text(r.Seats), x => v.Seats = x);
        F("depo", v.FuelTankLiters, VehicleFields.Text(r.FuelTankLiters), x => v.FuelTankLiters = x);
        F("TC / vergi no", v.TaxOrNationalId, VehicleFields.Code(r.TaxOrNationalId), x => v.TaxOrNationalId = x);
        F("adres", v.Address, VehicleFields.Text(r.Address), x => v.Address = x);
        if (v.FirstRegistrationDate is null && r.FirstRegistrationDate is { } d) { v.FirstRegistrationDate = d; filled.Add("ilk tescil"); }
        return filled;
    }

    private static string? SafeYear(string? raw, int thisYear)
    {
        try { return VehicleFields.Year(raw, thisYear); }
        catch (DomainException) { return null; }
    }
}
