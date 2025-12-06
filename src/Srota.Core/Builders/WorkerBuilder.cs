using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Srota.Core.Abstractions;

namespace Srota.Core.Builders
{
    public class WorkerBuilder
    {
        private readonly List<ITaskDefinition> _tasks = new();
        private ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        private Action<Exception, string>? _globalErrorHandler;

        private WorkerBuilder() { }

        public static WorkerBuilder Create() => new();

        public WorkerBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            return this;
        }

        public WorkerBuilder OnError(Action<Exception, string> handler)
        {
            _globalErrorHandler = handler;
            return this;
        }

        public PollingTaskBuilder AddPolling(string name, TimeSpan every)
        {
            return new PollingTaskBuilder(this, name, every);
        }

        public EventTaskBuilder<T> AddEvent<T>(string name, Func<IEventSource<T>> sourceFactory)
        {
            return new EventTaskBuilder<T>(this, name, sourceFactory);
        }

        public SseTaskBuilder AddSse(string name, string url)
        {
            return new SseTaskBuilder(this, name, url);
        }

        public PipelineTaskBuilder AddPipeline(string name)
        {
            return new PipelineTaskBuilder(this, name);
        }

        internal void RegisterTask(ITaskDefinition task)
        {
            _tasks.Add(task);
        }

        public ISrotaWorker Build()
        {
            return new SrotaWorker(_tasks, _loggerFactory, _globalErrorHandler);
        }
    }
}
