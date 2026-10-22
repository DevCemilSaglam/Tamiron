using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Lists;
using Tamiron.Application.Messages;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>B11 · Mesaj arşivi + toplu alıcı süzgeci. Hiçbir şey GÖNDERİLMEZ; kaydedilir.</summary>
public class MessageApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    async Task<Guid> VehicleOfA(string plate)
        => (await Read<VehicleList>(await f.For(ApiFactory.ShopA).GetAsync($"/api/vehicles/list?q={Uri.EscapeDataString(plate)}"))).Items.Single().Id;

    [Fact]
    public async Task Single_message_goes_to_default_target_and_archive_is_newest_first()
    {
        var api = f.For(ApiFactory.ShopA);
        var demir = await VehicleOfA("06 TMR 07");
        var m1 = await Read<MessageDto>(await api.PostAsJsonAsync("/api/messages",
            new { vehicleId = demir, channel = "WhatsApp", template = "hazir", body = "Hasan Bey, aracınız hazır." }, Json));
        Assert.Equal(("Hasan Demir", "0542 331 90 07"), (m1.RecipientName, m1.RecipientPhone));   // iletişim, santral değil
        Assert.False(m1.IsBulk);

        await Read<MessageDto>(await api.PostAsJsonAsync("/api/messages",
            new { vehicleId = demir, channel = "Sms", template = "serbest", body = "ikinci", target = "Owner" }, Json));
        var arsiv = await Read<List<MessageDto>>(await api.GetAsync($"/api/messages?vehicleId={demir}"));
        Assert.Equal(new[] { "ikinci", "Hasan Bey, aracınız hazır." }, arsiv.Select(m => m.Body).Take(2));
        Assert.Equal("0312 395 12 00", arsiv[0].RecipientPhone);                                    // açıkça sahip seçildi
    }

    [Fact]
    public async Task No_phone_no_message()
    {
        var res = await f.For(ApiFactory.ShopA).PostAsJsonAsync("/api/messages",
            new { vehicleId = await VehicleOfA("35 KLM 45"), channel = "WhatsApp", template = "serbest", body = "x" }, Json);   // Ayşe Kaya: telefon yok
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
        Assert.Equal("message.no_phone", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Bulk_goes_only_to_consenting_customers_each_with_own_greeting_and_record()
    {
        var api = f.For(ApiFactory.ShopB);
        // B dükkânı: üç müşteri; ikisi telefonlu, biri izinli.
        async Task<Guid> Arac(string plate, object patch)
        {
            var r = await Read<Tamiron.Application.ServiceRecords.ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "" }, Json));
            await Read<Tamiron.Application.Vehicles.VehicleDto>(await api.PatchAsJsonAsync($"/api/vehicles/{r.VehicleId}", patch, Json));
            return r.VehicleId;
        }
        await Arac("34 TOP 01", new { ownerName = "Ali Veli", ownerPhone = "05320000001" });
        await Arac("34 TOP 02", new { ownerName = "Kaya Ltd. Şti.", ownerPhone = "05320000002" });
        await Arac("34 TOP 03", new { ownerName = "Telefonsuz Tamer" });
        var ali = (await Read<Tamiron.Application.Customers.CustomerList>(await api.GetAsync("/api/customers?q=ali veli"))).Items.Single();
        await Read<Tamiron.Application.Customers.CustomerDetail>(await api.PutAsJsonAsync("/api/customers/consent", new { key = ali.Key, granted = true }, Json));

        var onizleme = await Read<BulkPreview>(await api.GetAsync("/api/messages/bulk-preview"));
        Assert.Equal(new[] { "Ali Veli" }, onizleme.Recipients.Select(r => r.Name));
        Assert.Contains("Kaya Ltd. Şti.", onizleme.WithoutConsent.Select(r => r.Name));          // gitmeyecek, sayılır
        Assert.DoesNotContain("Telefonsuz Tamer", onizleme.WithoutConsent.Select(r => r.Name));  // telefonsuz aday bile değil

        var sonuc = await Read<BulkResult>(await api.PostAsJsonAsync("/api/messages/bulk",
            new { channel = "WhatsApp", template = "bayram", body = "Bayramınız kutlu olsun." }, Json));
        Assert.Equal(1, sonuc.Recorded);
        var m = (await Read<List<MessageDto>>(await api.GetAsync("/api/messages"))).First();
        Assert.True(m.IsBulk);
        Assert.StartsWith("Ali Veli Bey/Hanım, Bayramınız kutlu olsun.", m.Body);
        Assert.EndsWith("\n\nÖteki Dükkân\n", m.Body);                                          // imza: dükkân adı + (boş) telefon
    }
}
