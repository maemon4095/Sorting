using MessagePipe;

namespace Sorting;

internal class CountedSubscriber<TMessage> : IDisposable, ISubscriber<TMessage>
{
    class Disposable : IDisposable
    {
        readonly IDisposable disposable;
        readonly CountedSubscriber<TMessage> subscriber;


        public Disposable(IDisposable disposable, CountedSubscriber<TMessage> subscriber)
        {
            this.disposable = disposable;
            this.subscriber = subscriber;
        }

        public void Dispose()
        {
            this.disposable.Dispose();
            this.subscriber.Delta(-1);
        }
    }

    public CountedSubscriber(EventFactory factory, ISubscriber<TMessage> subscriber)
        : this(factory.CreateEvent<(int, int)>(), subscriber) { }
    public CountedSubscriber(ISubscriber<TMessage> subscriber)
        : this(GlobalMessagePipe.CreateEvent<(int, int)>(), subscriber) { }
    private CountedSubscriber((IDisposablePublisher<(int, int)>, ISubscriber<(int, int)>) countChanged, ISubscriber<TMessage> subscriber)
    {
        (this.countChangedPublisher, this.CountChanged) = countChanged;
        this.subscriber = subscriber;
    }


    readonly ISubscriber<TMessage> subscriber;
    readonly IDisposablePublisher<(int Count, int Prev)> countChangedPublisher;
    public int Count { get; private set; } = 0;
    public ISubscriber<(int Count, int Prev)> CountChanged { get; }

    private void Delta(int delta)
    {
        var prev = this.Count;
        var count = prev + delta;
        this.Count = count;
        this.countChangedPublisher.Publish((count, prev));
    }

    public void Dispose()
    {
        this.countChangedPublisher.Dispose();
    }
    public IDisposable Subscribe(IMessageHandler<TMessage> handler, params MessageHandlerFilter<TMessage>[] filters)
    {
        var disposable = this.subscriber.Subscribe(handler, filters);
        this.Delta(1);
        return new Disposable(disposable, this);
    }
}
