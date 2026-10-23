using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// D8 · Ruhsat okununca (K-065 · K-069 · K-068). Beklenen davranış prototipin
/// ruhsatOku / ruhsatKaydet'i (20-ruhsat.html).
/// </summary>
public class RegistrationIntakeTests
{
    static readonly RegistrationRead Egea = new(Plate: "34abc123", Brand: "FIAT", Model: "EGEA", Year: "2021", Engine: "1.4 Fire",
        Color: "Beyaz", Vin: "zfa35600006123456", EngineNo: "350a1000", FirstRegistrationDate: new DateOnly(2021, 3, 2));

    [Fact]
    public void Typed_plate_differs_from_read_plate_is_a_conflict_and_nothing_is_saved()
    {
        var d = RegistrationIntake.Decide("34 ABE 123", Egea);   // usta E yazdı, ruhsatta C okundu (K-069)
        Assert.Equal((RegistrationOutcome.PlateConflict, "34 ABE 123", "34 ABC 123"), (d.Outcome, d.TypedPlate, d.ReadPlate));
        Assert.Null(d.Plate);
    }

    [Fact]
    public void Same_plate_or_no_typed_plate_saves_with_read_plate()
    {
        Assert.Equal((RegistrationOutcome.Save, "34 ABC 123"), (RegistrationIntake.Decide("34 abc 123", Egea).Outcome, RegistrationIntake.Decide("34 abc 123", Egea).Plate));
        Assert.Equal("34 ABC 123", RegistrationIntake.Decide(null, Egea).Plate);
        Assert.Equal("34 ABC 123", RegistrationIntake.Decide("34 A", Egea).Plate);   // yazılan geçersiz: çatışma değil
    }

    [Fact]
    public void Unreadable_plate_falls_back_to_typed_plate_or_saves_nothing()
    {
        var okunamadi = Egea with { Plate = "34 ?" };
        Assert.Equal((RegistrationOutcome.Save, "06 TMR 07"), (RegistrationIntake.Decide("06 tmr 07", okunamadi).Outcome, RegistrationIntake.Decide("06 tmr 07", okunamadi).Plate));
        Assert.Equal(RegistrationOutcome.NoPlate, RegistrationIntake.Decide("", okunamadi).Outcome);   // plakasız kayıt yanlış araca bağlanır
    }

    [Fact]
    public void New_vehicle_gets_every_read_field_normalized()
    {
        var v = RegistrationIntake.NewVehicle("34 ABC 123", Egea, 2026);
        Assert.Equal(("34 ABC 123", "FIAT", "2021", "ZFA35600006123456", "350A1000", new DateOnly(2021, 3, 2)),
            (v.PlateNo, v.Brand, v.Year, v.Vin, v.EngineNo, v.FirstRegistrationDate!.Value));
        Assert.True(v.HasPartIdentity);   // şase 17 hane (K-068)
    }

    [Fact]
    public void Known_vehicle_only_fills_empty_fields()
    {
        var v = new Vehicle { PlateNo = "34 ABC 123", Brand = "Fiat", Model = "Egea", Color = null, OwnerName = "Mehmet" };
        var filled = RegistrationIntake.FillEmpty(v, Egea with { Brand = "TOFAŞ", Model = "Doblo" }, 2026);
        Assert.Equal(("Fiat", "Egea", "Beyaz", "Mehmet"), (v.Brand, v.Model, v.Color, v.OwnerName));   // ustanınki yenilmez
        Assert.Contains("renk", filled);
        Assert.DoesNotContain("marka", filled);
    }

    [Fact]
    public void Impossible_year_is_dropped_not_saved()
        => Assert.Null(RegistrationIntake.NewVehicle("34 ABC 123", Egea with { Year = "2099" }, 2026).Year);
}
