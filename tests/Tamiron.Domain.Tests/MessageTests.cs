using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B11 · Mesaj — prototip mesajHedefleri / mesajHedefi / firmaMi / mesajHitabi / mesajImzasi.
/// Gönderim YOK (dış servis, para); yalnız kim, ne, hangi metin.
/// </summary>
public class MessageTests
{
    [Theory]
    [InlineData("Demir Nakliyat Ltd. Şti.", true)]
    [InlineData("Öz Yedek Parça San. Tic. A.Ş.", true)]
    [InlineData("Astan Usta", false)]      // "as" kelimenin içinde: firma DEĞİL (K-018)
    [InlineData("Hasan Demir", false)]
    public void Firm_detection_by_whole_words(string name, bool firm) => Assert.Equal(firm, MessageText.IsFirm(name));

    [Fact]
    public void Contact_comes_first_default_is_first_with_phone()
    {
        var v = new Vehicle { OwnerName = "Demir Nakliyat Ltd. Şti.", OwnerPhone = "0312 395 12 00",
                              ContactName = "Hasan Demir", ContactPhone = "0542 331 90 07" };
        Assert.Equal(new[] { MessageTargetKind.Contact, MessageTargetKind.Owner }, v.MessageTargets().Select(t => t.Kind));
        Assert.Equal("Hasan Demir", v.DefaultTarget()!.Name);

        v.ContactPhone = null;   // iletişim adı var, telefonu yok: telefonu olan sahip varsayılan
        Assert.Equal("0312 395 12 00", v.DefaultTarget()!.Phone);
    }

    [Theory]
    [InlineData("Mehmet Kaya", "Mehmet Kaya Bey/Hanım, ")]
    [InlineData("Demir Nakliyat Ltd. Şti.", "Demir Nakliyat Ltd. Şti., ")]   // firmaya "Bey/Hanım" yok (A-M2)
    [InlineData("", "Merhaba, ")]
    public void Greeting(string name, string expected)
        => Assert.Equal(expected, MessageText.Greeting(new MessageTarget(MessageTargetKind.Owner, name, "0532 000 00 00")));

    [Fact]
    public void Compose_is_greeting_body_signature()
    {
        var shop = new Shop { Name = "Ahmet Usta Oto Servis", Phone = "0532 000 00 00" };
        var t = new MessageTarget(MessageTargetKind.Owner, "Mehmet Kaya", "0532 215 30 00");
        Assert.Equal("Mehmet Kaya Bey/Hanım, Bayramınız kutlu olsun.\n\nAhmet Usta Oto Servis\n0532 000 00 00",
                     MessageText.Compose(t, "Bayramınız kutlu olsun.", shop));
    }
}
