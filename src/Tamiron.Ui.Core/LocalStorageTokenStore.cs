using Microsoft.JSInterop;

namespace Tamiron.Ui.Core;

/// <summary>
/// Tarayıcının yerel deposu: telefon kapanıp açılsa da oturum kalır (K-074.2 "tekrar tekrar girmek zorunda kalmasın").
/// H7 · Anahtar uygulama başına: tamirhane ve yedek parça AYNI adresten sunulur (K-091), yerel depo ortaktır —
/// biri ötekinin oturumunu almasın.
/// </summary>
public sealed class LocalStorageTokenStore : ITokenStore
{
    private readonly IJSRuntime js;
    private readonly string key;

    public LocalStorageTokenStore(IJSRuntime js) : this(js, "tamiron.oturum") { }
    public LocalStorageTokenStore(IJSRuntime js, string key) { this.js = js; this.key = key; }

    public ValueTask<string?> GetAsync() => js.InvokeAsync<string?>("localStorage.getItem", key);
    public async ValueTask SetAsync(string? token)
    {
        if (string.IsNullOrEmpty(token)) await js.InvokeVoidAsync("localStorage.removeItem", key);
        else await js.InvokeVoidAsync("localStorage.setItem", key, token);
    }
}
