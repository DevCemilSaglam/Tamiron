using System.Text.Json;
using MediatR;
using Tamiron.Domain;

namespace Tamiron.Application.Traces;

// D9 · İz kaydı (K-052 · K-053). İstemci olaylarını toplar, gönderir; sunucu temizler ve
// saklar. Okuma ucu YOK: iz ustaya görünmez, bizim tarafa ait (A-M42 özeti ayrı iş).

public sealed record TraceItem(DateTime? At, string? Kind, JsonElement? Name, JsonElement? Data);
public sealed record SendTrace(IReadOnlyList<TraceItem>? Events) : IRequest<TraceResult>;
/// <summary>Kabul edilen ve düşen olay sayısı — istemci tanımadığı bir türü yolladıysa görür.</summary>
public sealed record TraceResult(int Accepted, int Dropped);

/// <summary>90 günden eski ayrıntıyı siler (K-053.4). Zamanlayıcı yok: işletim betiği çağırır.</summary>
public sealed record PurgeTrace : IRequest<int>;

internal sealed class TraceHandlers(ITamironDb db, ICurrentSession session, IClock clock) :
    IRequestHandler<SendTrace, TraceResult>,
    IRequestHandler<PurgeTrace, int>
{
    public async Task<TraceResult> Handle(SendTrace c, CancellationToken ct)
    {
        var items = c.Events ?? [];
        if (items.Count > Trace.MaxBatch)
            throw new DomainException("trace.too_many", $"Bir seferde en çok {Trace.MaxBatch} olay.");
        var ok = items.Where(i => Trace.IsKind(i.Kind)).ToList();
        db.TraceEvents.AddRange(ok.Select(i => TraceEvent.Create(session.UserId, session.SessionId,
            i.At?.ToUniversalTime(), i.Kind!, i.Name, i.Data)));
        await db.SaveChangesAsync(ct);
        return new(ok.Count, items.Count - ok.Count);
    }

    /// <summary>Bütün dükkânlarda — iz bizim tarafın; silme gerçek (ayrıntının süresi doldu, soft-delete değil).</summary>
    public Task<int> Handle(PurgeTrace c, CancellationToken ct)
        => db.PurgeTraceBefore(Trace.PurgeBefore(clock.UtcNow), ct);
}
