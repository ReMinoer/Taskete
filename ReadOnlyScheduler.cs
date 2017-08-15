using System.Collections.Generic;
using Diese.Graph;

namespace Diese.Scheduling
{
    public class ReadOnlyScheduler<T> : IReadOnlyScheduler<T>
    {
        private readonly IScheduler<T> _scheduler;
        public IEnumerable<T> Planning => _scheduler.Planning;
        public IGraphData<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge> GraphData => _scheduler.GraphData;

        public ReadOnlyScheduler(IScheduler<T> scheduler)
        {
            _scheduler = scheduler;
        }
    }
}