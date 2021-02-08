using System;
using System.Collections;
using System.Collections.Generic;

namespace Taskete.Schedulers.Base
{
    public abstract class SchedulerBase<T> : IScheduler<T>
    {
        public bool IsDirty { get; protected set; } = true;

        public IList<T> Tasks { get; }
        public IList<ISchedulerRule<T>> Rules { get; }

        ICollection<T> IScheduler<T>.Tasks => Tasks;
        ICollection<ISchedulerRule<T>> IScheduler<T>.Rules => Rules;

        public SchedulerBase()
        {
            Tasks = new SchedulerList<T>(this);
            Rules = new DirtySchedulerList<ISchedulerRule<T>>(this);
        }

        private class SchedulerList<TItem> : IList<TItem>
        {
            protected readonly SchedulerBase<T> Scheduler;
            protected readonly List<TItem> List = new List<TItem>();

            public int Count => List.Count;

            public SchedulerList(SchedulerBase<T> scheduler)
            {
                Scheduler = scheduler;
            }

            public TItem this[int index]
            {
                get => List[index];
                set
                {
                    OnRemovingItem(this[index]);
                    List[index] = value;
                    OnAddedItem(value);
                }
            }

            public void Add(TItem item)
            {
                List.Add(item);
                OnAddedItem(item);
            }

            public virtual void Insert(int index, TItem item)
            {
                List.Insert(index, item);
                OnAddedItem(item);
            }

            public bool Remove(TItem item)
            {
                if (!List.Remove(item))
                    return false;

                OnRemovingItem(item);
                return true;
            }

            public void RemoveAt(int index)
            {
                OnRemovingItem(List[index]);
                List.RemoveAt(index);
            }

            public void Clear()
            {
                foreach (TItem item in List)
                    OnRemovingItem(item);
                List.Clear();
            }

            protected virtual void OnAddedItem(TItem item) => Scheduler.IsDirty = true;
            protected virtual void OnRemovingItem(TItem item) => Scheduler.IsDirty = true;

            public bool Contains(TItem item) => List.Contains(item);
            public int IndexOf(TItem item) => List.IndexOf(item);

            public IEnumerator<TItem> GetEnumerator() => List.GetEnumerator();

            bool ICollection<TItem>.IsReadOnly => false;
            void ICollection<TItem>.CopyTo(TItem[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class DirtySchedulerList<TItem> : SchedulerList<TItem>
            where TItem : INotifyDirty
        {
            public DirtySchedulerList(SchedulerBase<T> scheduler)
                : base(scheduler) { }

            protected override void OnAddedItem(TItem item)
            {
                base.OnAddedItem(item);
                item.Dirty += OnDirtyItem;
            }

            protected override void OnRemovingItem(TItem item)
            {
                item.Dirty -= OnDirtyItem;
                base.OnRemovingItem(item);
            }

            private void OnDirtyItem(object sender, EventArgs e)
            {
                Scheduler.IsDirty = true;
            }
        }
    }
}