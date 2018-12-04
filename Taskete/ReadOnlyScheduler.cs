using System.Collections.Generic;
using OverGraphed;

namespace Taskete
{
    public class ReadOnlyScheduler<T> : IReadOnlyScheduler<T>
    {
        private readonly IScheduler<T> _scheduler;
        public IEnumerable<T> Planning => _scheduler.Planning;
        public IGraph<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge> Graph => _scheduler.Graph;

        public ReadOnlyScheduler(IScheduler<T> scheduler)
        {
            _scheduler = scheduler;
        }
    }
}