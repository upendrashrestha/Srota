using Srota.Core.Abstractions;
using Srota.Core.Models;

namespace Srota.Core.Tasks
{
    internal class SseTaskDefinition : ITaskDefinition
    {
        public string Name { get; }
        private readonly string _url;
        private readonly Func<SseEvent, Task> _handler;

        public SseTaskDefinition(string name, string url, Func<SseEvent, Task> handler)
        {
            Name = name;
            _url = url;
            _handler = handler;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var client = new System.Net.Http.HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            using var response = await client.GetAsync(_url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            string? line;
            var currentEvent = new SseEvent();

            while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!string.IsNullOrEmpty(currentEvent.Data))
                    {
                        await _handler(currentEvent);
                    }
                    currentEvent = new SseEvent();
                    continue;
                }

                if (line.StartsWith("data:"))
                    currentEvent.Data = line.Substring(5).Trim();
                else if (line.StartsWith("event:"))
                    currentEvent.EventType = line.Substring(6).Trim();
                else if (line.StartsWith("id:"))
                    currentEvent.Id = line.Substring(3).Trim();
            }
        }
    }

}
