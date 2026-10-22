using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>B4 · PATCH /api/records/{id}: verilmeyen alana dokunulmaz.</summary>
public class RecordFlagsApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<ServiceRecordDto> Read(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<ServiceRecordDto>(Json))!;
    }

    [Fact]
    public async Task Patch_changes_only_given_fields_and_waiting_day_comes_from_server()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read(await api.PostAsJsonAsync("/api/records", new { plate = "34 BYR 01", complaint = "fren sesi" }, Json));
        Assert.Equal(ServiceType.Repair, r.Type);

        r = await Read(await api.PatchAsJsonAsync($"/api/records/{r.Id}", new { complaint = "periyodik bakım", km = "86400" }, Json));
        Assert.Equal(ServiceType.Maintenance, r.Type);    // tür metni takip etti
        Assert.Equal("86.400", r.Km);

        r = await Read(await api.PatchAsJsonAsync($"/api/records/{r.Id}", new { waitingForPart = true, showWaitingToCustomer = true }, Json));
        Assert.True(r.WaitingForPart);
        Assert.Equal(f.Clock.Today, r.WaitingSince);
        Assert.True(r.ShowWaitingToCustomer);
        Assert.Equal("periyodik bakım", r.Complaint);      // dokunulmadı
        Assert.Equal("86.400", r.Km);

        r = await Read(await api.PatchAsJsonAsync($"/api/records/{r.Id}", new { type = "Both" }, Json));
        Assert.Equal(ServiceType.Both, r.Type);
        Assert.True(r.TypeSetManually);
        Assert.True(r.WaitingForPart);                     // dokunulmadı
    }

    [Fact]
    public async Task Showing_waiting_without_waiting_is_rejected()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read(await api.PostAsJsonAsync("/api/records", new { plate = "34 BYR 02", complaint = "x" }, Json));
        var res = await api.PatchAsJsonAsync($"/api/records/{r.Id}", new { showWaitingToCustomer = true }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
    }
}
