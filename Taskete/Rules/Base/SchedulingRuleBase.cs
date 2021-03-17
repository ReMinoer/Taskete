using System;
using System.Collections.Generic;

namespace Taskete.Rules.Base
{
    public abstract class SchedulingRuleBase<T> : ISchedulerRule<T>
    {
        public float Weight { get; set; }
        public bool MustBeApplied { get; set; }
        public abstract bool IsValid { get; }
        public event EventHandler Dirty;
        public abstract void Apply(ISchedulerGraphBuilder<T> graph);

        protected void TryAddDependency(ISchedulerGraphBuilder<T> graph, IEnumerable<T> predecessors, IEnumerable<T> successors)
        {
            foreach (T predecessor in predecessors)
                foreach (T successor in successors)
                    graph.TryAddDependency(predecessor, successor, this);
        }

        protected void OnDirty(object sender, EventArgs e) => OnDirty();
        protected void OnDirty()
        {
            Dirty?.Invoke(this, EventArgs.Empty);
        }
    }
}