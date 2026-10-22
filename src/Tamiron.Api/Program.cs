using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Api;
using Tamiron.Application;
using Tamiron.Application.ServiceRecords;
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

// Veresiyenin parası geldi (K-071.1). Gün SUNUCUDAN: istemci tarih göndermez.
api.MapPost("/records/{id:guid}/collect", (Guid id, CollectBody body, ISender s, CancellationToken ct) =>
    s.Send(new CollectPayment(id, body.Method), ct));

api.MapPost("/records/{id:guid}/collect/undo", (Guid id, ISender s, CancellationToken ct) =>
    s.Send(new UndoCollection(id), ct));

api.MapGet("/cash", (PeriodKind period, ISender s, CancellationToken ct) =>
    s.Send(new GetCashSummary(period), ct));

app.Run();

public sealed record CollectBody(PaymentMethod Method);

public partial class Program;   // WebApplicationFactory için
