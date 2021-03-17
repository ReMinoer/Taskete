using System.Collections.Generic;
using Taskete.Schedulers.Base;

namespace Taskete.Schedulers
{
    public class LinearScheduler<T> : SchedulerBase<T>
    {
        private readonly List<T> _schedule = new List<T>();
        public IEnumerable<T> Schedule
        {
            get
            {
                if (IsDirty)
                    UpdateSchedule();
                return _schedule;
            }
        }

        private void UpdateSchedule()
        {
            _schedule.Clear();
            _schedule.Capacity = Tasks.Count;

            var graph = new SchedulerGraph<T>(Tasks, Rules);

            var pendingTasks = new Queue<int>(graph.GetPendingTasks());
            while (pendingTasks.Count > 0)
            {
                int currentTask = pendingTasks.Dequeue();

                _schedule.Add(Tasks[currentTask]);

                foreach (int nextTask in graph.GetNextTasks(currentTask))
                    pendingTasks.Enqueue(nextTask);
            }

            IsDirty = false;
        }
    }
}