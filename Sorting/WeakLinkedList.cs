using System.Collections;

namespace Sorting.Collections;

public class WeakLinkedList<THandler> : IEnumerable<THandler>
    where THandler : class
{
    public struct Enumerator : IEnumerator<THandler>
    {
        Node? node;

        public THandler Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            ref var head = ref this.node;
            if (head is null) return false;

            do
            {
                if (head.TryGet(out var handler))
                {
                    this.node = head.Next;
                    this.Current = handler!;
                    return true;
                }
                else
                {
                    head = head.Next;
                    if (head is null) return false;
                }
                head = ref head.Next;
            }
            while (head is not null);
            return false;
        }

        public void Reset() => throw new NotSupportedException();

        public Enumerator(WeakLinkedList<THandler> list)
        {
            this.node = list.head;
            this.Current = default!;
        }
    }

    class Node
    {
        public WeakReference<THandler> HandlerReference;
        public Node? Next;

        public bool TryGet(out THandler? handler) => this.HandlerReference.TryGetTarget(out handler);

        public Node(THandler handler)
        {
            this.HandlerReference = new(handler);
        }
    }

    Node? head;

    public void Add(THandler handler)
    {
        ref var head = ref this.head;
        while (head is not null)
        {
            if (!head.TryGet(out var h))
            {
                head = head.Next;
                if (head is null) break;
            }
            head = ref head.Next;
        }

        head = new Node(handler);
    }


    public void Remove(THandler handler)
    {
        ref var head = ref this.head;
        while (head is not null)
        {
            if (!head.TryGet(out var h) || h == handler)
            {
                head = head.Next;
                if (head is null) break;
            }
            head = ref head.Next;
        }
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<THandler> IEnumerable<THandler>.GetEnumerator() => this.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
