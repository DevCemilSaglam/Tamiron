using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Application.PriceRequests;

// D2 · Parça sorma (prototip 40-parca-sor, VERI-MODELI §5–6). Mesajı API GÖNDERMEZ:
// WhatsApp'ı ustanın telefonu açar (T-12b); API soruyu, metni ve kime gittiğini saklar.

public sealed record AskedLineDto(Guid Id, string Name, Guid? SelectedReplyId);
public sealed record PriceReplyDto(Guid Id, string RawText, decimal? ParsedPrice, string? ParsedBrand, string? ParsedCode, DateTime ReceivedAt);
public sealed record PriceRequestDto(Guid Id, Guid SupplierId, string SupplierName, string SupplierPhone, string SentText, DateTime SentAt,
                                     IReadOnlyList<AskedLineDto> Lines, IReadOnlyList<PriceReplyDto> Replies);

public sealed record AskBody(IReadOnlyList<Guid>? SupplierIds);
public sealed record ReplyBody(Guid LineId, string? RawText, decimal? Price, string? Brand, string? Code, decimal? CustomerPrice, bool OnCredit = false);

/// <summary>
/// G10 · Gönderilmeden ÖNCE: neyin sorulacağı, araç kimliği tamam mı (K-068; eksikse hangileri) ve
/// parçacıya gidecek metnin KENDİSİ (gönderilenle aynı fonksiyon — gördüğün gider).
/// </summary>
public sealed record GetPriceQuestion(Guid RecordId) : IRequest<PriceQuestionPreview>;
public sealed record PriceQuestionPreview(IReadOnlyList<AskedLineDto> Lines, bool HasPartIdentity, IReadOnlyList<string> Missing, string Text);

public sealed record AskForPrices(Guid RecordId, AskBody Body) : IRequest<List<PriceRequestDto>>;
public sealed record ListPriceRequests(Guid RecordId) : IRequest<List<PriceRequestDto>>;
public sealed record AddPriceReply(Guid RequestId, ReplyBody Body) : IRequest<ServiceRecordDto>;

internal sealed class PriceRequestHandlers(ITamironDb db, ITenantContext tenant, IClock clock) :
    IRequestHandler<AskForPrices, List<PriceRequestDto>>,
    IRequestHandler<ListPriceRequests, List<PriceRequestDto>>,
    IRequestHandler<AddPriceReply, ServiceRecordDto>,
    IRequestHandler<GetPriceQuestion, PriceQuestionPreview>
{
    public async Task<PriceQuestionPreview> Handle(GetPriceQuestion q, CancellationToken ct)
    {
        var r = await db.ServiceRecords.AsNoTracking().Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == q.RecordId, ct)
                ?? throw new NotFoundException("Servis kaydı");
        var v = await db.Vehicles.AsNoTracking().FirstAsync(x => x.Id == r.VehicleId, ct);
        var askable = PartQuestion.AskableLines(r.Lines.OrderBy(l => l.Position).ThenBy(l => l.DateTimeCreated));
        return new PriceQuestionPreview(askable.Select(l => new AskedLineDto(l.Id, l.Name, l.SelectedReplyId)).ToList(),
            v.HasPartIdentity, v.MissingForParts, PartQuestion.Compose(v, askable));
    }

    /// <summary>
    /// Seçilen her parçacıya AYRI soru (Kol B). Kapılar domain'de (K-068); başka dükkânın
    /// parçacısı 404 — süzgeç onu görmez.
    /// </summary>
    public async Task<List<PriceRequestDto>> Handle(AskForPrices c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == c.RecordId, ct)
                ?? throw new NotFoundException("Servis kaydı");
        var v = await db.Vehicles.AsNoTracking().FirstAsync(x => x.Id == r.VehicleId, ct);
        var ids = (c.Body.SupplierIds ?? []).Distinct().ToList();
        var askable = PartQuestion.AskableLines(r.Lines.OrderBy(l => l.Position).ThenBy(l => l.DateTimeCreated));
        PartQuestion.EnsureCanAsk(v, askable, ids.Count);

        var suppliers = await db.Suppliers.AsNoTracking().Where(s => ids.Contains(s.Id)).ToListAsync(ct);
        if (suppliers.Count != ids.Count) throw new NotFoundException("Parçacı");

        var text = PartQuestion.Compose(v, askable);
        var at = clock.UtcNow;
        // Seçim sırası korunur: düğme "2 parçacıya gönder" dediğinde WhatsApp sırayla açılır.
        var made = ids.Select(id => PriceRequest.Create(tenant.TenantId, r.Id, id, text, askable, at)).ToList();
        db.PriceRequests.AddRange(made);
        await db.SaveChangesAsync(ct);
        return await Load(r.Id, ct, made.Select(m => m.Id).ToList());
    }

    public Task<List<PriceRequestDto>> Handle(ListPriceRequests q, CancellationToken ct) => Load(q.RecordId, ct, null);

    /// <summary>
    /// Cevap: ham metin olduğu gibi saklanır, satıra işlenir ve "kimden geldi" o cevap olur.
    /// Satır bu soruda sorulmamışsa yazılmaz — yanlış satıra fiyat düşmesin.
    /// </summary>
    public async Task<ServiceRecordDto> Handle(AddPriceReply c, CancellationToken ct)
    {
        var req = await db.PriceRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == c.RequestId, ct)
                  ?? throw new NotFoundException("Fiyat sorusu");
        if (!req.Lines.Any(l => l.ServiceLineId == c.Body.LineId))
            throw new DomainException("reply.line_not_asked", "Bu parça o parçacıya sorulmamış.");
        var line = await db.ServiceLines.FirstOrDefaultAsync(x => x.Id == c.Body.LineId, ct)
                   ?? throw new NotFoundException("Satır");

        var reply = PriceReply.Create(req.Id, c.Body.RawText, c.Body.Price, c.Body.Brand, c.Body.Code, clock.UtcNow);
        // H3: parça bu parçacıdan alındı; "veresiye" denirse borç doğar (K-090).
        line.ApplyReply(reply, c.Body.CustomerPrice, req.SupplierId, c.Body.OnCredit);
        db.PriceReplies.Add(reply);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, req.ServiceRecordId, ct);
    }

    private async Task<List<PriceRequestDto>> Load(Guid recordId, CancellationToken ct, List<Guid>? only)
    {
        if (!await db.ServiceRecords.AnyAsync(x => x.Id == recordId, ct)) throw new NotFoundException("Servis kaydı");
        var reqs = await db.PriceRequests.AsNoTracking()
            .Where(x => x.ServiceRecordId == recordId && (only == null || only.Contains(x.Id)))
            .OrderBy(x => x.SentAt).ToListAsync(ct);
        var reqIds = reqs.Select(x => x.Id).ToList();
        var replies = await db.PriceReplies.AsNoTracking().Where(x => reqIds.Contains(x.PriceRequestId)).OrderBy(x => x.ReceivedAt).ToListAsync(ct);
        var supIds = reqs.Select(x => x.SupplierId).Distinct().ToList();
        // Silinmiş parçacı da adıyla görünsün: geçmiş soru kime gittiğini kaybetmez (K-076.1).
        var sups = await db.Suppliers.IgnoreQueryFilters().AsNoTracking()
            .Where(s => supIds.Contains(s.Id) && s.TenantId == tenant.TenantId).ToDictionaryAsync(s => s.Id, ct);
        var lines = await db.ServiceLines.AsNoTracking().Where(l => l.ServiceRecordId == recordId).ToDictionaryAsync(l => l.Id, ct);

        // Aynı anda gönderilen sorular seçim sırasıyla döner (SentAt eşit; Create sırası = liste sırası).
        if (only is not null) reqs = only.Select(id => reqs.First(x => x.Id == id)).ToList();
        return reqs.Select(x => new PriceRequestDto(
            x.Id, x.SupplierId, sups[x.SupplierId].Name, sups[x.SupplierId].Phone, x.SentText, x.SentAt,
            x.Lines.Where(l => lines.ContainsKey(l.ServiceLineId))
                   .Select(l => lines[l.ServiceLineId])
                   .OrderBy(l => l.Position).ThenBy(l => l.DateTimeCreated)
                   .Select(l => new AskedLineDto(l.Id, l.Name, l.SelectedReplyId)).ToList(),
            replies.Where(p => p.PriceRequestId == x.Id)
                   .Select(p => new PriceReplyDto(p.Id, p.RawText, p.ParsedPrice, p.ParsedBrand, p.ParsedCode, p.ReceivedAt)).ToList()
        )).ToList();
    }
}
