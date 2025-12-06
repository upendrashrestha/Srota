using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;
using Srota.Core.Abstractions;

namespace Srota.Examples.EventSources
{
    public class ChannelEventSource<T> : IEventSource<T>
    {
        private readonly ChannelReader<T> _reader;
        private bool _disposed;

        public ChannelEventSource(ChannelReader<T> reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<T> ReadAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ChannelEventSource<T>));

            return await _reader.ReadAsync(cancellationToken);
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
