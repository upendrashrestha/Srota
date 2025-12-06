using Srota.Core.Builders;

namespace Srota.Tests.Unit
{
    public class PipelineTaskTests
    {
        [Fact]
        public async Task PipelineTaskExecutesStepsInOrder()
        {
            // Arrange
            var executionOrder = new List<int>();

            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPipeline("Pipeline")
                    .ThenDo(() => executionOrder.Add(1))
                    .ThenDo(() => executionOrder.Add(2))
                    .ThenDo(() => executionOrder.Add(3))
                    .Execute(TimeSpan.FromMilliseconds(100))
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(350);
            await worker.StopAsync();

            // Assert
            Assert.True(executionOrder.Count >= 9); // At least 3 full cycles
            Assert.Equal(1, executionOrder[0]);
            Assert.Equal(2, executionOrder[1]);
            Assert.Equal(3, executionOrder[2]);
            Assert.Equal(1, executionOrder[3]);
        }

        [Fact]
        public async Task PipelineTaskSupportsAsyncSteps()
        {
            // Arrange
            var values = new List<string>();

            var worker = WorkerBuilder.Create()
                .UseLoggerFactory(NullLoggerFactory.Instance)
                .AddPipeline("AsyncPipeline")
                    .ThenDo(async () =>
                    {
                        await Task.Delay(10);
                        values.Add("Step1");
                    })
                    .ThenDo(async () =>
                    {
                        await Task.Delay(10);
                        values.Add("Step2");
                    })
                    .Execute(TimeSpan.FromMilliseconds(100))
                .Build();

            // Act
            await worker.StartAsync();
            await Task.Delay(250);
            await worker.StopAsync();

            // Assert
            Assert.True(values.Count >= 4);
            Assert.Equal("Step1", values[0]);
            Assert.Equal("Step2", values[1]);
        }
    }
}
