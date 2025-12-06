using Srota.Core.Abstractions;

namespace Srota.Core.Tasks
{
    internal class PollingTaskDefinition : ITaskDefinition
    {
        public string Name { get; }
        private readonly TimeSpan _interval;
        private readonly Func<Task> _handler;
        private readonly int _maxRetries;
        private readonly TimeSpan _retryDelay;

        public PollingTaskDefinition(string name, TimeSpan interval, Func<Task> handler, int maxRetries, TimeSpan retryDelay)
        {
            Name = name;
            _interval = interval;
            _handler = handler;
            _maxRetries = maxRetries;
            _retryDelay = retryDelay;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var retries = 0;
                Exception? lastException = null;
                while (retries <= _maxRetries)
                {
                    try
                    {
                        await _handler();
                        break;
                    }
                    catch (Exception ex) when (retries < _maxRetries)
                    {
                        lastException = ex;
                        retries++;
                        await Task.Delay(_retryDelay, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // All retries exhausted, propagate the exception
                        lastException = ex;
                        throw;
                    }
                }

                await Task.Delay(_interval, cancellationToken);
            }
        }
    }

}
