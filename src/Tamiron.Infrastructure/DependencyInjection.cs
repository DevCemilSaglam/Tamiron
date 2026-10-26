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

/// <summary>Üretimde sağlayıcı bağlanana kadar: gönderim YAPILMAZ ve bu açıkça söylenir (K-074.5).</summary>
public sealed class NotConfiguredSmsSender : ISmsSender
{
    public Task SendAsync(string phone, string text, CancellationToken ct) => throw new SmsNotConfiguredException();
}

/// <summary>Okuyucu bağlanana kadar: fotoğraf hiçbir yere GİTMEZ, usta elle yazar (D8).</summary>
public sealed class NotConfiguredRegistrationReader : IRegistrationReader
{
    public Task<Domain.RegistrationRead> ReadAsync(byte[] image, string mediaType, CancellationToken ct)
        => throw new ReaderNotConfiguredException();
}

/// <summary>GELİŞTİRME ve YEREL kurulum (K-080.1): SMS gitmez, kod sunucu penceresine/günlüğe yazılır.</summary>
public sealed class DevLogSmsSender(Microsoft.Extensions.Logging.ILogger<DevLogSmsSender> log) : ISmsSender
{
    public Task SendAsync(string phone, string text, CancellationToken ct)
    {
        Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(log, "SMS GÖNDERİLMEDİ (paket yok) — kod burada → {Phone}: {Text}", phone, text);
        return Task.CompletedTask;
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TamironDbContext>(o => o.UseSqlServer(connectionString));
        services.AddScoped<ITamironDb>(sp => sp.GetRequiredService<TamironDbContext>());
        services.AddSingleton<IClock, TurkeyClock>();
        services.AddSingleton<IRegistrationReader, NotConfiguredRegistrationReader>();
        return services;
    }
}
