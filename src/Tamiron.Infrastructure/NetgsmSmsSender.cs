using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Tamiron.Application;

namespace Tamiron.Infrastructure;

/// <summary>
/// Netgsm hesabı (K-076.7). ORTAM DEĞİŞKENİNDEN okunur, depoya girmez:
/// <c>Sms__Netgsm__UserCode</c> · <c>Sms__Netgsm__Password</c> · <c>Sms__Netgsm__Header</c>
/// (başlık = Netgsm'de onaylı gönderici adı). Biri eksikse hesap YOK sayılır.
/// </summary>
public sealed record NetgsmOptions(string UserCode, string Password, string Header)
{
    public static NetgsmOptions? From(IConfiguration c)
    {
        var s = c.GetSection("Sms:Netgsm");
        string? V(string k) => string.IsNullOrWhiteSpace(s[k]) ? null : s[k]!.Trim();
        return V("UserCode") is { } u && V("Password") is { } p && V("Header") is { } h ? new(u, p, h) : null;
    }

    /// <summary>Parola günlüğe/hata metnine düşmesin.</summary>
    public override string ToString() => $"NetgsmOptions {{ UserCode = {UserCode}, Header = {Header}, Password = *** }}";
}

/// <summary>
/// Netgsm XML ucu — biçim Netgsm'in resmi PHP örneğinden (github.com/netgsm1/sms, SmsSend.php):
/// POST https://api.netgsm.com.tr/sms/send/xml, yanıt "KOD bulkid"; 00/01/02 kabul, gerisi hata.
/// ⚠️ Gerçek hesapla HİÇ koşturulmadı (para, Cemil'in kararı). İlk gerçek gönderimden önce
/// Netgsm panelindeki güncel belgeyle karşılaştırılır.
/// </summary>
public sealed class NetgsmSmsSender(HttpClient http, NetgsmOptions o) : ISmsSender
{
    public const string Url = "https://api.netgsm.com.tr/sms/send/xml";

    public async Task SendAsync(string phone, string text, CancellationToken ct)
    {
        var body = new XElement("mainbody",
            new XElement("header",
                new XElement("company", new XAttribute("dil", "TR"), "Netgsm"),
                new XElement("usercode", o.UserCode),
                new XElement("password", o.Password),
                new XElement("type", "1:n"),
                new XElement("msgheader", o.Header),
                // Giriş/kayıt kodu ticari ileti DEĞİL: İYS süzgeci kapalı (0). Toplu kampanya bu yoldan gitmez.
                new XElement("filter", "0"),
                new XElement("encoding", "TR")),
            new XElement("body",
                new XElement("msg", new XCData(text)),
                new XElement("no", Number(phone))));

        using var content = new StringContent(body.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
        HttpResponseMessage res;
        try { res = await http.PostAsync(Url, content, ct); }
        catch (HttpRequestException) { throw new SmsSendFailedException("sms.provider_unreachable"); }
        using (res)
        {
            if (!res.IsSuccessStatusCode) throw new SmsSendFailedException("sms.provider_unreachable");
            var code = (await res.Content.ReadAsStringAsync(ct)).Trim().Split(' ', 2)[0];
            switch (code)
            {
                case "00" or "01" or "02": return;
                case "30": throw new SmsSendFailedException("sms.provider_auth");     // kullanıcı/şifre, API izni ya da IP kısıtı
                case "40": throw new SmsSendFailedException("sms.provider_header");   // gönderici adı hesapta tanımlı değil
                default: throw new SmsSendFailedException("sms.provider_rejected");
            }
        }
    }

    /// <summary>"0532 000 00 01" → "5320000001": Netgsm numarayı başında 0 olmadan bekler.</summary>
    public static string Number(string phone)
    {
        var d = new string(phone.Where(char.IsDigit).ToArray());
        if (d.StartsWith("90") && d.Length == 12) d = d[2..];
        return d.TrimStart('0');
    }
}
