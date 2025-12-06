using Srota.Core.Abstractions;
using Srota.Core.Tasks;

namespace Srota.Core.Builders
{
    public class EventTaskBuilder<T>
    {
        private readonly WorkerBuilder _parent;
        private readonly string _name;
        private readonly Func<IEventSource<T>> _sourceFactory;

        internal EventTaskBuilder(WorkerBuilder parent, string name, Func<IEventSource<T>> sourceFactory)
        {
            _parent = parent;
            _name = name;
            _sourceFactory = sourceFactory;
        }

        public WorkerBuilder Do(Func<T, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _parent.RegisterTask(new EventTaskDefinition<T>(_name, _sourceFactory, handler));
            return _parent;
        }

        public WorkerBuilder Do(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return Do(item => { handler(item); return Task.CompletedTask; });
        }
    }
}
