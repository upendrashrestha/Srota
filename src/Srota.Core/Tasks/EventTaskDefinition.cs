using Srota.Core.Abstractions;

namespace Srota.Core.Tasks
{
    internal class EventTaskDefinition<T> : ITaskDefinition
    {
        public string Name { get; }
        private readonly Func<IEventSource<T>> _sourceFactory;
        private readonly Func<T, Task> _handler;

        public EventTaskDefinition(string name, Func<IEventSource<T>> sourceFactory, Func<T, Task> handler)
        {
            Name = name;
            _sourceFactory = sourceFactory;
            _handler = handler;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var source = _sourceFactory();
            while (!cancellationToken.IsCancellationRequested)
            {
                var item = await source.ReadAsync(cancellationToken);
                await _handler(item);
            }
        }
    }

}
