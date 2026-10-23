using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Suppliers;

// D1 · Parçacılar (VERI-MODELI §4). Prototipte liste sabitti (PARCACILAR); ekleme
// ekranı yoktu ama tamircinin sözü açık: "Bunları eklememiz gerekecek." (K-076.1)

public sealed record SupplierDto(Guid Id, string Name, string Phone);
public sealed record SupplierBody(string Name, string Phone);

public sealed record ListSuppliers : IRequest<List<SupplierDto>>;
public sealed record AddSupplier(SupplierBody Body) : IRequest<SupplierDto>;
public sealed record UpdateSupplier(Guid Id, SupplierBody Body) : IRequest<SupplierDto>;
public sealed record DeleteSupplier(Guid Id) : IRequest;

internal sealed class SupplierHandlers(ITamironDb db, ITenantContext tenant) :
    IRequestHandler<ListSuppliers, List<SupplierDto>>,
    IRequestHandler<AddSupplier, SupplierDto>,
    IRequestHandler<UpdateSupplier, SupplierDto>,
    IRequestHandler<DeleteSupplier>
{
    private static SupplierDto Dto(Supplier s) => new(s.Id, s.Name, s.Phone);

    public async Task<List<SupplierDto>> Handle(ListSuppliers q, CancellationToken ct)
        => (await db.Suppliers.AsNoTracking().ToListAsync(ct))
           .OrderBy(s => s.Name, StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), false))
           .Select(Dto).ToList();

    /// <summary>Aynı dükkânda iki parçacı aynı numarayı taşımaz: soru kime gitti karışır (K-076.1).</summary>
    private async Task EnsurePhoneFree(string phone, Guid? except, CancellationToken ct)
    {
        if (await db.Suppliers.AnyAsync(s => s.Phone == phone && s.Id != except, ct))
            throw new DomainException("supplier.phone_taken", $"{phone} başka bir parçacıda kayıtlı.");
    }

    public async Task<SupplierDto> Handle(AddSupplier c, CancellationToken ct)
    {
        var s = Supplier.Create(tenant.TenantId, c.Body.Name, c.Body.Phone);
        await EnsurePhoneFree(s.Phone, null, ct);
        db.Suppliers.Add(s);
        await db.SaveChangesAsync(ct);
        return Dto(s);
    }

    public async Task<SupplierDto> Handle(UpdateSupplier c, CancellationToken ct)
    {
        var s = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new NotFoundException("Parçacı");
        s.Update(c.Body.Name, c.Body.Phone);
        await EnsurePhoneFree(s.Phone, s.Id, ct);
        await db.SaveChangesAsync(ct);
        return Dto(s);
    }

    /// <summary>Silme = soft-delete: geçmiş fiyat soruları kime gittiğini kaybetmesin.</summary>
    public async Task Handle(DeleteSupplier c, CancellationToken ct)
    {
        var s = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new NotFoundException("Parçacı");
        db.Suppliers.Remove(s);
        await db.SaveChangesAsync(ct);
    }
}
