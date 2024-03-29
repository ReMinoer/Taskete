﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Taskete.Schedulers.Base;

namespace Taskete.Schedulers
{
    public class AsyncScheduler<T, TParam> : SchedulerBase<T>
    {
        private readonly Func<T, TParam, CancellationToken, Task> _awaitableSelector;
        private readonly Action<T, TParam, CancellationToken> _actionInvoker;
        private SchedulerGraph<T> _graph;

        public AsyncScheduler(Func<T, TParam, CancellationToken, Task> awaitableSelector, Action<T, TParam, CancellationToken> actionInvoker)
        {
            _awaitableSelector = awaitableSelector;
            _actionInvoker = actionInvoker;
        }

        public void RunSchedule(TParam param) => RunSchedule(param, CancellationToken.None);
        public void RunSchedule(TParam param, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsDirty)
                _graph = new SchedulerGraph<T>(Tasks, Rules);

            cancellationToken.ThrowIfCancellationRequested();

            var graphCopy = new SchedulerGraph<T>(_graph);
            var graphLock = new SemaphoreSlim(1);

            foreach (int pendingTask in graphCopy.GetPendingTasks())
                Run(pendingTask, param, graphCopy, graphLock, cancellationToken);
        }

        public Task RunScheduleAsync(TParam param) => RunScheduleAsync(param, CancellationToken.None);
        public async Task RunScheduleAsync(TParam param, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsDirty)
                _graph = new SchedulerGraph<T>(Tasks, Rules);

            cancellationToken.ThrowIfCancellationRequested();

            var graphCopy = new SchedulerGraph<T>(_graph);
            var graphLock = new SemaphoreSlim(1);

            await Task.WhenAll(graphCopy.GetPendingTasks().Select(x => RunTask(x, param, graphCopy, graphLock, cancellationToken)));
        }

        private void Run(int currentTask, TParam param, SchedulerGraph<T> graph, SemaphoreSlim graphLock, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _actionInvoker(Tasks[currentTask], param, cancellationToken);

            int[] nextTasks;
            graphLock.Wait(cancellationToken);
            try
            {
                nextTasks = graph.GetNextTasks(currentTask).ToArray();
            }
            finally
            {
                graphLock.Release();
            }

            foreach (int nextTask in nextTasks)
                Run(nextTask, param, graph, graphLock, cancellationToken);
        }

        private async Task RunTask(int currentTask, TParam param, SchedulerGraph<T> graph, SemaphoreSlim graphLock, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _awaitableSelector(Tasks[currentTask], param, cancellationToken);

            int[] nextTasks;
            await graphLock.WaitAsync(cancellationToken);
            try
            {
                nextTasks = graph.GetNextTasks(currentTask).ToArray();
            }
            finally
            {
                graphLock.Release();
            }

            await Task.WhenAll(nextTasks.Select(x => RunTask(x, param, graph, graphLock, cancellationToken)));
        }
    }
}