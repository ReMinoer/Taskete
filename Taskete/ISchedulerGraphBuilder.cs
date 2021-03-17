using System.Collections.Generic;

namespace Taskete
{
    public interface ISchedulerGraphBuilder<T>
    {
        IReadOnlyCollection<T> AllTasks { get; }
        bool TryAddDependency(T predecessor, T successor, ISchedulerRule<T> rule);
    }
}