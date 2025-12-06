using Srota.Core.Builders;

namespace Srota.Tests.Unit
{
    public class WorkerLifecycleTests
    {
        [Fact]
        public async Task Worker_CanStartAndStop()
        {
            // Arrange
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("Test", TimeSpan.FromSeconds(1))
                    .Do(() => { })
                .Build();

            // Act & Assert
            Assert.False(worker.IsRunning);

            await worker.StartAsync();
            Assert.True(worker.IsRunning);

            await worker.StopAsync();
            Assert.False(worker.IsRunning);
        }

        [Fact]
        public async Task Worker_ThrowsWhenStartedTwice()
        {
            // Arrange
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("Test", TimeSpan.FromSeconds(1))
                    .Do(() => { })
                .Build();

            await worker.StartAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => worker.StartAsync());

            await worker.StopAsync();
        }

        [Fact]
        public async Task Worker_GlobalErrorHandler_CatchesExceptions()
        {
            // Arrange
            Exception? caughtException = null;
            string? caughtTaskName = null;
            var errorHandled = new TaskCompletionSource<bool>();

            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .OnError((ex, taskName) =>
                {
                    caughtException = ex;
                    caughtTaskName = taskName;
                    errorHandled.TrySetResult(true);
                })
                .AddPolling("FailingTask", TimeSpan.FromMilliseconds(50))
                    .WithRetry(maxRetries: 0, retryDelay: TimeSpan.Zero) // No retries
                    .Do(() => throw new InvalidOperationException("Test error"))
                .Build();

            // Act
            await worker.StartAsync();

            // Wait for error handler to be called (with timeout)
            var completedTask = await Task.WhenAny(
                errorHandled.Task,
                Task.Delay(1000)
            );

            await worker.StopAsync();

            // Assert
            Assert.True(completedTask == errorHandled.Task, "Error handler was not called within timeout");
            Assert.NotNull(caughtException);
            Assert.IsType<InvalidOperationException>(caughtException);
            Assert.Equal("FailingTask", caughtTaskName);
            Assert.Equal("Test error", caughtException?.Message); 
        }

        [Fact]
        public void Worker_DisposesCleanly()
        {
            // Arrange
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("Test", TimeSpan.FromSeconds(1))
                    .Do(() => { })
                .Build();

            // Act & Assert (should not throw)
            worker.Dispose();
        }
    }
}
