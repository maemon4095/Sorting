using Microsoft.JSInterop;
using MessagePipe;

namespace Sorting.Services;

public class LocalStorageService : IAsyncDisposable
{
    const string ClassName = "LocalStorageExtension";
    const string SetMethod = $"{ClassName}.SetLocalStorage";
    const string GetMethod = $"{ClassName}.GetLocalStorage";
    const string RemoveMethod = $"{ClassName}.RemoveLocalStorage";
    const string ContainsMethod = $"{ClassName}.LocalStorageContains";
    const string ClearMethod = $"{ClassName}.ClearLocalStorage";
    const string IsAvailableMethod = $"{ClassName}.LocalStorageIsAvailable";
    const string SetLocalStorageEventHandleMethod = $"{ClassName}.SetLocalStorageEventHandle";
    const string ClearLocalStorageEventHandleMethod = $"{ClassName}.ClearLocalStorageEventHandle";

    readonly IJSRuntime jsRuntime;
    readonly IDisposablePublisher<LocalStorageEventArgs> storageChangedPublisher;
    readonly CountedSubscriber<LocalStorageEventArgs> storageChangedSubscriber;
    readonly DotNetObjectReference<LocalStorageService>? reference;
    public ISubscriber<LocalStorageEventArgs> StorageChanged => this.storageChangedSubscriber;
    public LocalStorageService(IJSRuntime jsRuntime)
    {
        this.reference = DotNetObjectReference.Create(this);
        this.jsRuntime = jsRuntime;
        (this.storageChangedPublisher, var subscriber) = GlobalMessagePipe.CreateEvent<LocalStorageEventArgs>();
        this.storageChangedSubscriber = new CountedSubscriber<LocalStorageEventArgs>(subscriber);
        this.storageChangedSubscriber.CountChanged.Subscribe(async tuple =>
        {
            switch (tuple)
            {
                case { Count: > 0, Prev: <= 0 }:
                    await this.SetLocalStorageEventHandle();
                    break;
                case { Count: <= 0, Prev: > 0 }:
                    await this.ClearLocalStorageEventHandle();
                    break;
                default: break;
            }
        });

        Task.Run(this.SetLocalStorageEventHandle);
    }

    private ValueTask SetLocalStorageEventHandle() => this.jsRuntime.InvokeVoidAsync(SetLocalStorageEventHandleMethod, this.reference);
    private ValueTask ClearLocalStorageEventHandle() => this.jsRuntime.InvokeVoidAsync(ClearLocalStorageEventHandleMethod);

    public ValueTask<bool> IsAvailable() => this.jsRuntime.InvokeAsync<bool>(IsAvailableMethod);

    public async ValueTask Set(string key, string value) => await this.jsRuntime.InvokeVoidAsync(SetMethod, key, value);

    public ValueTask<string> Get(string key) => this.jsRuntime.InvokeAsync<string>(GetMethod, key);

    public async ValueTask Remove(string key) => await this.jsRuntime.InvokeVoidAsync(RemoveMethod, key);

    public ValueTask<bool> Contains(string key) => this.jsRuntime.InvokeAsync<bool>(ContainsMethod, key);

    public async ValueTask Clear() => await this.jsRuntime.InvokeVoidAsync(ClearMethod);

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        this.storageChangedPublisher.Dispose();
        this.reference?.Dispose();
        return this.ClearLocalStorageEventHandle();
    }

    [JSInvokable]
    public void FireStorageChanged(string category, string key, string oldValue, string newValue) => this.InvokeStorageChanged(new(Enum.Parse<LocalStorageEventCategory>(category), key, oldValue, newValue));
    private void InvokeStorageChanged(LocalStorageEventArgs args) => this.storageChangedPublisher.Publish(args);
}



