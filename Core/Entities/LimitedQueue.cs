using System.Collections.Generic;

namespace Trustcoin.Core.Entities
{
    public class LimitedQueue<TElement>
    {
        private readonly Queue<TElement> _queue;

        public LimitedQueue(int limit)
        {
            Limit = limit;
            _queue = new Queue<TElement>(limit);
        }

        public int Limit { get; }

        public bool Contains(TElement element)
            => _queue.Contains(element);

        public void Enqueue(TElement element)
        {
            while (_queue.Count >= Limit)
                _queue.Dequeue();
            _queue.Enqueue(element);
        }
    }
}