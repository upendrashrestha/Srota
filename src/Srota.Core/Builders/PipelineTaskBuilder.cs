using Srota.Core.Tasks;

namespace Srota.Core.Builders
{
    public class PipelineTaskBuilder
    {
        private readonly WorkerBuilder _parent;
        private readonly string _name;
        private readonly List<Func<Task>> _steps = new();

        internal PipelineTaskBuilder(WorkerBuilder parent, string name)
        {
            _parent = parent;
            _name = name;
        }

        public PipelineTaskBuilder ThenDo(Func<Task> handler)
        {
            _steps.Add(handler ?? throw new ArgumentNullException(nameof(handler)));
            return this;
        }

        public PipelineTaskBuilder ThenDo(Action handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return ThenDo(() => { handler(); return Task.CompletedTask; });
        }

        public WorkerBuilder Execute(TimeSpan every)
        {
            _parent.RegisterTask(new PipelineTaskDefinition(_name, _steps, every));
            return _parent;
        }
    }
}
