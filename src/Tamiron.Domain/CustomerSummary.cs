using System.Security.Cryptography;

namespace Tamiron.Domain;

/// <summary>
/// Müşterinin gördüğü özet (K-023) — prototip ozetGovdeHtml. Salt okunur; alış fiyatı
/// YOK (VERI-MODELI §3). Adres tahmin edilemez bir koddur (K-076.3; A-M9'u kapatır).
/// </summary>
public static class CustomerSummary
{
    /// <summary>Okunurken karışan 0/o, 1/l/i yok; küçük harf — WhatsApp'ta elle yazılsa da tutar.</summary>
    private const string Alphabet = "23456789abcdefghjkmnpqrstuvwxyz";

    /// <summary>16 karakter × 31 harf ≈ 79 bit: denenerek bulunamaz.</summary>
    public static string NewCode() => RandomNumberGenerator.GetString(Alphabet, 16);

    public static string Link(string code) => "tamiron.app/o/" + code;

    /// <summary>
    /// Satırın müşteriye görünen tutarı. Müşteri parçasında RAKAM YOK (K-037): ödemediği
    /// parçanın yanında rakam ücret isteniyormuş gibi okunur. Fiyatı gelmemiş satır "—".
    /// </summary>
    public static decimal? Amount(ServiceLine l)
        => !l.CountsInTotal || l.Price == 0 ? null : l.Price;

    /// <summary>
    /// Tek durum (K-004): müşterinin ikinci sorusu "aracım hazır mı". Parça beklendiği
    /// YALNIZ usta öyle seçtiyse söylenir (A-M15).
    /// </summary>
    public static string StatusText(ServiceRecord r)
    {
        if (r.DeliveredAt is { } d)
        {
            var s = $"Aracınız {d:dd.MM.yyyy} tarihinde teslim edildi.";
            return r.PaymentMethod switch
            {
                PaymentMethod.Unpaid => s + " Ödeme alınmadı.",
                null => s,
                var m => s + " Ödeme: " + Csv.PaymentLabel(m) + ".",
            };
        }
        return r.WaitingForPart && r.ShowWaitingToCustomer
            ? "Aracınız serviste, parça bekleniyor. Parça gelir gelmez işe başlayıp size haber vereceğiz."
            : "Aracınız serviste. Hazır olduğunda haber vereceğiz.";
    }
}
