using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// D17 · İndeksler (K-076.16) göçle kurulur. Test veritabanı göçlerle oluştuğu için burada
/// görünen indeks üretimde de var demektir. Sıcak sorgular: kasa/teslim/alacak (dükkân + teslim
/// günü), sonradan tahsil (tahsil günü), hatırlatması gelenler (gönderilmemiş hatırlatma).
/// </summary>
public class IndexTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    sealed record Ix(string Name, string Columns, string? Filter);

    async Task<List<Ix>> Indexes(string table)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
        return await db.Database.SqlQueryRaw<Ix>(@"
            SELECT i.name AS Name,
                   STRING_AGG(c.name, ',') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columns,
                   i.filter_definition AS Filter
            FROM sys.indexes i
            JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
            JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE i.object_id = OBJECT_ID({0})
            GROUP BY i.name, i.filter_definition", table).ToListAsync();
    }

    [Fact]
    public async Task Service_records_have_indexes_for_the_hot_queries()
    {
        var ix = await Indexes("ServiceRecords");
        Assert.Contains(ix, i => i.Columns == "TenantId,DeliveredAt");
        Assert.Contains(ix, i => i.Columns == "TenantId,PaymentCollectedAt" && i.Filter == "([PaymentCollectedAt] IS NOT NULL)");
        Assert.Contains(ix, i => i.Columns == "TenantId,ReminderDate" && i.Filter == "([ReminderDate] IS NOT NULL AND [ReminderSent]=(0))");
    }
}
