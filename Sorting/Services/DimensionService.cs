using MessagePipe;
using Microsoft.JSInterop;

namespace Sorting.Services;

public class DimensionService : IAsyncDisposable
{
    private struct SizeInterop
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    const string ClassName = "DimensionExtension";
    const string GetInnerWindowDimensionsMethod = $"{ClassName}.GetInnerWindowDimensions";
    const string GetClientDimensionsMethod = $"{ClassName}.GetClientDimensions";
    const string SetResizeEventHandleMethod = $"{ClassName}.SetResizeEventHandle";
    const string ClearResizeEventHandleMethod = $"{ClassName}.ClearResizeEventHandle";

    readonly IJSRuntime jsRuntime;
    readonly IDisposablePublisher<DimensionService> resizePublisher;
    readonly CountedSubscriber<DimensionService> resizeSubscriber;
    readonly DotNetObjectReference<DimensionService>? reference;
    public ISubscriber<DimensionService> OnResize => this.resizeSubscriber;

    public DimensionService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
        this.reference = DotNetObjectReference.Create(this);
        (this.resizePublisher, var subscriber) = GlobalMessagePipe.CreateEvent<DimensionService>();
        this.resizeSubscriber = new CountedSubscriber<DimensionService>(subscriber);
        this.resizeSubscriber.CountChanged.Subscribe(async tuple =>
        {
            switch (tuple)
            {
                case { Count: > 0, Prev: <= 0 }:
                    await this.SetResizeEventHandle();
                    break;
                case { Count: <= 0, Prev: > 0 }:
                    await this.ClearResizeEventHandle();
                    break;
                default: break;
            }
        });

        Task.Run(this.SetResizeEventHandle);
    }

    private ValueTask SetResizeEventHandle() => this.jsRuntime.InvokeVoidAsync(SetResizeEventHandleMethod, this.reference);
    private ValueTask ClearResizeEventHandle() => this.jsRuntime.InvokeVoidAsync(ClearResizeEventHandleMethod);

    public async ValueTask<Dimension2D> GetInnerWindowDimensions()
    {
        var size = await this.jsRuntime.InvokeAsync<SizeInterop>(GetInnerWindowDimensionsMethod);
        return new(size.Width, size.Height);
    }

    public async ValueTask<Dimension2D> GetClientDimensions()
    {
        var size = await this.jsRuntime.InvokeAsync<SizeInterop>(GetClientDimensionsMethod);
        return new(size.Width, size.Height);
    }

    [JSInvokable]
    public void FireResizeWindow() => this.resizePublisher.Publish(this);


    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        this.reference?.Dispose();
        this.resizePublisher.Dispose();
        return this.ClearResizeEventHandle();
    }
}
