using Srota.Core.Builders;

namespace Srota.Tests.Unit
{
    public class BuilderApiTests
    {
        [Fact]
        public void Builder_CanChainMultipleTasks()
        {
            // Arrange & Act
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("Polling", TimeSpan.FromSeconds(1))
                    .Do(() => { })
                .AddPipeline("Pipeline")
                    .ThenDo(() => { })
                    .Execute(TimeSpan.FromSeconds(5))
                .Build();

            // Assert
            Assert.NotNull(worker);
        }

        [Fact]
        public void Builder_ThrowsOnNullHandler()
        {
            // Arrange
            var builder = WorkerBuilder.Create();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddPolling("Test", TimeSpan.FromSeconds(1))
                    .Do((Action)null!));
        }

        [Fact]
        public void Builder_AcceptsBothSyncAndAsyncHandlers()
        {
            // Arrange & Act
            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPolling("Sync", TimeSpan.FromSeconds(1))
                    .Do(() => { })
                .AddPolling("Async", TimeSpan.FromSeconds(1))
                    .Do(async () => await Task.Delay(1))
                .Build();

            // Assert
            Assert.NotNull(worker);
        }
    }
}
