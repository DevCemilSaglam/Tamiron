using MediatR;
using Tamiron.Application.Expenses;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Application.Accounting;

/// <summary>
/// C8 · Muhasebe ekranı (95-muhasebe "Basit") tek istekte: kasa özeti + dönemin giderleri +
/// alacak. Çevrimdışı çalışan istemci (PWA) için: üç istek yerine bir; biri düşüp ötekiler
/// gelirse ekran yarım rakam göstermez.
/// Yeni hesap YOK — her parça KENDİ sorgusundan gelir; ayrı uçlarla birebir aynıdır (testli).
/// </summary>
public sealed record AccountingOverview(CashSummary Cash, ExpenseList Expenses, ReceivableList Receivables);

public sealed record GetAccountingOverview(PeriodKind Period) : IRequest<AccountingOverview>;

internal sealed class AccountingOverviewHandler(ISender sender) : IRequestHandler<GetAccountingOverview, AccountingOverview>
{
    public async Task<AccountingOverview> Handle(GetAccountingOverview q, CancellationToken ct)
        => new(await sender.Send(new GetCashSummary(q.Period), ct),
               await sender.Send(new ListExpenses(q.Period), ct),
               // Alacak bir bakiye: döneme tabi DEĞİL (05.09).
               await sender.Send(new ListReceivables(), ct));
}
