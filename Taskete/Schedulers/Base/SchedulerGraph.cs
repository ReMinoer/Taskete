﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Taskete.Schedulers.Base
{
    public class SchedulerGraph<T> : ISchedulerGraphBuilder<T>
    {
        private enum Edge
        {
            None,
            Connected,
            AlmostCycle
        }
        
        private readonly IList<T> _tasks;
        private readonly Dictionary<T, int> _taskIndices;
        public IReadOnlyCollection<T> AllTasks { get; }

        private int TaskCount => _taskIndices.Count;

        private readonly Edge[,] _edges;
        private readonly bool[,] _followingTasks;
        private readonly int[] _incomingEdgeCount;

        protected SchedulerGraph(IList<T> tasks)
        {
            _tasks = tasks;
            AllTasks = new ReadOnlyCollection<T>(tasks);

            _taskIndices = new Dictionary<T, int>();

            int i = 0;
            foreach (T task in tasks)
            {
                _taskIndices.Add(task, i);
                i++;
            }

            _edges = new Edge[TaskCount, TaskCount];
            _followingTasks = new bool[TaskCount, TaskCount];
            _incomingEdgeCount = new int[TaskCount];
        }

        public SchedulerGraph(IList<T> tasks, IEnumerable<ISchedulerRule<T>> rules)
            : this(tasks)
        {
            IEnumerable<ISchedulerRule<T>> orderedValidRules = rules.Where(x => x.IsValid).OrderByDescending(x => x.Weight);
            foreach (ISchedulerRule<T> rule in orderedValidRules)
                rule.Apply(this);
        }

        public SchedulerGraph(SchedulerGraph<T> other)
            : this(other._tasks)
        {
            Array.Copy(other._edges, _edges, _edges.Length);
            Array.Copy(other._followingTasks, _followingTasks, _followingTasks.Length);
            Array.Copy(other._incomingEdgeCount, _incomingEdgeCount, _incomingEdgeCount.Length);
        }

        public IEnumerable<int> GetPendingTasks()
        {
            for (int task = 0; task < TaskCount; task++)
                if (HasNoIncomingEdge(task))
                    yield return task;
        }

        public IEnumerable<int> GetNextTasks(int completedTask)
        {
            for (int otherTask = 0; otherTask < TaskCount; otherTask++)
            {
                if (!AreConnected(completedTask, otherTask))
                    continue;

                SetAsNotConnected(completedTask, otherTask);

                DecrementIncomingEdge(otherTask);
                if (HasNoIncomingEdge(otherTask))
                    yield return otherTask;
            }
        }

        public bool TryAddDependency(T predecessor, T successor, ISchedulerRule<T> rule)
        {
            int predecessorIndex = _taskIndices[predecessor];
            if (predecessorIndex == -1)
                return false;

            int successorIndex = _taskIndices[successor];
            if (successorIndex == -1)
                return false;

            if (predecessorIndex == successorIndex)
                throw new ArgumentException("A task cannot depends of itself.");

            if (AreConnected(predecessorIndex, successorIndex))
                return false;
            if (AreAlmostCycle(predecessorIndex, successorIndex))
            {
                if (rule.MustBeApplied)
                    throw new InvalidOperationException("Rule must be applied but cannot without creating cycle dependencies.");
                return false;
            }

            Connect(predecessorIndex, successorIndex);
            return true;
        }

        private void Connect(int predecessor, int successor)
        {
            SetAsConnected(predecessor, successor);
            IncrementIncomingEdge(successor);
            
            SetAsFollowingTask(predecessor, successor);
            if (AreNotConnected(successor, predecessor))
                SetAsAlmostCycle(successor, predecessor);

            for (int i = 0; i < TaskCount; i++)
            {
                if (!IsFollowingTask(successor, i))
                    continue;

                SetAsFollowingTask(predecessor, i);
                if (AreNotConnected(i, predecessor))
                    SetAsAlmostCycle(i, predecessor);
            }
        }

        private bool AreConnected(int predecessor, int successor) => _edges[predecessor, successor] == Edge.Connected;
        private void SetAsConnected(int predecessor, int successor) => _edges[predecessor, successor] = Edge.Connected;
        private bool AreAlmostCycle(int predecessor, int successor) => _edges[predecessor, successor] == Edge.AlmostCycle;
        private void SetAsAlmostCycle(int predecessor, int successor) => _edges[predecessor, successor] = Edge.AlmostCycle;
        private bool AreNotConnected(int predecessor, int successor) => _edges[predecessor, successor] != Edge.Connected;
        private void SetAsNotConnected(int predecessor, int successor) => _edges[predecessor, successor] = Edge.None;

        private bool IsFollowingTask(int predecessor, int successor) => _followingTasks[predecessor, successor];
        private void SetAsFollowingTask(int predecessor, int successor) => _followingTasks[predecessor, successor] = true;

        private bool HasNoIncomingEdge(int task) => _incomingEdgeCount[task] == 0;
        private void IncrementIncomingEdge(int task) => _incomingEdgeCount[task]++;
        private void DecrementIncomingEdge(int task) => _incomingEdgeCount[task]--;
    }
}