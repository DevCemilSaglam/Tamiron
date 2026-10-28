using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Tamiron.Ui.Core;
using Tamiron.YedekParca.Web;

// H7 · Yedek parça web kabuğu (MIMARI §2, K-091). Ekran YOK — ekranlar Tamiron.YedekParca.Ui'de.
// Aynı adresten /parca/ altında sunulur: API kökü sayfanın kökü DEĞİL, adresin köküdür (yoksa istek /parca/api'ye
// giderdi). Oturum anahtarı ayrı: tamirhaneyle aynı tarayıcı deposunu paylaşır.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBase"] is { Length: > 0 } a ? a : new Uri(new Uri(builder.HostEnvironment.BaseAddress), "/").ToString();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<ITokenStore>(sp => new LocalStorageTokenStore(sp.GetRequiredService<IJSRuntime>(), "tamiron.parca.oturum"));
builder.Services.AddScoped<TamironApi>();

await builder.Build().RunAsync();
