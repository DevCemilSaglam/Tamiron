using Microsoft.JSInterop;

namespace Tamiron.Ui.Core;

/// <summary>Tarayıcının yerel deposu: telefon kapanıp açılsa da oturum kalır (K-074.2 "tekrar tekrar girmek zorunda kalmasın").</summary>
public sealed class LocalStorageTokenStore(IJSRuntime js) : ITokenStore
{
    private const string Key = "tamiron.oturum";
    public ValueTask<string?> GetAsync() => js.InvokeAsync<string?>("localStorage.getItem", Key);
    public async ValueTask SetAsync(string? token)
    {
        if (string.IsNullOrEmpty(token)) await js.InvokeVoidAsync("localStorage.removeItem", Key);
        else await js.InvokeVoidAsync("localStorage.setItem", Key, token);
    }
}
