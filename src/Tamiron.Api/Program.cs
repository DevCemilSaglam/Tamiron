using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Api;
using Tamiron.Application;
using Tamiron.Application.Accounting;
using Tamiron.Application.Auth;
using Tamiron.Application.Customers;
using Tamiron.Application.Expenses;
using Tamiron.Application.Lists;
using Tamiron.Application.Messages;
using Tamiron.Application.Reminders;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Vehicles;
using Tamiron.Domain;
using Tamiron.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Tamiron")
    ?? throw new InvalidOperationException("ConnectionStrings:Tamiron yok."));
builder.Services.AddScoped<RequestContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<RequestContext>());
builder.Services.AddScoped<ICurrentSession>(sp => sp.GetRequiredService<RequestContext>());
// SMS: geliştirmede günlüğe yazılır; başka her ortamda sağlayıcı bağlanana kadar
// gönderim YAPILMAZ ve 503 döner (K-074.5 — sağlayıcı Cemil'in kararı, paralı).
if (builder.Environment.IsDevelopment()) builder.Services.AddSingleton<ISmsSender, DevLogSmsSender>();
else builder.Services.AddSingleton<ISmsSender, NotConfiguredSmsSender>();
builder.Services.AddProblemDetails();
// Okunamayan istek SESSİZ 400 yazmasın, istisna atsın: ErrorMiddleware onu Türkçe
// Problem Details'e çevirir (C7). Varsayılan, geliştirme dışında gövdesiz 400'dü.
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
// Enum'lar ad olarak gidip gelir ("Card", 3 değil): istemci kodu okunur kalsın.
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.UseMiddleware<ErrorMiddleware>();
app.UseMiddleware<SessionMiddleware>();

if (app.Environment.IsDevelopment())
    await DevSeed.RunAsync(app.Services);

var api = app.MapGroup("/api");

// Giriş (K-074): kod iste → kodu gir → anahtar. Oturum yalnız çıkışla biter.
api.MapPost("/auth/code", async (RequestLoginCode cmd, ISender s, CancellationToken ct) =>
{
    await s.Send(cmd, ct);
    return Results.Accepted();
});
api.MapPost("/auth/verify", (VerifyLoginCode cmd, ISender s, CancellationToken ct) => s.Send(cmd, ct));
api.MapPost("/auth/logout", async (ISender s, CancellationToken ct) => { await s.Send(new Logout(), ct); return Results.NoContent(); });
api.MapPost("/auth/logout-all", async (ISender s, CancellationToken ct) => { await s.Send(new LogoutAll(), ct); return Results.NoContent(); });
api.MapGet("/auth/sessions", (ISender s, CancellationToken ct) => s.Send(new ListSessions(), ct));

api.MapGet("/records/{id:guid}", (Guid id, ISender s, CancellationToken ct) =>
    s.Send(new GetServiceRecord(id), ct));

api.MapPost("/records", async (OpenServiceRecord cmd, ISender s, CancellationToken ct) =>
{
    var r = await s.Send(cmd, ct);
    return Results.Created($"/api/records/{r.Id}", r);
});

// Hatırlatması gelenler (B12): en çok bekleyen üstte.
api.MapGet("/reminders/due", (ISender s, CancellationToken ct) => s.Send(new ListDueReminders(), ct));

// Mesajlar (B11): GÖNDERİM YOK, kayıt. Toplu: yalnız izinli müşteriler.
api.MapPost("/messages", (RecordMessage cmd, ISender s, CancellationToken ct) => s.Send(cmd, ct));
api.MapGet("/messages", (Guid? vehicleId, ISender s, CancellationToken ct) => s.Send(new ListMessages(vehicleId), ct));
api.MapGet("/messages/bulk-preview", (ISender s, CancellationToken ct) => s.Send(new GetBulkPreview(), ct));
api.MapPost("/messages/bulk", (SendBulk cmd, ISender s, CancellationToken ct) => s.Send(cmd, ct));

// Müşteriler (B10): araçtan türetilir; anahtar katlanmış ad + "|" + telefon.
api.MapGet("/customers", (string? q, ISender s, CancellationToken ct) => s.Send(new ListCustomers(q), ct));
api.MapGet("/customers/detail", (string key, ISender s, CancellationToken ct) => s.Send(new GetCustomer(key), ct));
api.MapPut("/customers/consent", (SetConsent cmd, ISender s, CancellationToken ct) => s.Send(cmd, ct));

// Parçacılar (D1): ad + telefon; aynı numara iki kez olmaz; silme soft.
// D2 · Parça sorma: soru (parçacı başına bir kayıt) · liste · cevap (satıra işlenir).
api.MapPost("/records/{id:guid}/price-requests", (Guid id, Tamiron.Application.PriceRequests.AskBody b, ISender s, CancellationToken ct) =>
    s.Send(new Tamiron.Application.PriceRequests.AskForPrices(id, b), ct));
api.MapGet("/records/{id:guid}/price-requests", (Guid id, ISender s, CancellationToken ct) =>
    s.Send(new Tamiron.Application.PriceRequests.ListPriceRequests(id), ct));
api.MapPost("/price-requests/{id:guid}/replies", (Guid id, Tamiron.Application.PriceRequests.ReplyBody b, ISender s, CancellationToken ct) =>
    s.Send(new Tamiron.Application.PriceRequests.AddPriceReply(id, b), ct));

api.MapGet("/suppliers", (ISender s, CancellationToken ct) => s.Send(new Tamiron.Application.Suppliers.ListSuppliers(), ct));
api.MapPost("/suppliers", (Tamiron.Application.Suppliers.SupplierBody b, ISender s, CancellationToken ct) =>
    s.Send(new Tamiron.Application.Suppliers.AddSupplier(b), ct));
api.MapPut("/suppliers/{id:guid}", (Guid id, Tamiron.Application.Suppliers.SupplierBody b, ISender s, CancellationToken ct) =>
    s.Send(new Tamiron.Application.Suppliers.UpdateSupplier(id, b), ct));
api.MapDelete("/suppliers/{id:guid}", async (Guid id, ISender s, CancellationToken ct) =>
{
    await s.Send(new Tamiron.Application.Suppliers.DeleteSupplier(id), ct);
    return Results.NoContent();
});

// Dükkân ayarları (C5): bakım periyodu (3·6·12 ay) ve alış sorusu; gizlilik notu bir kez.
api.MapGet("/shop", (ISender s, CancellationToken ct) => s.Send(new Tamiron.Application.Shops.GetShopSettings(), ct));
api.MapPatch("/shop", (Tamiron.Application.Shops.PatchShopSettings cmd, ISender s, CancellationToken ct) => s.Send(cmd, ct));
api.MapPost("/shop/privacy-notice-shown", (ISender s, CancellationToken ct) => s.Send(new Tamiron.Application.Shops.MarkPrivacyNoticeShown(), ct));

// Ana ekran özeti (C3): menü alt yazıları + hatırlatma rozeti tek istekte.
api.MapGet("/home", (ISender s, CancellationToken ct) => s.Send(new Tamiron.Application.Home.GetHomeSummary(), ct));

// Muhasebe ekranı tek istekte (C8): kasa + gider + alacak; her parça kendi ucunun aynısı.
api.MapGet("/accounting", (PeriodKind? period, ISender s, CancellationToken ct) =>
    s.Send(new GetAccountingOverview(period ?? PeriodKind.Today), ct));

// İş başına fark (C1): dönem teslim gününe göre; bilinmeyen alış hesap dışı ve sayılır.
api.MapGet("/profit", (PeriodKind? period, ISender s, CancellationToken ct) =>
    s.Send(new ListJobProfits(period ?? PeriodKind.Month), ct));

// Parça alışları (C2): yalnız teslim edilmiş işler, tamircinin aldığı, alışı yazılı.
api.MapGet("/part-purchases", (PeriodKind? period, ISender s, CancellationToken ct) =>
    s.Send(new ListPartPurchases(period ?? PeriodKind.Month), ct));

// Alacak (B9): döneme tabi değil. CSV: BOM'lu UTF-8, indirilen dosya.
// ?vehicleId= → araç ekranının "Ödeme durumu" (C4): aynı liste, araca süzülmüş.
api.MapGet("/receivables", (Guid? vehicleId, ISender s, CancellationToken ct) => s.Send(new ListReceivables(vehicleId), ct));
api.MapGet("/export/cash.csv", async (PeriodKind? period, ISender s, CancellationToken ct) =>
{
    var f = await s.Send(new ExportCashCsv(period ?? PeriodKind.Month), ct);
    // BOM içerikte (Csv.Document); encoder ikinci bir BOM eklemesin diye ham baytlar.
    return Results.File(new System.Text.UTF8Encoding(false).GetBytes(f.Content), "text/csv; charset=utf-8", f.FileName);
});

// Gider (B8): dönem listesi + toplam; sil/geri al dönemin güncel listesini döner.
api.MapGet("/expenses", (PeriodKind? period, ISender s, CancellationToken ct) =>
    s.Send(new ListExpenses(period ?? PeriodKind.Month), ct));
api.MapPost("/expenses", (AddExpense cmd, ISender s, CancellationToken ct) => s.Send(cmd, ct));
api.MapDelete("/expenses/{id:guid}", (Guid id, PeriodKind? period, ISender s, CancellationToken ct) =>
    s.Send(new DeleteExpense(id, period ?? PeriodKind.Month), ct));
api.MapPost("/expenses/{id:guid}/restore", (Guid id, PeriodKind? period, ISender s, CancellationToken ct) =>
    s.Send(new RestoreExpense(id, period ?? PeriodKind.Month), ct));

// Listeler (B7): serviste olanlar üstte; arama Türkçe katlamalı.
api.MapGet("/records", (string? q, bool? inShop, ISender s, CancellationToken ct) =>
    s.Send(new ListRecords(q, inShop ?? false), ct));
api.MapGet("/vehicles/list", (string? q, ISender s, CancellationToken ct) => s.Send(new ListVehicles(q), ct));

// Araç (B6). PATCH: null = dokunma, "" = temizle.
api.MapGet("/vehicles/{id:guid}", (Guid id, ISender s, CancellationToken ct) => s.Send(new GetVehicle(id), ct));
api.MapGet("/vehicles", (string plate, ISender s, CancellationToken ct) => s.Send(new FindVehicleByPlate(plate), ct));
api.MapPatch("/vehicles/{id:guid}", (Guid id, PatchVehicle body, ISender s, CancellationToken ct) =>
    s.Send(body with { Id = id }, ct));

// Kaydın bayrakları (B4): verilmeyen alana dokunulmaz.
api.MapPatch("/records/{id:guid}", (Guid id, RecordPatch p, ISender s, CancellationToken ct) =>
    s.Send(new UpdateRecord(id, p.Complaint, p.Type, p.Km, p.WaitingForPart, p.ShowWaitingToCustomer), ct));

// Teslim (B5). Önizleme kaydetmez: teslim ekranı "hatırlatma şu güne kurulacak" der.
api.MapGet("/records/{id:guid}/delivery-preview", (Guid id, ISender s, CancellationToken ct) =>
    s.Send(new GetDeliveryPreview(id), ct));
api.MapPost("/records/{id:guid}/deliver", (Guid id, CollectBody body, ISender s, CancellationToken ct) =>
    s.Send(new DeliverRecord(id, body.Method), ct));

// Satırlar (B3). Her uç kaydın güncel hâlini döner.
api.MapPost("/records/{id:guid}/lines", (Guid id, LineBody body, ISender s, CancellationToken ct) =>
    s.Send(new AddLine(id, body), ct));
api.MapPut("/records/{id:guid}/lines/{lineId:guid}", (Guid id, Guid lineId, LineBody body, ISender s, CancellationToken ct) =>
    s.Send(new UpdateLine(id, lineId, body), ct));
api.MapDelete("/records/{id:guid}/lines/{lineId:guid}", (Guid id, Guid lineId, ISender s, CancellationToken ct) =>
    s.Send(new DeleteLine(id, lineId), ct));
api.MapPost("/records/{id:guid}/lines/{lineId:guid}/restore", (Guid id, Guid lineId, ISender s, CancellationToken ct) =>
    s.Send(new RestoreLine(id, lineId), ct));

// Veresiyenin parası geldi (K-071.1). Gün SUNUCUDAN: istemci tarih göndermez.
api.MapPost("/records/{id:guid}/collect", (Guid id, CollectBody body, ISender s, CancellationToken ct) =>
    s.Send(new CollectPayment(id, body.Method), ct));

api.MapPost("/records/{id:guid}/collect/undo", (Guid id, ISender s, CancellationToken ct) =>
    s.Send(new UndoCollection(id), ct));

api.MapGet("/cash", (PeriodKind period, ISender s, CancellationToken ct) =>
    s.Send(new GetCashSummary(period), ct));

app.Run();

public sealed record CollectBody(PaymentMethod Method);
public sealed record RecordPatch(string? Complaint, ServiceType? Type, string? Km, bool? WaitingForPart, bool? ShowWaitingToCustomer);

public partial class Program;   // WebApplicationFactory için
