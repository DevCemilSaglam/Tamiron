using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Api;
using Tamiron.Application;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Vehicles;
using Tamiron.Domain;
using Tamiron.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Tamiron")
    ?? throw new InvalidOperationException("ConnectionStrings:Tamiron yok."));
builder.Services.AddScoped<HeaderTenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<HeaderTenantContext>());
builder.Services.AddProblemDetails();
// Enum'lar ad olarak gidip gelir ("Card", 3 değil): istemci kodu okunur kalsın.
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.UseMiddleware<ErrorMiddleware>();
app.UseMiddleware<TenantMiddleware>();

if (app.Environment.IsDevelopment())
    await DevSeed.RunAsync(app.Services);

var api = app.MapGroup("/api");

api.MapGet("/records/{id:guid}", (Guid id, ISender s, CancellationToken ct) =>
    s.Send(new GetServiceRecord(id), ct));

api.MapPost("/records", async (OpenServiceRecord cmd, ISender s, CancellationToken ct) =>
{
    var r = await s.Send(cmd, ct);
    return Results.Created($"/api/records/{r.Id}", r);
});

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
