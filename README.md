# Srota

A lightweight, plug-and-play .NET library for background task execution with multiple trigger types.

## Features

- **Polling Tasks**: Execute tasks at regular intervals
- **Event-based Tasks**: React to custom event sources (queues, channels, etc.)
- **SSE Tasks**: Process Server-Sent Events streams
- **Pipeline Tasks**: Chain multiple steps together
- **Fluent API**: Intuitive builder pattern
- **Built-in Retry Logic**: Configurable retry policies
- **Comprehensive Logging**: Uses Microsoft.Extensions.Logging
- **Fully Tested**: Extensive unit test coverage

## Installation

```bash
dotnet add package Srota.Core
```

## Quick Start

```csharp
using Srota.Core;

var worker = WorkerBuilder.Create()
    .AddPolling("CheckNewData", every: TimeSpan.FromSeconds(10))
        .Do(async () => await CheckData())
    .AddEvent("OnMessage", () => new MyEventSource())
        .Do(message => HandleMessage(message))
    .Build();

await worker.StartAsync();
// ... when done
await worker.StopAsync();
```

## Examples

### Polling Task with Retry

```csharp
var worker = WorkerBuilder.Create()
    .UseLoggerFactory(loggerFactory)
    .AddPolling("ApiCheck", every: TimeSpan.FromMinutes(5))
        .WithRetry(maxRetries: 3, retryDelay: TimeSpan.FromSeconds(10))
        .Do(async () =>
        {
            var data = await FetchFromApi();
            await ProcessData(data);
        })
    .Build();
```

### Event-based Processing

```csharp
// Implement IEventSource<T>
public class MyQueueSource : IEventSource<Message>
{
    public async Task<Message> ReadAsync(CancellationToken ct)
    {
        return await _queue.DequeueAsync(ct);
    }
}

var worker = WorkerBuilder.Create()
    .AddEvent("QueueProcessor", () => new MyQueueSource())
        .Do(async msg => await ProcessMessage(msg))
    .Build();
```

### Pipeline

```csharp
var worker = WorkerBuilder.Create()
    .AddPipeline("ETL")
        .ThenDo(async () => await Extract())
        .ThenDo(async () => await Transform())
        .ThenDo(async () => await Load())
        .Execute(every: TimeSpan.FromHours(1))
    .Build();
```

### Error Handling

```csharp
var worker = WorkerBuilder.Create()
    .OnError((exception, taskName) =>
    {
        logger.LogError(exception, "Task {TaskName} failed", taskName);
        // Send alert, log to external service, etc.
    })
    .AddPolling("CriticalTask", TimeSpan.FromSeconds(30))
        .Do(() => PerformCriticalOperation())
    .Build();
```

## Advanced Usage

### Custom Event Sources

Implement `IEventSource<T>`:

```csharp
public class MyEventSource : IEventSource<MyData>
{
    public async Task<MyData> ReadAsync(CancellationToken cancellationToken)
    {
        // Your implementation
        // Wait for events, read from queue, etc.
    }

    public void Dispose()
    {
        // Cleanup
    }
}
```

### Using with Dependency Injection

```csharp
services.AddSingleton<ISrotaWorker>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var myService = sp.GetRequiredService<IMyService>();

    return WorkerBuilder.Create()
        .UseLoggerFactory(loggerFactory)
        .AddPolling("ServiceTask", TimeSpan.FromMinutes(1))
            .Do(() => myService.Execute())
        .Build();
});

// Start on application startup
public class SrotaHostedService : IHostedService
{
    private readonly ISrotaWorker _worker;

    public SrotaHostedService(ISrotaWorker worker)
    {
        _worker = worker;
    }

    public Task StartAsync(CancellationToken ct) => _worker.StartAsync(ct);
    public Task StopAsync(CancellationToken ct) => _worker.StopAsync(ct);
}
```

## License

MIT
