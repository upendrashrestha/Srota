namespace Srota.Core.Abstractions
{
    public interface ISrotaWorker : IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
        bool IsRunning { get; }
    }
}
