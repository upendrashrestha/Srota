using Srota.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Srota.Examples.EventSources
{
    public class MessageQueueSource : IEventSource<string>
    {
        private readonly MessageQueue _queue;
        private bool _disposed;

        public MessageQueueSource(MessageQueue queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public async Task<string> ReadAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MessageQueueSource));

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var message))
                {
                    return message;
                }

                await Task.Delay(100, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            throw new OperationCanceledException();
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
