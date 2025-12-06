using Srota.Core.Models;
using Srota.Core.Tasks;

namespace Srota.Core.Builders
{
    public class SseTaskBuilder
    {
        private readonly WorkerBuilder _parent;
        private readonly string _name;
        private readonly string _url;

        internal SseTaskBuilder(WorkerBuilder parent, string name, string url)
        {
            _parent = parent;
            _name = name;
            _url = url;
        }

        public WorkerBuilder Do(Func<SseEvent, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _parent.RegisterTask(new SseTaskDefinition(_name, _url, handler));
            return _parent;
        }
    }
}
