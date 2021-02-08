using System.Collections.Generic;

namespace Taskete
{
    public interface IScheduler<T>
    {
        ICollection<T> Tasks { get; }
        ICollection<ISchedulerRule<T>> Rules { get; }
    }
}