using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application;

namespace Tamiron.Infrastructure;

/// <summary>Ustanın günü Türkiye saatiyle: gece 01:00'de UTC hâlâ dün.</summary>
public sealed class TurkeyClock : IClock
{
    private static readonly TimeZoneInfo Tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => LocalDate(DateTime.UtcNow);
    public DateOnly LocalDate(DateTime utc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Tz));
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TamironDbContext>(o => o.UseSqlServer(connectionString));
        services.AddScoped<ITamironDb>(sp => sp.GetRequiredService<TamironDbContext>());
        services.AddSingleton<IClock, TurkeyClock>();
        return services;
    }
}
