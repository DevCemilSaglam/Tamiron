using System.Net;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Tamiron.Application;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// D7 · SMS sağlayıcı adaptörü (K-076.7): Netgsm XML ucu. AĞA ÇIKILMAZ — sahte HTTP işleyici
/// isteği yakalar, yanıtı test verir. Biçim Netgsm'in resmi örneğinden (netgsm1/sms, SmsSend.php).
/// </summary>
public class NetgsmSmsSenderTests
{
    sealed class FakeHandler(string reply, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public HttpRequestMessage? Request;
        public string? Body;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            Request = r;
            Body = r.Content is null ? null : await r.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(reply) };
        }
    }

    static readonly NetgsmOptions Opt = new("8500000000", "p<&>ss", "USTAOTO");

    static (NetgsmSmsSender s, FakeHandler h) Make(string reply, HttpStatusCode status = HttpStatusCode.OK)
    {
        var h = new FakeHandler(reply, status);
        return (new NetgsmSmsSender(new HttpClient(h), Opt), h);
    }

    [Fact]
    public async Task Posts_xml_with_credentials_header_number_and_turkish_text()
    {
        var (s, h) = Make("00 123456789");
        await s.SendAsync("0532 000 00 01", "Tamiron giriş kodun: 123456. Kimseyle paylaşma.", default);

        Assert.Equal(HttpMethod.Post, h.Request!.Method);
        Assert.Equal("https://api.netgsm.com.tr/sms/send/xml", h.Request.RequestUri!.ToString());
        Assert.Equal("text/xml", h.Request.Content!.Headers.ContentType!.MediaType);

        var x = XDocument.Parse(h.Body!).Root!;
        var head = x.Element("header")!;
        Assert.Equal("TR", head.Element("company")!.Attribute("dil")!.Value);          // Türkçe harf
        Assert.Equal(("8500000000", "p<&>ss", "USTAOTO", "1:n"),
            (head.Element("usercode")!.Value, head.Element("password")!.Value, head.Element("msgheader")!.Value, head.Element("type")!.Value));
        Assert.Equal("TR", head.Element("encoding")!.Value);
        Assert.Equal("0", head.Element("filter")!.Value);   // giriş kodu ticari ileti DEĞİL: İYS süzgeci kapalı
        Assert.Equal("Tamiron giriş kodun: 123456. Kimseyle paylaşma.", x.Element("body")!.Element("msg")!.Value);
        Assert.Equal("5320000001", x.Element("body")!.Element("no")!.Value);              // başında 0 yok
    }

    [Theory]
    [InlineData("00 123")]
    [InlineData("01 123")]
    [InlineData("02 123")]
    public async Task Accepted_codes_pass(string reply) => await Make(reply).s.SendAsync("05320000001", "x", default);

    [Theory]
    [InlineData("30", "sms.provider_auth")]      // kullanıcı/şifre ya da IP izni
    [InlineData("40", "sms.provider_header")]    // başlık tanımlı değil
    [InlineData("70", "sms.provider_rejected")]  // hatalı istek
    [InlineData("", "sms.provider_rejected")]
    public async Task Error_codes_fail_loudly(string reply, string code)
    {
        var e = await Assert.ThrowsAsync<SmsSendFailedException>(() => Make(reply).s.SendAsync("05320000001", "x", default));
        Assert.Equal(code, e.Code);
    }

    [Fact]
    public async Task Http_error_fails_loudly()
        => Assert.Equal("sms.provider_unreachable", (await Assert.ThrowsAsync<SmsSendFailedException>(() =>
            Make("", HttpStatusCode.BadGateway).s.SendAsync("05320000001", "x", default))).Code);

    [Fact]
    public async Task Message_with_cdata_end_marker_survives()
    {
        var (s, h) = Make("00 1");
        await s.SendAsync("05320000001", "a]]>b", default);
        Assert.Equal("a]]>b", XDocument.Parse(h.Body!).Root!.Element("body")!.Element("msg")!.Value);
    }

    // ---- Hangi gönderici: yapılandırmaya göre ----

    static IConfiguration Conf(params (string k, string v)[] kv)
        => new ConfigurationBuilder().AddInMemoryCollection(kv.Select(x => new KeyValuePair<string, string?>(x.k, x.v))).Build();

    [Fact]
    public void Options_come_from_configuration_and_are_all_required()
    {
        Assert.Equal(Opt, NetgsmOptions.From(Conf(("Sms:Netgsm:UserCode", "8500000000"), ("Sms:Netgsm:Password", "p<&>ss"), ("Sms:Netgsm:Header", "USTAOTO"))));
        Assert.Null(NetgsmOptions.From(Conf(("Sms:Netgsm:UserCode", "8500000000"), ("Sms:Netgsm:Password", "x"))));   // başlık yok
        Assert.Null(NetgsmOptions.From(Conf()));
    }

    [Fact]
    public void Options_never_print_the_password()
        => Assert.DoesNotContain("p<&>ss", Opt.ToString());
}
