using Microsoft.JSInterop;

namespace Sorting;

public class LocalStorageService
{
    class WeakEventLink
    {
        public WeakReference<EventHandler<string>> HandlerReference;

        public WeakEventLink? Next;

        public bool TryGet(out EventHandler<string>? handler) => this.HandlerReference.TryGetTarget(out handler);

        public WeakEventLink(EventHandler<string> handler)
        {
            this.HandlerReference = new(handler);
        }
    }

    const string ClassName = "LocalStorageExtension";
    const string SetMethod = $"{ClassName}.SetLocalStorage";
    const string GetMethod = $"{ClassName}.GetLocalStorage";
    const string RemoveMethod = $"{ClassName}.RemoveLocalStorage";
    const string ContainsMethod = $"{ClassName}.LocalStorageContains";
    const string ClearMethod = $"{ClassName}.ClearLocalStorage";
    const string IsAvailableMethod = $"{ClassName}.LocalStorageIsAvailable";

    readonly object _lock = new();
    readonly IJSRuntime jsRuntime;
    WeakEventLink? handlers = null;

    public event EventHandler<string> StorageChanged
    {
        add
        {
            lock (this._lock)
            {
                var head = this.handlers;
                var link = new WeakEventLink(value);
                this.handlers = link;
                if (head is not null) head.Next = link;
            }
        }
        remove
        {
            lock (this._lock)
            {
                ref var head = ref this.handlers;
                while (head is not null)
                {
                    if (!head.TryGet(out var handler) || handler == value)
                    {
                        head = head.Next;
                        if (head is null) break;
                    }
                    head = ref head.Next;
                }
            }
        }
    }

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



    private void InvokeStorageChanged(string key)
    {
        lock (this._lock)
        {
            ref var head = ref this.handlers;
            while (head is not null)
            {
                if (head.TryGet(out var handler))
                {
                    handler?.Invoke(this, key);
                }
                else
                {
                    head = head.Next;
                    if (head is null) break;
                }
                head = ref head.Next;
            }
        }
    }
}
