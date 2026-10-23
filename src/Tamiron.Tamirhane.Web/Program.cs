using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tamiron.Tamirhane.Web;
using Tamiron.Ui.Core;

// D20 · Tamirhane web kabuğu (K-076.19). Ekran YOK — ekranlar Tamiron.Tamirhane.Ui'de (K-015:
// web ≡ mobil, host'a ekran eklenmez). Burası yalnız bağlar: API adresi, oturum deposu.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API adresi yapılandırmadan (wwwroot/appsettings.*.json); yoksa AYNI köken — üretimde arayüz
// API'yle aynı yerden sunulur ve CORS gerekmez (K-076.10).
var apiBase = builder.Configuration["ApiBase"] is { Length: > 0 } a ? a : builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<TamironApi>();

await builder.Build().RunAsync();
