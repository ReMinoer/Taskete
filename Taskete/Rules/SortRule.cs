using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Taskete.Rules.Base;

namespace Taskete.Rules
{
    public class SortRule<T, TKey> : SchedulingRuleBase<T>, IDisposable
    {
        private readonly INotifyCollectionChanged _tasksChanges;
        private readonly Action<T, EventHandler> _keyChangeSubscriber;
        private readonly Action<T, EventHandler> _keyChangeUnsubscriber;

        public IEnumerable<T> Tasks { get; }
        public Func<T, TKey> KeySelector { get; }
        public IComparer<TKey> KeyComparer { get; }

        public override bool IsValid => Tasks?.Any() ?? false;

        public SortRule(IEnumerable<T> tasks, Func<T, TKey> keySelector, IComparer<TKey> keyComparer,
            Action<T, EventHandler> keyChangeSubscriber = null, Action<T, EventHandler> keyChangeUnsubscriber = null)
        {
            Tasks = tasks;
            _tasksChanges = tasks as INotifyCollectionChanged;

            if (_tasksChanges != null)
                _tasksChanges.CollectionChanged += OnTasksCollectionChanged;

            KeySelector = keySelector;
            KeyComparer = keyComparer;

            _keyChangeSubscriber = keyChangeSubscriber;
            _keyChangeUnsubscriber = keyChangeUnsubscriber;
        }

        public void Dispose()
        {
            if (_tasksChanges != null)
                _tasksChanges.CollectionChanged -= OnTasksCollectionChanged;
        }

        public override void Apply(ISchedulerGraphBuilder<T> graph)
        {
            IOrderedEnumerable<IGrouping<TKey, T>> groups = (Tasks ?? graph.AllTasks).GroupBy(KeySelector).OrderBy(x => x.Key, KeyComparer);
            using (IEnumerator<IGrouping<TKey, T>> enumerator = groups.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                IEnumerable<T> previousGroup = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    IEnumerable<T> currentGroup = enumerator.Current;
                    TryAddDependency(graph, previousGroup, currentGroup);
                    previousGroup = currentGroup;
                }
            }
        }

        private void OnTasksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDirty();

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (T task in Tasks)
                    _keyChangeUnsubscriber?.Invoke(task, OnDirty);
                return;
            }

            if (e.OldItems != null)
                foreach (T task in e.OldItems)
                    _keyChangeUnsubscriber?.Invoke(task, OnDirty);

            if (e.NewItems != null)
                foreach (T task in e.NewItems)
                    _keyChangeSubscriber?.Invoke(task, OnDirty);
        }
    }
}