using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Srota.Examples.EventSources;
using Srota.Core.Builders;

namespace Srota.Examples
{
    class Program
{
    static async Task Main(string[] args)
    {
        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        Console.WriteLine("=== Srota Examples ===\n");

        // Example 1: Simple Polling
        await RunPollingExample(loggerFactory);

        // Example 2: Event-based Processing
        await RunEventExample(loggerFactory);

        // Example 3: Pipeline
        await RunPipelineExample(loggerFactory);

        // Example 4: Combined Worker
        await RunCombinedExample(loggerFactory);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task RunPollingExample(ILoggerFactory loggerFactory)
    {
        Console.WriteLine("--- Example 1: Polling Task ---");

        var worker = WorkerBuilder.Create()
            .UseLoggerFactory(loggerFactory)
            .OnError((ex, taskName) =>
            {
                Console.WriteLine($"Error in {taskName}: {ex.Message}");
            })
            .AddPolling("CheckDatabase", every: TimeSpan.FromSeconds(2))
                .WithRetry(maxRetries: 3, retryDelay: TimeSpan.FromSeconds(1))
                .Do(async () =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Checking database...");
                    await Task.Delay(100); // Simulate work
                })
            .Build();

        await worker.StartAsync();
        await Task.Delay(7000);
        await worker.StopAsync();
        worker.Dispose();

        Console.WriteLine();
    }

    static async Task RunEventExample(ILoggerFactory loggerFactory)
    {
        Console.WriteLine("--- Example 2: Event-based Task ---");

        var messageQueue = new MessageQueue();

        // Simulate message producer
        _ = Task.Run(async () =>
        {
            for (int i = 1; i <= 5; i++)
            {
                await Task.Delay(1000);
                messageQueue.Enqueue($"Message {i}");
            }
        });

        var worker = WorkerBuilder.Create()
            .UseLoggerFactory(loggerFactory)
            .AddEvent("MessageProcessor", () => new MessageQueueSource(messageQueue))
                .Do(async message =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Processing: {message}");
                    await Task.Delay(500); // Simulate processing
                })
            .Build();

        await worker.StartAsync();
        await Task.Delay(7000);
        await worker.StopAsync();
        worker.Dispose();

        Console.WriteLine();
    }

    static async Task RunPipelineExample(ILoggerFactory loggerFactory)
    {
        Console.WriteLine("--- Example 3: Pipeline Task ---");

        var worker = WorkerBuilder.Create()
            .UseLoggerFactory(loggerFactory)
            .AddPipeline("DataPipeline")
                .ThenDo(async () =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Step 1: Fetch data");
                    await Task.Delay(200);
                })
                .ThenDo(async () =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Step 2: Transform data");
                    await Task.Delay(200);
                })
                .ThenDo(async () =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Step 3: Save data");
                    await Task.Delay(200);
                })
                .Execute(every: TimeSpan.FromSeconds(3))
            .Build();

        await worker.StartAsync();
        await Task.Delay(8000);
        await worker.StopAsync();
        worker.Dispose();

        Console.WriteLine();
    }

    static async Task RunCombinedExample(ILoggerFactory loggerFactory)
    {
        Console.WriteLine("--- Example 4: Combined Worker ---");

        var messageQueue = new MessageQueue();

        // Simulate message producer
        _ = Task.Run(async () =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await Task.Delay(2000);
                messageQueue.Enqueue($"Alert {i}");
            }
        });

        var worker = WorkerBuilder.Create()
            .UseLoggerFactory(loggerFactory)
            .OnError((ex, taskName) =>
            {
                Console.WriteLine($"[ERROR] {taskName}: {ex.Message}");
            })
            // Health check every 3 seconds
            .AddPolling("HealthCheck", every: TimeSpan.FromSeconds(3))
                .Do(() =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Health check passed");
                })
            // Process incoming alerts
            .AddEvent("AlertProcessor", () => new MessageQueueSource(messageQueue))
                .Do(alert =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔔 {alert}");
                })
            // Cleanup pipeline every 5 seconds
            .AddPipeline("Cleanup")
                .ThenDo(() => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] → Cleanup: Check logs"))
                .ThenDo(() => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] → Cleanup: Archive old data"))
                .Execute(every: TimeSpan.FromSeconds(5))
            .Build();

        await worker.StartAsync();
        await Task.Delay(10000);
        await worker.StopAsync();
        worker.Dispose();

        Console.WriteLine();
    }
}
}