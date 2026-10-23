using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>D13 · Yanlış açılan kayıt silinir; teslim edilmiş kayıt SİLİNMEZ (kasada izi var) — K-076.12.</summary>
public class RecordDeleteTests
{
    [Fact]
    public void Open_record_can_be_deleted()
        => new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren").EnsureDeletable();

    [Fact]
    public void Delivered_record_cannot_be_deleted()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren");
        r.Deliver(PaymentMethod.Cash, new DateOnly(2026, 10, 22), MaintenancePolicy.Default, []);
        var e = Assert.Throws<DomainException>(r.EnsureDeletable);
        Assert.Equal("record.delivered_cannot_delete", e.Code);
    }
}
