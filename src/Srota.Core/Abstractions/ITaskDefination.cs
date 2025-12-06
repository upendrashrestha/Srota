namespace Srota.Core.Abstractions
{
    internal interface ITaskDefinition
    {
        string Name { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
