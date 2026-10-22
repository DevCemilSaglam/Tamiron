namespace Tamiron.Domain;

/// <summary>
/// İş başına fark — prototipin <c>isKari</c>'si (96-is-basina, K-055.2, K-057).
/// "Kâr" değil FARK: giderler burada yok (K-014: eksik giderle çıkan kâr, kârdan kötü).
///  · Yalnız TAMİRCİNİN aldığı parça sayılır — müşteri parçası zaten toplama girmiyor.
///  · Alış bilinmiyorsa (null) parça SATIŞIYLA BİRLİKTE hesap dışı kalır ve sayılır;
///    yalnız alışı sıfır saymak satışı farka katar ve şişme sürerdi. 0 = bedava, bilinir.
///  · Fiyatsız satır farka hiçbir şey katmaz.
/// </summary>
public sealed record JobProfit(decimal Received, decimal PartsCost, decimal Outside, int UnknownCount)
{
    public decimal Difference => Received - Outside - PartsCost;

    public static JobProfit Of(ServiceRecord r)
    {
        decimal parts = 0, outside = 0;
        var unknown = 0;
        foreach (var l in r.Lines.Where(l => l.Kind == LineKind.Part && l.SuppliedBy == PartSupplier.Shop))
        {
            if (l.Cost is { } c) { parts += c; continue; }
            if (l.Price > 0) { unknown++; outside += l.Price; }
        }
        return new JobProfit(r.Total, parts, outside, unknown);
    }
}
