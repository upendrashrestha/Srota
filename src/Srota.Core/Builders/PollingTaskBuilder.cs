using Srota.Core.Tasks;

namespace Srota.Core.Builders
{
    public class PollingTaskBuilder
    {
        private readonly WorkerBuilder _parent;
        private readonly string _name;
        private readonly TimeSpan _interval;
        private Func<Task>? _handler;
        private int _maxRetries = 3;
        private TimeSpan _retryDelay = TimeSpan.FromSeconds(5);

        internal PollingTaskBuilder(WorkerBuilder parent, string name, TimeSpan interval)
        {
            _parent = parent;
            _name = name;
            _interval = interval;
        }

        public PollingTaskBuilder WithRetry(int maxRetries, TimeSpan retryDelay)
        {
            _maxRetries = maxRetries;
            _retryDelay = retryDelay;
            return this;
        }

        public WorkerBuilder Do(Func<Task> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _parent.RegisterTask(new PollingTaskDefinition(_name, _interval, _handler, _maxRetries, _retryDelay));
            return _parent;
        }

        public WorkerBuilder Do(Action handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return Do(() => { handler(); return Task.CompletedTask; });
        }
    }
}
