using Microsoft.JSInterop;

namespace Sorting;

public class LocalStorageService
{
    const string ClassName = "LocalStorageExtension";
    const string SetMethod = $"{ClassName}.SetLocalStorage";
    const string GetMethod = $"{ClassName}.GetLocalStorage";
    const string RemoveMethod = $"{ClassName}.RemoveLocalStorage";
    const string ContainsMethod = $"{ClassName}.LocalStorageContains";
    const string ClearMethod = $"{ClassName}.ClearLocalStorage";
    const string IsAvailableMethod = $"{ClassName}.LocalStorageIsAvailable";

    private readonly IJSRuntime jsRuntime;

    public event EventHandler<string>? StorageChanged;

    public LocalStorageService(IJSRuntime jsRuntime) => this.jsRuntime = jsRuntime;


    public ValueTask<bool> IsAvailable() => this.jsRuntime.InvokeAsync<bool>(IsAvailableMethod);

    public async ValueTask Set(string key, string value)
    {
        await this.jsRuntime.InvokeVoidAsync(SetMethod, key, value);
        this.InvokeStorageChanged(key);
    }

    public ValueTask<string> Get(string key) => this.jsRuntime.InvokeAsync<string>(GetMethod, key);

    public async ValueTask Remove(string key)
    {
        await this.jsRuntime.InvokeVoidAsync(RemoveMethod, key);
        this.InvokeStorageChanged(key);
    }

    public ValueTask<bool> Contains(string key) => this.jsRuntime.InvokeAsync<bool>(ContainsMethod, key);

    public ValueTask Clear() => this.jsRuntime.InvokeVoidAsync(ClearMethod);

    private void InvokeStorageChanged(string key) => this.StorageChanged?.Invoke(this, key);
}
