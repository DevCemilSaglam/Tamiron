namespace Tamiron.Domain;

/// <summary>
/// Liste araması — prototipin <c>norm(...).indexOf(q)</c>'su: Türkçe küçük harf +
/// ı/ü/ö/ş/ç/ğ katlama, parçalar boşlukla birleşir, alt dize aranır.
/// "ayse" "Ayşe Kaya"yı bulur; gösterilen bir şeyin aranamaması ustaya "yok" dedirtir.
/// </summary>
public static class TextSearch
{
    public static bool Matches(string? q, params string?[] parts)
    {
        var n = ServiceTypeGuess.Fold(q).Trim();
        if (n.Length == 0) return true;
        return ServiceTypeGuess.Fold(string.Join(' ', parts.Where(p => p is not null))).Contains(n, StringComparison.Ordinal);
    }
}
