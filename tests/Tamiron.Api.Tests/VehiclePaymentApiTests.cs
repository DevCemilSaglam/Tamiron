using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Accounting;
using Tamiron.Application.Lists;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>
/// C4 · Araç ödeme durumu — prototip 60-arac arOdeme: muhasebedeki alacak listesiyle
/// AYNI kaynak, araca süzülmüş. Satır metni şikâyet, boşsa tür adı.
/// </summary>
public class VehiclePaymentApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<Guid> VehicleOf(HttpClient api, string plate)
        => (await api.GetFromJsonAsync<VehicleList>($"/api/vehicles/list?q={Uri.EscapeDataString(plate)}", Json))!.Items.Single().Id;

    [Fact]
    public async Task Vehicle_receivables_are_the_same_list_filtered()
    {
        var api = f.For(ApiFactory.ShopA);
        var demir = await api.GetFromJsonAsync<ReceivableList>($"/api/receivables?vehicleId={await VehicleOf(api, "06 TMR 07")}", Json);
        var aku = Assert.Single(demir!.Items);
        Assert.Equal(("Akü değişimi", 3450m), (aku.Description, aku.Total));
        Assert.Equal(3450m, demir.Total);

        var mehmet = await api.GetFromJsonAsync<ReceivableList>($"/api/receivables?vehicleId={await VehicleOf(api, "34 ABC 123")}", Json);
        Assert.Empty(mehmet!.Items);
        Assert.Equal(0m, mehmet.Total);
    }

    [Fact]
    public async Task Empty_complaint_shows_type_label()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await (await api.PostAsJsonAsync("/api/records", new { plate = "34 ODE 01", complaint = "" }, Json)).Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        await api.PostAsJsonAsync($"/api/records/{r!.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 400 }, Json);
        (await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Unpaid" }, Json)).EnsureSuccessStatusCode();
        var l = await api.GetFromJsonAsync<ReceivableList>($"/api/receivables?vehicleId={r.VehicleId}", Json);
        Assert.Equal(("Tamirat", 400m), (Assert.Single(l!.Items).Description, l.Total));
    }
}
