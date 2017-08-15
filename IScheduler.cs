using System.Collections.Generic;
using Diese.Graph;

namespace Diese.Scheduling
{
    public interface IReadOnlyScheduler<T>
    {
        IEnumerable<T> Planning { get; }
        IGraphData<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge> GraphData { get; }
    }

    public interface IScheduler<T> : IReadOnlyScheduler<T>, IBatchTree
    {
        void Plan(T item);
        void Unplan(T item);
    }

    public interface IScheduler<out TController, T> : IScheduler<T>
    {
        new TController Plan(T item);
    }
}