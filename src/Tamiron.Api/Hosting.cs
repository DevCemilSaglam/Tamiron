using Tamiron.Infrastructure;

namespace Tamiron.Api;

/// <summary>
/// Ortama göre açılış kararları — tek yerde, testli (G1, K-080.1).
/// <list type="bullet">
/// <item><b>Development</b>: geliştirici; prototipin tohumu, kod günlüğe.</item>
/// <item><b>Local</b>: ustanın kendi bilgisayarı (K-079.3 — sunucu yok). Boş veritabanı (TamironYerel),
/// göçler açılışta, SMS paketi yok → kod sunucu penceresine yazılır.</item>
/// <item>Diğerleri (Production/Testing): göç elle, SMS ancak hesap tamamsa.</item>
/// </list>
/// </summary>
public static class Hosting
{
    public const string Local = "Local";

    static bool IsDevOrLocal(string env) => env is "Development" or Local;

    /// <summary>
    /// Uygulama sayfası (index.html) her açılışta yeniden sorulur: başlıksız kalınca tarayıcı tahminle önbelleğe
    /// alıyor, güncellemeden sonra ESKİ sayfa açılıyordu (G11, tarayıcıda görüldü). Değişmediyse ETag'le 304 — hızlı.
    /// Öteki dosyalar dokunulmaz.
    /// </summary>
    public static void MarkAppPage(Microsoft.AspNetCore.Http.HttpResponse res, string fileName)
    {
        if (fileName == "index.html") res.Headers.CacheControl = "no-cache";
    }

    /// <summary>Geliştirmede ve yerelde HEP pencereye/günlüğe — hesap tanımlı olsa bile para harcanmaz.</summary>
    public static Type SmsSenderType(string env, NetgsmOptions? netgsm)
        => IsDevOrLocal(env) ? typeof(DevLogSmsSender)
         : netgsm is not null ? typeof(NetgsmSmsSender)
         : typeof(NotConfiguredSmsSender);

    public static bool MigratesOnStart(string env) => IsDevOrLocal(env);

    /// <summary>Prototipin tohumu yalnız geliştirmede; yerelde ustanın kendi dükkânı boş başlar.</summary>
    public static bool WritesDevSeed(string env) => env == "Development";
}
