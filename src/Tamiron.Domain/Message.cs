namespace Tamiron.Domain;

public enum MessageChannel { WhatsApp = 1, Sms = 2, Call = 3 }
public enum MessageTargetKind { Contact = 1, Owner = 2 }

/// <summary>Mesajın gideceği kişi — prototip mesajHedefleri'nin bir satırı.</summary>
public sealed record MessageTarget(MessageTargetKind Kind, string Name, string? Phone)
{
    public bool IsPerson => !MessageText.IsFirm(Name);
}

/// <summary>
/// Gönderilen mesaj (VERI-MODELI §10, K-048.2). Gönderildiği AN kime gittiği
/// yazılır — araçtaki telefon sonradan değişebilir. Toplu da olsa kişi başına
/// bir kayıt: ispat kişi başına (K-045.3). Saklama üç yıl.
/// Gönderim (WhatsApp/SMS) bu fazda YOK — kayıt var.
/// </summary>
public class Message : TenantEntity
{
    public Guid VehicleId { get; private set; }
    public Guid? ServiceRecordId { get; private set; }
    public string RecipientName { get; private set; } = "";
    public string RecipientPhone { get; private set; } = "";
    public MessageChannel Channel { get; private set; }
    public string Template { get; private set; } = "serbest";
    public bool IsBulk { get; private set; }
    public string Body { get; private set; } = "";

    private Message() { }   // EF

    public static Message Record(Guid tenantId, Vehicle vehicle, MessageTarget target, MessageChannel channel,
                                 string? template, string body, bool isBulk, Guid? serviceRecordId = null)
    {
        if (string.IsNullOrWhiteSpace(target.Phone))
            throw new DomainException("message.no_phone", "Telefon yok — mesaj gönderilemez. Araç kaydına telefon ekle.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("message.empty", "Gidecek mesajı yaz.");
        return new Message
        {
            TenantId = tenantId, VehicleId = vehicle.Id, ServiceRecordId = serviceRecordId,
            RecipientName = target.Name, RecipientPhone = target.Phone!, Channel = channel,
            Template = string.IsNullOrWhiteSpace(template) ? "serbest" : template.Trim(),
            IsBulk = isBulk, Body = body
        };
    }
}

/// <summary>Mesaj metninin parçaları — hitap ve imza TEK YERDE (prototip mesajHitabi / mesajImzasi).</summary>
public static class MessageText
{
    // Katlanmış biçimde (ş→s, ç→c): "Şti." → "sti.", "A.Ş." → "a.s.".
    private static readonly HashSet<string> FirmMarks =
        ["ltd", "ltd.", "sti", "sti.", "a.s.", "as", "anonim", "limited", "san", "san.", "tic", "tic.",
         "sirketi", "kollektif", "komandit"];

    /// <summary>
    /// Firma mı: ad BOŞLUKTAN bölünür, her parça eşitlikle bakılır — desenle aransaydı
    /// "Astan" içindeki "as" firma sanılırdı (K-018).
    /// </summary>
    public static bool IsFirm(string? name)
        => ServiceTypeGuess.Fold(name).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Any(FirmMarks.Contains);

    /// <summary>Kişiye "Ad Bey/Hanım, ", firmaya "Ad, " — firmaya "Ltd. Şti. Bey/Hanım" yazılmaz (A-M2).</summary>
    public static string Greeting(MessageTarget? t)
        => t is null || string.IsNullOrWhiteSpace(t.Name) ? "Merhaba, " : t.IsPerson ? t.Name + " Bey/Hanım, " : t.Name + ", ";

    public static string Signature(Shop shop) => "\n\n" + shop.Name + "\n" + (shop.Phone ?? "");

    public static string Compose(MessageTarget t, string body, Shop shop) => Greeting(t) + body + Signature(shop);
}
