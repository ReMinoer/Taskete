namespace Taskete
{
    public interface ISchedulerGraphBuilder<T>
    {
        bool TryAddDependency(T predecessor, T successor, ISchedulerRule<T> rule);
    }
}