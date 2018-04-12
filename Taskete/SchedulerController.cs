using Taskete.Controllers;

namespace Taskete
{
    public interface ISchedulerController<T> : IRelativeController<ISchedulerController<T>, T>, IPriorityController<ISchedulerController<T>>
    {
    }

    public class SchedulerController<T> : ISchedulerController<T>
    {
        private readonly SchedulerBase<ISchedulerController<T>, T> _scheduler;
        private readonly SchedulerGraph<T>.Vertex _vertex;

        public SchedulerController(SchedulerBase<ISchedulerController<T>, T> scheduler, SchedulerGraph<T>.Vertex vertex)
        {
            _scheduler = scheduler;
            _vertex = vertex;
        }

        public ISchedulerController<T> After(T item)
        {
            new RelativeController<ISchedulerController<T>, T>(_scheduler, _vertex).After(item);
            return this;
        }

        public ISchedulerController<T> Before(T item)
        {
            new RelativeController<ISchedulerController<T>, T>(_scheduler, _vertex).Before(item);
            return this;
        }

        public ISchedulerController<T> AtStart()
        {
            new PriorityController<ISchedulerController<T>, T>(_scheduler, _vertex).AtStart();
            return this;
        }

        public ISchedulerController<T> AtEnd()
        {
            new PriorityController<ISchedulerController<T>, T>(_scheduler, _vertex).AtEnd();
            return this;
        }

        void IRelativeController<T>.After(T item) => After(item);
        void IRelativeController<T>.Before(T item) => Before(item);
        void IPriorityController.AtStart() => AtStart();
        void IPriorityController.AtEnd() => AtEnd();
    }
}