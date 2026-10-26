using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tamiron.Ui.Core;

// D20 · Arayüzün API'yle tek konuşma yeri (K-076.19). Karar ARAYÜZDE DEĞİL: istemci API'nin
// söylediğini gösterir (CLAUDE.md "iş mantığı API'de"). DTO'lar burada küçük kopyalar —
// Application'a başvurmak tarayıcıya EF Core taşırdı.

public sealed record LoginResult(string Token, string ShopName, string? UserName);

public sealed record RecordListItem(Guid Id, Guid VehicleId, string PlateNo, string Type, string TypeLabel,
                                    string Complaint, DateOnly OpenedOn, DateOnly? DeliveredAt,
                                    string? PaymentMethod, bool WaitingForPart, decimal Total,
                                    string? Brand, string? Model, string? OwnerName);

public sealed record RecordList(int InShopCount, int Count, IReadOnlyList<RecordListItem> Items);

/// <summary>G2 · Plaka sorusunun cevabı (API LookupPlate): geçerli mi, ipucu, kayıtlı araç, açık kaydı.</summary>
public sealed record PlateLookup(string Plate, bool Valid, string? Hint, VehicleBrief? Vehicle, Guid? OpenRecordId);

public sealed record VehicleBrief(Guid Id, string PlateNo, string? Brand, string? Model, string? OwnerName, bool HasPartIdentity);

/// <summary>Kayıtlı olmayan araç için ekranda yazılanlar — kayıtla AYNI istekte gider (API NewVehicleInfo).</summary>
public sealed record NewVehicleInfo(string? Brand, string? Model, string? Year, string? Engine, string? Color,
                                    string? FuelType, string? Vin, string? OwnerName, string? OwnerPhone);

public sealed record OpenedRecord(Guid Id, Guid VehicleId, string PlateNo);

public sealed record HomeSummary(int InShopCount, int RecordCount, int VehicleCount, int CustomerCount,
                                 decimal CollectedToday, decimal Receivable, int DueReminderCount);

/// <summary>API'nin Türkçe hatası (Problem Details: code + title). Ekran <see cref="Exception.Message"/>'ı olduğu gibi gösterir.</summary>
public sealed class ApiException(HttpStatusCode status, string? code, string message) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
    public string? Code { get; } = code;
}

/// <summary>Oturum anahtarı — süresiz (K-074); cihazda saklanır, yalnız çıkışla silinir.</summary>
public interface ITokenStore
{
    ValueTask<string?> GetAsync();
    ValueTask SetAsync(string? token);
}

public sealed class TamironApi(HttpClient http, ITokenStore tokens)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public async Task RequestCodeAsync(string phone) => await Send(HttpMethod.Post, "api/auth/code", new { phone }, auth: false);

    public async Task<LoginResult> VerifyAsync(string phone, string code, string? device)
    {
        var r = await Read<LoginResult>(await Send(HttpMethod.Post, "api/auth/verify", new { phone, code, deviceName = device }, auth: false));
        await tokens.SetAsync(r.Token);
        return r;
    }

    public async Task<bool> HasSessionAsync() => !string.IsNullOrEmpty(await tokens.GetAsync());

    /// <summary>G1 · Yeni dükkân (D6): kayıtlı olmayan numaraya kod; kod + dükkân adı → oturum.</summary>
    public async Task RequestSignupCodeAsync(string phone) => await Send(HttpMethod.Post, "api/auth/signup/code", new { phone }, auth: false);

    public async Task<LoginResult> SignUpAsync(string phone, string code, string shopName, string? device)
    {
        var r = await Read<LoginResult>(await Send(HttpMethod.Post, "api/auth/signup", new { phone, code, shopName, deviceName = device }, auth: false));
        await tokens.SetAsync(r.Token);
        return r;
    }

    /// <summary>Çıkış bu cihazın oturumunu bitirir (K-074). Sunucuya ulaşılamasa da anahtar burada silinir.</summary>
    public async Task LogoutAsync()
    {
        try { (await Send(HttpMethod.Post, "api/auth/logout", null, auth: true)).Dispose(); }
        finally { await tokens.SetAsync(null); }
    }

    public async Task<PlateLookup> LookupPlateAsync(string plate)
        => await Read<PlateLookup>(await Send(HttpMethod.Get, "api/vehicles/lookup?plate=" + Uri.EscapeDataString(plate), null, auth: true));

    public async Task<OpenedRecord> OpenRecordAsync(string plate, NewVehicleInfo? vehicle)
        => await Read<OpenedRecord>(await Send(HttpMethod.Post, "api/records", new { plate, complaint = "", vehicle }, auth: true));

    public async Task<HomeSummary> HomeAsync() => await Read<HomeSummary>(await Send(HttpMethod.Get, "api/home", null, auth: true));

    public async Task<RecordList> RecordsAsync(string? q = null, bool inShop = false)
    {
        var url = "api/records?inShop=" + (inShop ? "true" : "false") + (string.IsNullOrWhiteSpace(q) ? "" : "&q=" + Uri.EscapeDataString(q));
        return await Read<RecordList>(await Send(HttpMethod.Get, url, null, auth: true));
    }

    private async Task<HttpResponseMessage> Send(HttpMethod m, string url, object? body, bool auth)
    {
        using var req = new HttpRequestMessage(m, url);
        if (body is not null) req.Content = JsonContent.Create(body, options: Json);
        if (auth && await tokens.GetAsync() is { Length: > 0 } t) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", t);
        HttpResponseMessage res;
        try { res = await http.SendAsync(req); }
        catch (HttpRequestException) { throw new ApiException(0, "network", "Sunucuya ulaşılamadı. İnternet bağlantını kontrol et."); }
        if (res.IsSuccessStatusCode) return res;

        // Oturum düştüyse (çıkış başka cihazdan yapıldı) anahtar silinir — ekran girişe döner.
        if (res.StatusCode == HttpStatusCode.Unauthorized && auth) await tokens.SetAsync(null);
        string? code = null, title = null;
        try
        {
            var p = await res.Content.ReadFromJsonAsync<JsonElement>(Json);
            if (p.TryGetProperty("code", out var c)) code = c.GetString();
            if (p.TryGetProperty("title", out var ti)) title = ti.GetString();
        }
        catch (JsonException) { }
        throw new ApiException(res.StatusCode, code, title ?? "Bir şeyler ters gitti. Tekrar dene.");
    }

    private static async Task<T> Read<T>(HttpResponseMessage res)
    {
        using (res) return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }
}

/// <summary>Ekranın biçimleri — prototipin <c>tutar</c> ve tarih yazımıyla aynı.</summary>
public static class Format
{
    /// <summary>
    /// Ayırıcılar AÇIKÇA yazılır, tr-TR kültür verisinden okunmaz: Blazor WASM tarayıcıya yalnız
    /// tarayıcı dilinin kültür verisini yükler; İngilizce tarayıcıda "3.450" → "3,450" oluyordu
    /// (tarayıcıda ölçülerek bulundu — sunucudaki testler tam veriyle koştuğu için göremezdi).
    /// </summary>
    private static readonly NumberFormatInfo Tr = new() { NumberGroupSeparator = ".", NumberDecimalSeparator = ",", NegativeSign = "-" };
    /// <summary>JS toLocaleString('tr-TR'): binlik nokta, gereksiz ondalık yok.</summary>
    public static string Money(decimal n) => n.ToString("#,0.###", Tr);
    public static string Date(DateOnly d) => d.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
