using Srota.Core.Builders;

namespace Srota.Tests.Unit
{
    public class PollingTaskTests
    {
        [Fact]
        public async Task PollingTask_ExecutesAtSpecifiedInterval()
        {
            // Arrange
            var executionCount = 0;
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("TestTask", TimeSpan.FromMilliseconds(50))
                    .Do(() => { executionCount++; })
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(250);
            await worker.StopAsync();

            // Assert
            Assert.True(executionCount >= 3, $"Expected at least 3 executions, got {executionCount}");
        }

        [Fact]
        public async Task PollingTask_SupportsAsyncHandlers()
        {
            // Arrange
            var values = new List<int>();
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("AsyncTask", TimeSpan.FromMilliseconds(50))
                    .Do(async () =>
                    {
                        await Task.Delay(10);
                        values.Add(values.Count + 1);
                    })
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(250);
            await worker.StopAsync();

            // Assert
            Assert.True(values.Count >= 3);
            Assert.Equal(1, values[0]);
            Assert.Equal(2, values[1]);
        }

        [Fact]
        public async Task PollingTask_RetriesOnFailure()
        {
            // Arrange
            var attemptCount = 0;
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("RetryTask", TimeSpan.FromMilliseconds(500))
                    .WithRetry(maxRetries: 2, retryDelay: TimeSpan.FromMilliseconds(10))
                    .Do(() =>
                    {
                        attemptCount++;
                        if (attemptCount < 3)
                            throw new InvalidOperationException("Simulated failure");
                    })
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(200);
            await worker.StopAsync();

            // Assert
            Assert.Equal(3, attemptCount); // Initial + 2 retries
        }

        [Fact]
        public async Task MultiplePollingTasks_ExecuteConcurrently()
        {
            // Arrange
            var task1Count = 0;
            var task2Count = 0;

            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("Task1", TimeSpan.FromMilliseconds(50))
                    .Do(() => { task1Count++; })
                .AddPolling("Task2", TimeSpan.FromMilliseconds(75))
                    .Do(() => { task2Count++; })
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(300);
            await worker.StopAsync();

            // Assert
            Assert.True(task1Count >= 4);
            Assert.True(task2Count >= 3);
        }
    }
}
