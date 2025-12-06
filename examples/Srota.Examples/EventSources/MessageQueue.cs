using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Srota.Examples.EventSources
{
    public class MessageQueue
    {
        private readonly ConcurrentQueue<string> _queue = new();

        public void Enqueue(string message)
        {
            _queue.Enqueue(message);
        }

        public bool TryDequeue(out string? message)
        {
            return _queue.TryDequeue(out message);
        }

        public bool IsEmpty => _queue.IsEmpty;
    }
}
