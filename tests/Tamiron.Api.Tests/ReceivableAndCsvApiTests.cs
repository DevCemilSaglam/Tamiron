using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Accounting;

namespace Tamiron.Api.Tests;

/// <summary>B9 · Alacak listesi (döneme tabi değil) ve muhasebe CSV'si.</summary>
public class ReceivableAndCsvApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task Receivables_are_a_balance_not_bound_to_period()
    {
        // A'nın tohumundaki veresiye 55 gün önce teslim edildi: "bugün" süzgecinde de görünür.
        var l = (await f.For(ApiFactory.ShopA).GetFromJsonAsync<ReceivableList>("/api/receivables", Json))!;
        var akü = Assert.Single(l.Items);
        Assert.Equal(("06 TMR 07", "Demir Nakliyat Ltd. Şti.", 3450m), (akü.PlateNo, akü.OwnerName, akü.Total));
        Assert.Equal(3450m, l.Total);
    }

    [Fact]
    public async Task Csv_has_header_rows_in_order_and_is_a_utf8_bom_download()
    {
        var api = f.For(ApiFactory.ShopA);
        await api.PostAsJsonAsync("/api/expenses", new { description = "=HAT; \"deneme\"", amount = 1234 }, Json);

        var res = await api.GetAsync("/api/export/cash.csv?period=Year");
        res.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", res.Content.Headers.ContentType!.MediaType);
        Assert.StartsWith("tamiron-muhasebe-year-", res.Content.Headers.ContentDisposition!.FileNameStar ?? res.Content.Headers.ContentDisposition.FileName);

        var bytes = await res.Content.ReadAsByteArrayAsync();
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);   // BOM: yoksa Excel Türkçe harfleri bozar
        var lines = Encoding.UTF8.GetString(bytes[3..]).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("Tür;Tarih;Açıklama;Plaka;Müşteri;Ödeme;Tutar", lines[0]);
        // Tohum: yıl içinde 2 tahsilat (havale haziran, nakit bugün) — tarih sırasıyla; sonra gider; sonra alacak.
        Assert.StartsWith("Tahsilat;20.06.", lines[1]);
        Assert.EndsWith(";Havale;2850", lines[1]);
        Assert.StartsWith("Tahsilat;22.10.2026;Ön cam silecek motoru;35 KLM 45;Ayşe Kaya;Nakit;2800", lines[2]);
        Assert.Equal("Gider;22.10.2026;\"'=HAT; \"\"deneme\"\"\";;;;1234", lines[3]);
        Assert.Equal("Alacak;28.08.2026;Akü değişimi;06 TMR 07;Demir Nakliyat Ltd. Şti.;Alınmadı;3450", lines[4]);
        Assert.Equal(5, lines.Length);
    }
}
