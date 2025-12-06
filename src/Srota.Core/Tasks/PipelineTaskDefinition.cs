using Srota.Core.Abstractions;

namespace Srota.Core.Tasks
{

    internal class PipelineTaskDefinition : ITaskDefinition
    {
        public string Name { get; }
        private readonly List<Func<Task>> _steps;
        private readonly TimeSpan _interval;

        public PipelineTaskDefinition(string name, List<Func<Task>> steps, TimeSpan interval)
        {
            Name = name;
            _steps = steps;
            _interval = interval;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var step in _steps)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await step();
                }

                await Task.Delay(_interval, cancellationToken);
            }
        }
    }
}
