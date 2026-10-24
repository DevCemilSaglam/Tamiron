using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// C9 · Tahsil günü kuralının İKİ biçimi var: bellekteki <c>CollectedOn</c> ve SQL'e çevrilen
/// <c>CollectedBetween</c> ifadesi. İkisi ayrışırsa ana ekran ile kasa farklı rakam söyler.
/// Bu test her durumda (teslim edilmemiş, teslimde ödenmiş, veresiye, sonradan ödenmiş,
/// dönem sınırları) iki biçimin aynı cevabı verdiğini ölçer.
/// </summary>
public class CollectedExpressionTests
{
    static readonly DateOnly D1 = new(2026, 9, 30), D2 = new(2026, 10, 1), D3 = new(2026, 10, 22), D4 = new(2026, 10, 23);

    static IEnumerable<ServiceRecord> States()
    {
        // Tutarlı iş: 0 ₺'lik işte tahsil edilecek kalan yoktur (K-078, prototip veresiyeMi).
        ServiceRecord New() { var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "iş"); r.AddLine(ServiceLine.Labor("İşçilik", 100m)); return r; }
        yield return New();                                                              // serviste
        foreach (var on in new[] { D1, D2, D3, D4 })
        {
            foreach (var m in new[] { PaymentMethod.Cash, PaymentMethod.Card, PaymentMethod.Transfer, PaymentMethod.Unpaid })
            {
                var r = New();
                r.Deliver(m, on, MaintenancePolicy.Default, []);
                yield return r;
            }
            foreach (var later in new[] { on, on.AddDays(1), on.AddDays(25) })
            {
                var r = New();
                r.Deliver(PaymentMethod.Unpaid, on, MaintenancePolicy.Default, []);
                r.CollectPayment(PaymentMethod.Cash, later);                             // sonradan ödendi
                yield return r;
            }
        }
    }

    /// <summary>K-078 · "Kalanı var mı"nın SQL biçimi bellekteki Remaining &gt; 0 ile her durumda aynı.</summary>
    [Fact]
    public void Remaining_sql_form_agrees_with_memory_form()
    {
        var has = ServiceRecord.HasRemaining.Compile();
        var states = States().ToList();
        // Kısmi ödemeli, müşteri parçalı, 0 ₺'lik ve kapanmış durumlar da.
        ServiceRecord Partial(decimal paid)
        {
            var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "iş");
            r.AddLine(ServiceLine.Part("Akü", 3200m)); r.AddLine(ServiceLine.Part("Getirdiği", 900m, PartSupplier.Customer));
            r.Deliver(PaymentMethod.Unpaid, D1, MaintenancePolicy.Default, []);
            if (paid > 0) r.CollectPayment(paid, PaymentMethod.Cash, D2);
            return r;
        }
        states.AddRange([Partial(0), Partial(1000m), Partial(3200m)]);
        var zero = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "iş");
        zero.Deliver(PaymentMethod.Unpaid, D1, MaintenancePolicy.Default, []);
        states.Add(zero);
        Assert.Contains(states, r => r.Remaining > 0);       // ölçü boş kalmasın
        Assert.Contains(states, r => r.DeliveredAt is not null && r.Remaining == 0);
        foreach (var r in states) Assert.Equal(r.Remaining > 0, has(r));
    }

    [Theory]
    [InlineData(2026, 10, 1, 2026, 10, 22)]    // bu ay
    [InlineData(2026, 10, 22, 2026, 10, 22)]   // bugün
    [InlineData(2026, 1, 1, 2026, 10, 22)]     // bu yıl
    [InlineData(2026, 9, 30, 2026, 9, 30)]     // tek gün, sınır
    public void Sql_form_agrees_with_memory_form_in_every_state(int y1, int m1, int d1, int y2, int m2, int d2)
    {
        var start = new DateOnly(y1, m1, d1);
        var end = new DateOnly(y2, m2, d2);
        var sql = ServiceRecord.CollectedBetween(start, end).Compile();
        foreach (var r in States())
            Assert.Equal(r.CollectedOn is { } c && c >= start && c <= end, sql(r));
    }
}
