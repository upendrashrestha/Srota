using Microsoft.Extensions.Logging;
using Srota.Core.Abstractions;

namespace Srota.Core
{
    internal class SrotaWorker : ISrotaWorker
    {
        private readonly List<ITaskDefinition> _tasks;
        private readonly ILogger<SrotaWorker> _logger;
        private readonly Action<Exception, string>? _globalErrorHandler;
        private CancellationTokenSource? _cts;
        private Task? _runningTask;

        public bool IsRunning { get; private set; }

        public SrotaWorker(
            List<ITaskDefinition> tasks,
            ILoggerFactory loggerFactory,
            Action<Exception, string>? globalErrorHandler)
        {
            _tasks = tasks;
            _logger = loggerFactory.CreateLogger<SrotaWorker>();
            _globalErrorHandler = globalErrorHandler;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
                throw new InvalidOperationException("Worker is already running");

            _logger.LogInformation("Starting Srota worker with {TaskCount} tasks", _tasks.Count);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            IsRunning = true;

            var taskExecutions = _tasks.Select(task => ExecuteTaskWithLogging(task, _cts.Token));
            _runningTask = Task.WhenAll(taskExecutions);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRunning) return;

            _logger.LogInformation("Stopping Srota worker");

            _cts?.Cancel();

            if (_runningTask != null)
            {
                try
                {
                    await _runningTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected
                }
            }

            IsRunning = false;
            _logger.LogInformation("Srota worker stopped");
        }

        private async Task ExecuteTaskWithLogging(ITaskDefinition task, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting task: {TaskName}", task.Name);

            try
            {
                await task.ExecuteAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Task cancelled: {TaskName}", task.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in task: {TaskName}", task.Name);
                _globalErrorHandler?.Invoke(ex, task.Name);
            }
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }
    }
}
