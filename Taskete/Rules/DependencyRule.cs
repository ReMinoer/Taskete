using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Taskete.Rules.Base;

namespace Taskete.Rules
{
    public class DependencyRule<T> : SchedulingRuleBase<T>, IDisposable
    {
        private readonly INotifyCollectionChanged _predecessorsChanges;
        private readonly INotifyCollectionChanged _successorsChanges;

        public IEnumerable<T> Predecessors { get; }
        public IEnumerable<T> Successors { get; }

        public override bool IsValid => (Predecessors?.Any() ?? false) && (Successors?.Any() ?? false);

        public override event EventHandler Dirty;

        private DependencyRule(IEnumerable<T> predecessors, INotifyCollectionChanged predecessorsChanges,
            IEnumerable<T> successors, INotifyCollectionChanged successorsChanges)
        {
            Predecessors = predecessors;
            Successors = successors;
            _predecessorsChanges = predecessorsChanges;
            _successorsChanges = successorsChanges;

            if (_predecessorsChanges != null)
                _predecessorsChanges.CollectionChanged += OnCollectionChanged;
            if (_successorsChanges != null)
                _successorsChanges.CollectionChanged += OnCollectionChanged;
        }

        static public DependencyRule<T> New<TPredecessors, TSuccessors>(TPredecessors predecessors, TSuccessors successors)
            where TPredecessors : IEnumerable<T>, INotifyCollectionChanged
            where TSuccessors : IEnumerable<T>, INotifyCollectionChanged
            => new DependencyRule<T>(predecessors, predecessors, successors, successors);
        static public DependencyRule<T> New<TPredecessors>(TPredecessors predecessors, IEnumerable<T> successors)
            where TPredecessors : IEnumerable<T>, INotifyCollectionChanged
            => new DependencyRule<T>(predecessors, predecessors, successors, null);
        static public DependencyRule<T> New<TSuccessors>(IEnumerable<T> predecessors, TSuccessors successors)
            where TSuccessors : IEnumerable<T>, INotifyCollectionChanged
            => new DependencyRule<T>(predecessors, null, successors, successors);
        static public DependencyRule<T> New(IEnumerable<T> predecessors, IEnumerable<T> successors)
            => new DependencyRule<T>(predecessors, null, successors, null);

        public override void Apply(ISchedulerGraphBuilder<T> graph)
        {
            foreach (T predecessor in Predecessors)
                foreach (T successor in Successors)
                    graph.TryAddDependency(predecessor, successor, this);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dirty?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_predecessorsChanges != null)
                _predecessorsChanges.CollectionChanged -= OnCollectionChanged;
            if (_successorsChanges != null)
                _successorsChanges.CollectionChanged -= OnCollectionChanged;
        }
    }
}