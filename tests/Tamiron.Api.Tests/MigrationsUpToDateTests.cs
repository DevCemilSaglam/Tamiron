using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// D17 · "Modeli değiştirdim, göç üretmeyi unuttum" hatası (K-076.16). Testler veritabanını
/// GÖÇLERDEN kurar: göçü olmayan model değişikliği (indeks, sütun, süzgeç) hiçbir testte
/// görünmez, üretimde de veritabanına hiç gelmez. Mutasyonla bulundu: indeks silinince
/// testler yeşil kalıyordu. Güncel model ile son göçün anlık görüntüsü arasında fark olmamalı.
/// </summary>
public class MigrationsUpToDateTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public void Model_has_no_changes_without_a_migration()
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
        var snapshot = db.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
        if (snapshot is IMutableModel m) snapshot = m.FinalizeModel();
        snapshot = db.GetService<IModelRuntimeInitializer>().Initialize(snapshot);
        var diff = db.GetService<IMigrationsModelDiffer>().GetDifferences(
            snapshot.GetRelationalModel(), db.GetService<IDesignTimeModel>().Model.GetRelationalModel());
        Assert.True(diff.Count == 0, "Göçü olmayan model değişikliği: " + string.Join(", ", diff.Select(d => d.GetType().Name)) +
            " — dotnet tool run dotnet-ef migrations add <Ad> …");
    }
}
