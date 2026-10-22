using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tamiron.Application;

namespace Tamiron.Infrastructure;

/// <summary>
/// `dotnet ef migrations` için. Göç üretirken kiracı yok; süzgeç ifadesi
/// modelde yer alır ama hiçbir sorgu koşmaz.
/// </summary>
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<TamironDbContext>
{
    public TamironDbContext CreateDbContext(string[] args)
    {
        var o = new DbContextOptionsBuilder<TamironDbContext>()
            .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=TamironDev;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new TamironDbContext(o, new NoTenant(), new TurkeyClock());
    }

    private sealed class NoTenant : ITenantContext { public Guid TenantId => Guid.Empty; }
}
