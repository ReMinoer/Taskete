using System;

namespace Taskete.Rules.Base
{
    public abstract class SchedulingRuleBase<T> : ISchedulerRule<T>
    {
        public float Weight { get; set; }
        public bool MustBeApplied { get; set; }
        public abstract event EventHandler Dirty;
        public abstract void Apply(ISchedulerGraphBuilder<T> graph);
    }
}