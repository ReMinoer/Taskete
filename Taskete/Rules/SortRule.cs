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

        public IEnumerable<T> Tasks { get; }
        public Func<T, TKey> KeySelector { get; }
        public IComparer<TKey> KeyComparer { get; }

        public override bool IsValid => Tasks?.Any() ?? false;

        public override event EventHandler Dirty;

        public SortRule(IEnumerable<T> tasks, INotifyCollectionChanged tasksChanges, Func<T, TKey> keySelector, IComparer<TKey> keyComparer)
        {
            Tasks = tasks;
            _tasksChanges = tasksChanges;

            if (_tasksChanges != null)
                _tasksChanges.CollectionChanged += OnCollectionChanged;

            KeySelector = keySelector;
            KeyComparer = keyComparer;
        }

        static public SortRule<T, TKey> New<TTasks>(TTasks tasks, Func<T, TKey> keySelector, IComparer<TKey> keyComparer)
            where TTasks : IEnumerable<T>, INotifyCollectionChanged
            => new SortRule<T, TKey>(tasks, tasks, keySelector, keyComparer);
        static public SortRule<T, TKey> New(IEnumerable<T> tasks, Func<T, TKey> keySelector, IComparer<TKey> keyComparer)
            => new SortRule<T, TKey>(tasks, null, keySelector, keyComparer);

        public override void Apply(ISchedulerGraphBuilder<T> graph)
        {
            IOrderedEnumerable<IGrouping<TKey, T>> groups = Tasks.GroupBy(KeySelector).OrderBy(x => x.Key, KeyComparer);
            using (IEnumerator<IGrouping<TKey, T>> enumerator = groups.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                IEnumerable<T> previousGroup = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    IEnumerable<T> currentGroup = enumerator.Current;

                    foreach (T previous in previousGroup)
                        foreach (T current in currentGroup)
                            graph.TryAddDependency(previous, current, this);

                    previousGroup = currentGroup;
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dirty?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_tasksChanges != null)
                _tasksChanges.CollectionChanged -= OnCollectionChanged;
        }
    }
}