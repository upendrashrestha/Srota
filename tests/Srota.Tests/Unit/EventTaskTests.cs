using Srota.Core.Abstractions;
using Srota.Core.Builders;

namespace Srota.Tests.Unit
{
    public class EventTaskTests
    {
        private class TestEventSource : IEventSource<string>
        {
            private readonly Queue<string> _events;
            private bool _disposed;

            public TestEventSource(params string[] events)
            {
                _events = new Queue<string>(events);
            }

            public async Task<string> ReadAsync(CancellationToken cancellationToken)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(TestEventSource));

                while (_events.Count == 0 && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(10, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                return _events.Dequeue();
            }

            public void Dispose()
            {
                _disposed = true;
            }
        }

        [Fact]
        public async Task EventTask_ProcessesEvents()
        {
            // Arrange
            var processedEvents = new List<string>();
            var eventSource = new TestEventSource("Event1", "Event2", "Event3");

            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddEvent("EventTask", () => eventSource)
                    .Do(evt => processedEvents.Add(evt))
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(200);
            await worker.StopAsync();

            // Assert
            Assert.Contains("Event1", processedEvents);
            Assert.Contains("Event2", processedEvents);
            Assert.Contains("Event3", processedEvents);
        }

        [Fact]
        public async Task EventTask_SupportsAsyncHandlers()
        {
            // Arrange
            var processedEvents = new List<string>();
            var eventSource = new TestEventSource("A", "B");

            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddEvent("AsyncEventTask", () => eventSource)
                    .Do(async evt =>
                    {
                        await Task.Delay(10);
                        processedEvents.Add(evt);
                    })
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(200);
            await worker.StopAsync();

            // Assert
            Assert.Equal(2, processedEvents.Count);
        }
    }
}
