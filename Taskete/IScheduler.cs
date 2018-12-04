using System.Collections.Generic;
using Diese;
using OverGraphed;

namespace Taskete
{
    public interface IReadOnlyScheduler<T>
    {
        IEnumerable<T> Planning { get; }
        IGraph<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge> Graph { get; }
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