using Microsoft.JSInterop;
using Sorting.Collections;

namespace Sorting.Storage;

public class LocalStorageService
{
    const string ClassName = "LocalStorageExtension";
    const string SetMethod = $"{ClassName}.SetLocalStorage";
    const string GetMethod = $"{ClassName}.GetLocalStorage";
    const string RemoveMethod = $"{ClassName}.RemoveLocalStorage";
    const string ContainsMethod = $"{ClassName}.LocalStorageContains";
    const string ClearMethod = $"{ClassName}.ClearLocalStorage";
    const string IsAvailableMethod = $"{ClassName}.LocalStorageIsAvailable";

    readonly IJSRuntime jsRuntime;
    readonly WeakLinkedList<LocalStorageEventHandler> handlers = new();

    public event LocalStorageEventHandler StorageChanged
    {
        add
        {
            lock (this.handlers) this.handlers.Add(value);
        }
        remove
        {
            lock (this.handlers) this.handlers.Remove(value);
        }
    }

    public LocalStorageService(IJSRuntime jsRuntime) => this.jsRuntime = jsRuntime;


    public ValueTask<bool> IsAvailable() => this.jsRuntime.InvokeAsync<bool>(IsAvailableMethod);

    public async ValueTask Set(string key, string value)
    {
        await this.jsRuntime.InvokeVoidAsync(SetMethod, key, value);
        this.InvokeStorageChanged(new LocalStorageEventArgs(LocalStorageEventCategory.Set, key, value));
    }

    public ValueTask<string> Get(string key) => this.jsRuntime.InvokeAsync<string>(GetMethod, key);

    public async ValueTask Remove(string key)
    {
        await this.jsRuntime.InvokeVoidAsync(RemoveMethod, key);
        this.InvokeStorageChanged(new LocalStorageEventArgs(LocalStorageEventCategory.Remove, key, null));
    }

    public ValueTask<bool> Contains(string key) => this.jsRuntime.InvokeAsync<bool>(ContainsMethod, key);

    public async ValueTask Clear()
    {
        await this.jsRuntime.InvokeVoidAsync(ClearMethod);
        this.InvokeStorageChanged(new LocalStorageEventArgs(LocalStorageEventCategory.Clear, null, null));
    }

    private void InvokeStorageChanged(LocalStorageEventArgs args)
    {
        lock (this.handlers)
        {
            foreach (var handler in this.handlers)
                handler(this, args);
        }
    }
}



