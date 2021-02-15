namespace Taskete
{
    public interface ISchedulerRule<T> : INotifyDirty
    {
        float Weight { get; }
        bool MustBeApplied { get; }
        bool IsValid { get; }
        void Apply(ISchedulerGraphBuilder<T> graph);
    }
}