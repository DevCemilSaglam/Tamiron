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

/// <summary>G3 · Servis kaydı (API ServiceRecordDto'nun ekranın okuduğu kısmı).</summary>
public sealed record RecordDetail(Guid Id, Guid VehicleId, string PlateNo, string Type, string Complaint,
                                  DateOnly? DeliveredAt, bool WaitingForPart, bool ShowWaitingToCustomer,
                                  decimal Total, IReadOnlyList<LineItem> Lines, IReadOnlyList<string> Warnings,
                                  decimal Paid, decimal Remaining, string? PaymentMethod = null, DateOnly? ReminderDate = null,
                                  IReadOnlyList<PaymentItem>? Payments = null);

/// <summary>Alınan ödeme (K-078). AtDelivery: teslimde alındı — teslimle gider, "geri al"la değil.</summary>
public sealed record PaymentItem(Guid Id, decimal Amount, string Method, DateOnly PaidOn, bool AtDelivery);

public sealed record LineItem(Guid Id, string Kind, string Name, decimal Price, decimal? Cost, string? SuppliedBy, bool CountsInTotal);

/// <summary>API'ye giden satır (LineBody). Kind: "Part" | "Labor"; SuppliedBy: null | "Shop" | "Customer".</summary>
public sealed record LineInput(string Kind, string Name, decimal Price, decimal? Cost, string? SuppliedBy);

public sealed record VehicleCard(Guid Id, string PlateNo, string? Brand, string? Model, string? OwnerName, string? OwnerPhone,
                                 string? ContactName, string? ContactPhone, string? Year = null, string? Engine = null);

/// <summary>G4 · Teslimden önce: kurulacak bakım hatırlatması (API DeliveryPreview). Basis: "Month" | "Km".</summary>
public sealed record DeliveryPreview(Guid RecordId, ReminderPlan? Reminder);
public sealed record ReminderPlan(DateOnly Date, string Basis, DateOnly MonthDate, int? KmTarget, DateOnly? KmDate);

public sealed record ShopSettings(string Name, int MaintenanceMonths, int MaintenanceKm, bool AskPartCost);

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

    public async Task<RecordDetail> RecordAsync(Guid id) => await Read<RecordDetail>(await Send(HttpMethod.Get, $"api/records/{id}", null, auth: true));
    public async Task<VehicleCard> VehicleAsync(Guid id) => await Read<VehicleCard>(await Send(HttpMethod.Get, $"api/vehicles/{id}", null, auth: true));
    public async Task<RecordDetail> AddLineAsync(Guid id, LineInput l) => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/lines", l, auth: true));
    public async Task<RecordDetail> UpdateLineAsync(Guid id, Guid lineId, LineInput l) => await Read<RecordDetail>(await Send(HttpMethod.Put, $"api/records/{id}/lines/{lineId}", l, auth: true));
    public async Task<RecordDetail> DeleteLineAsync(Guid id, Guid lineId) => await Read<RecordDetail>(await Send(HttpMethod.Delete, $"api/records/{id}/lines/{lineId}", null, auth: true));
    public async Task<RecordDetail> RestoreLineAsync(Guid id, Guid lineId) => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/lines/{lineId}/restore", null, auth: true));

    /// <summary>Kısmi güncelleme (API RecordPatch): null = dokunma.</summary>
    public async Task<RecordDetail> PatchRecordAsync(Guid id, string? complaint = null, string? type = null, bool? waitingForPart = null, bool? showWaitingToCustomer = null)
        => await Read<RecordDetail>(await Send(HttpMethod.Patch, $"api/records/{id}", new { complaint, type, waitingForPart, showWaitingToCustomer }, auth: true));

    public async Task<DeliveryPreview> DeliveryPreviewAsync(Guid id) => await Read<DeliveryPreview>(await Send(HttpMethod.Get, $"api/records/{id}/delivery-preview", null, auth: true));
    public async Task<ShopSettings> ShopAsync() => await Read<ShopSettings>(await Send(HttpMethod.Get, "api/shop", null, auth: true));

    /// <summary>Teslim (K-004). <paramref name="amount"/> dolu = "Bir kısmını aldım" (K-078); kural API'de.</summary>
    public async Task<RecordDetail> DeliverAsync(Guid id, string method, decimal? amount)
        => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/deliver", new { method, amount }, auth: true));

    /// <summary>Sonradan tahsilat (K-078): tutar + yol; fazlası API'de reddedilir.</summary>
    public async Task<RecordDetail> CollectAsync(Guid id, string method, decimal amount)
        => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/collect", new { method, amount }, auth: true));
    public async Task<RecordDetail> UndoCollectAsync(Guid id) => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/collect/undo", null, auth: true));
    /// <summary>E1 · Ödeme yolunu düzelt (K-077.2): son ödemenin yolu; gün korunur.</summary>
    public async Task<RecordDetail> CorrectPaymentAsync(Guid id, string method)
        => await Read<RecordDetail>(await Send(HttpMethod.Post, $"api/records/{id}/payment", new { method }, auth: true));

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
    /// <summary>Ustanın yazdığı tutar: "1.200" · "1200" · "1.234,5" · "" (boş = 0). Okunamazsa null.</summary>
    public static decimal? ParseMoney(string? s)
    {
        var t = (s ?? "").Replace("₺", "").Replace(" ", "").Replace(".", "").Replace(',', '.');
        if (t.Length == 0) return 0;
        return decimal.TryParse(t, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d) && d >= 0 ? d : null;
    }
    /// <summary>Ödeme yolunun Türkçesi (prototip ODEME_ETIKET).</summary>
    public static string PaymentLabel(string? method) => method switch
    {
        "Cash" => "Nakit", "Card" => "Kart", "Transfer" => "Havale", "Unpaid" => "Almadım", _ => "",
    };

    public static string Date(DateOnly d) => d.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
