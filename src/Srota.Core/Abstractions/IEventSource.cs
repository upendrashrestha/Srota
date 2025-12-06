namespace Srota.Core.Abstractions
{
    public interface IEventSource<T> : IDisposable
    {
        Task<T> ReadAsync(CancellationToken cancellationToken);
    }
}
