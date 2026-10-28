using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.PriceRequests;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Suppliers;

namespace Tamiron.Api.Tests;

/// <summary>
/// H3 · Parçacıya borç (K-090, TASARIM-H-MUHASEBE). Parça sorda cevap kayda yazılınca parça o parçacıdan
/// alınmış olur; "veresiye" denirse borç doğar. Elle de işaretlenir. Ödeme ≤ borç, geri alınır. Silinen kayıt
/// borçtan düşer. Ödeme gider değil, kasa tahsilatı değil; CSV'de "Parçacıya ödeme".
/// </summary>
public class SupplierDebtApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        Assert.True(res.IsSuccessStatusCode, $"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    static async Task<string?> Code(HttpResponseMessage res) => (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

    record Ortam(HttpClient Api, ServiceRecordDto Kayit, SupplierDto Oz);

    async Task<Ortam> Hazirla(string plate)
    {
        var api = f.For(await f.NewShopAsync());
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "Fren" }, Json));
        await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{r.VehicleId}",
            new { brand = "Fiat", model = "Egea", year = "2021", engine = "1.4 Fire" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Balata", price = 0 }, Json));
        var oz = await Read<SupplierDto>(await api.PostAsJsonAsync("/api/suppliers", new { name = "Öz Parça", phone = "05331000001" }, Json));
        return new(api, r, oz);
    }

    static async Task<ServiceRecordDto> Cevapla(Ortam o, bool veresiye)
    {
        var sorular = await Read<List<PriceRequestDto>>(await o.Api.PostAsJsonAsync($"/api/records/{o.Kayit.Id}/price-requests",
            new { supplierIds = new[] { o.Oz.Id } }, Json));
        return await Read<ServiceRecordDto>(await o.Api.PostAsJsonAsync($"/api/price-requests/{sorular[0].Id}/replies",
            new { lineId = o.Kayit.Lines[0].Id, rawText = "1200", price = 1200, customerPrice = 1500, onCredit = veresiye }, Json));
    }

    record Borc(Guid SupplierId, string Name, string Phone, decimal Debt, List<BorcSatir> Lines, List<Odeme> Payments);
    record BorcSatir(Guid RecordId, Guid LineId, string PlateNo, string Name, decimal Cost);
    record Odeme(Guid Id, decimal Amount, string Method, DateOnly PaidOn);
    record BorcListe(decimal Total, List<Borc> Items);

    static async Task<BorcListe> Borclar(HttpClient api) => await Read<BorcListe>(await api.GetAsync("/api/supplier-debts"));

    [Fact]
    public async Task Reply_on_credit_makes_a_debt_to_that_supplier()
    {
        var o = await Hazirla("34 BR 101");
        var r = await Cevapla(o, veresiye: true);
        Assert.Equal(o.Oz.Id, r.Lines[0].SupplierId);
        Assert.True(r.Lines[0].OnCredit);

        var b = await Borclar(o.Api);
        var oz = Assert.Single(b.Items);
        Assert.Equal(1200m, oz.Debt);
        Assert.Equal(1200m, b.Total);
        Assert.Equal("Balata", Assert.Single(oz.Lines).Name);
        Assert.Equal("34 BR 101", oz.Lines[0].PlateNo);
    }

    [Fact]
    public async Task Reply_paid_on_the_spot_knows_the_supplier_but_owes_nothing()
    {
        var o = await Hazirla("34 BR 102");
        var r = await Cevapla(o, veresiye: false);
        Assert.Equal(o.Oz.Id, r.Lines[0].SupplierId);
        Assert.False(r.Lines[0].OnCredit);
        Assert.Empty((await Borclar(o.Api)).Items);
    }

    [Fact]
    public async Task Paying_lowers_the_debt_cannot_overpay_and_can_be_undone()
    {
        var o = await Hazirla("34 BR 103");
        await Cevapla(o, veresiye: true);
        var b = await Read<BorcListe>(await o.Api.PostAsJsonAsync($"/api/suppliers/{o.Oz.Id}/payments", new { amount = 500, method = "Cash" }, Json));
        Assert.Equal(700m, b.Items[0].Debt);
        var odeme = Assert.Single(b.Items[0].Payments);
        Assert.Equal(500m, odeme.Amount);

        var fazla = await o.Api.PostAsJsonAsync($"/api/suppliers/{o.Oz.Id}/payments", new { amount = 701, method = "Cash" }, Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fazla.StatusCode);
        Assert.Equal("supplier_payment.over_debt", await Code(fazla));

        b = await Read<BorcListe>(await o.Api.PostAsync($"/api/supplier-payments/{odeme.Id}/undo", null));
        Assert.Equal(1200m, b.Items[0].Debt);
        Assert.Empty(b.Items[0].Payments);
    }

    [Fact]
    public async Task Paid_off_supplier_still_shows_its_last_payment_to_undo()
    {
        var o = await Hazirla("34 BR 104");
        await Cevapla(o, veresiye: true);
        var b = await Read<BorcListe>(await o.Api.PostAsJsonAsync($"/api/suppliers/{o.Oz.Id}/payments", new { amount = 1200, method = "Transfer" }, Json));
        var oz = Assert.Single(b.Items);
        Assert.Equal(0m, oz.Debt);
        Assert.Equal(0m, b.Total);
        Assert.Single(oz.Payments);
    }

    [Fact]
    public async Task A_line_can_be_marked_by_hand_and_unmarked()
    {
        var o = await Hazirla("34 BR 105");
        var lid = o.Kayit.Lines[0].Id;
        await Read<ServiceRecordDto>(await o.Api.PutAsJsonAsync($"/api/records/{o.Kayit.Id}/lines/{lid}",
            new { kind = "Part", name = "Balata", price = 1500, cost = 900 }, Json));
        var r = await Read<ServiceRecordDto>(await o.Api.PatchAsJsonAsync($"/api/records/{o.Kayit.Id}/lines/{lid}/purchase",
            new { supplierId = o.Oz.Id, onCredit = true }, Json));
        Assert.True(r.Lines[0].OnCredit);
        Assert.Equal(900m, (await Borclar(o.Api)).Total);

        await Read<ServiceRecordDto>(await o.Api.PatchAsJsonAsync($"/api/records/{o.Kayit.Id}/lines/{lid}/purchase",
            new { supplierId = o.Oz.Id, onCredit = false }, Json));
        Assert.Equal(0m, (await Borclar(o.Api)).Total);
    }

    [Fact]
    public async Task Credit_without_cost_is_refused()
    {
        var o = await Hazirla("34 BR 106");
        var res = await o.Api.PatchAsJsonAsync($"/api/records/{o.Kayit.Id}/lines/{o.Kayit.Lines[0].Id}/purchase",
            new { supplierId = o.Oz.Id, onCredit = true }, Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("line.credit_not_possible", await Code(res));
    }

    [Fact]
    public async Task Another_shops_supplier_cannot_be_used()
    {
        var o = await Hazirla("34 BR 107");
        var baska = f.For(await f.NewShopAsync());
        var yabanci = await Read<SupplierDto>(await baska.PostAsJsonAsync("/api/suppliers", new { name = "Yabancı", phone = "05339990001" }, Json));
        await Read<ServiceRecordDto>(await o.Api.PutAsJsonAsync($"/api/records/{o.Kayit.Id}/lines/{o.Kayit.Lines[0].Id}",
            new { kind = "Part", name = "Balata", price = 1500, cost = 900 }, Json));
        var res = await o.Api.PatchAsJsonAsync($"/api/records/{o.Kayit.Id}/lines/{o.Kayit.Lines[0].Id}/purchase",
            new { supplierId = yabanci.Id, onCredit = true }, Json);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Deleted_record_leaves_the_debt()
    {
        var o = await Hazirla("34 BR 108");
        await Cevapla(o, veresiye: true);
        Assert.Equal(HttpStatusCode.NoContent, (await o.Api.DeleteAsync($"/api/records/{o.Kayit.Id}")).StatusCode);
        Assert.Equal(0m, (await Borclar(o.Api)).Total);
    }

    [Fact]
    public async Task Payment_is_not_an_expense_nor_cash_in_and_goes_to_the_csv_with_its_own_type()
    {
        var o = await Hazirla("34 BR 109");
        await Cevapla(o, veresiye: true);
        await Read<BorcListe>(await o.Api.PostAsJsonAsync($"/api/suppliers/{o.Oz.Id}/payments", new { amount = 400, method = "Cash" }, Json));

        var hesap = await Read<JsonElement>(await o.Api.GetAsync("/api/accounting?period=Today"));
        Assert.Equal(0m, hesap.GetProperty("cash").GetProperty("cash").GetDecimal());
        Assert.Equal(0m, hesap.GetProperty("expenses").GetProperty("total").GetDecimal());

        var csv = await (await o.Api.GetAsync("/api/export/cash.csv?period=Today")).Content.ReadAsStringAsync();
        Assert.Contains("Parçacıya ödeme;", csv);
        Assert.Contains("Öz Parça", csv);
    }
}
