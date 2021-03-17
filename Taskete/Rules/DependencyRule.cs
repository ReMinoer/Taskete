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

        public DependencyRule(IEnumerable<T> predecessors, IEnumerable<T> successors)
        {
            Predecessors = predecessors;
            Successors = successors;
            _predecessorsChanges = predecessors as INotifyCollectionChanged;
            _successorsChanges = successors as INotifyCollectionChanged;

            if (_predecessorsChanges != null)
                _predecessorsChanges.CollectionChanged += OnDirty;
            if (_successorsChanges != null)
                _successorsChanges.CollectionChanged += OnDirty;
        }

        public override void Apply(ISchedulerGraphBuilder<T> graph)
        {
            TryAddDependency(graph, Predecessors, Successors);
        }

        public void Dispose()
        {
            if (_predecessorsChanges != null)
                _predecessorsChanges.CollectionChanged -= OnDirty;
            if (_successorsChanges != null)
                _successorsChanges.CollectionChanged -= OnDirty;
        }
    }
}