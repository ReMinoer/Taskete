using System;
using System.Collections.Generic;
using System.Linq;
using Diese;
using OverGraphed;

namespace Taskete
{
    public abstract class SchedulerBase<TController, T> : BatchTree<QueueBatchNode<T>>, IScheduler<TController, T>
    {
        protected internal readonly IDictionary<T, SchedulerGraph<T>.Vertex> ItemsVertex;
        protected internal readonly SchedulerGraph<T> SchedulerGraph;
        private readonly TopologicalOrderVisitor<T> _topologicalOrderVisitor;
        public IEnumerable<T> Items => SchedulerGraph.Vertices.SelectMany(x => x.Items);

        public IEnumerable<T> Planning
        {
            get
            {
                if (IsBatching)
                    throw new InvalidOperationException(
                        $"Dependency graph is currently in batch mode ! Call {nameof(EndBatch)}() to finish.");

                return _topologicalOrderVisitor.Result;
            }
        }

        public IGraphData<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge> GraphData { get; }

        protected SchedulerBase()
        {
            ItemsVertex = new Dictionary<T, SchedulerGraph<T>.Vertex>();

            SchedulerGraph = new SchedulerGraph<T>();
            _topologicalOrderVisitor = new TopologicalOrderVisitor<T>();
            GraphData = new ReadOnlyGraph<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge>(SchedulerGraph);
        }

        public void ApplyProfile(IGraphData<SchedulerGraph<Predicate<object>>.Vertex, SchedulerGraph<Predicate<object>>.Edge> profileGraph)
        {
            BeginBatch();
            Clear();

            var vertexDictionary = new Dictionary<SchedulerGraph<Predicate<object>>.Vertex, SchedulerGraph<T>.Vertex>();
            foreach (SchedulerGraph<Predicate<object>>.Vertex predicateVertex in profileGraph.Vertices)
            {
                var vertex = new SchedulerGraph<T>.Vertex(predicateVertex.Items.First());
                vertexDictionary[predicateVertex] = vertex;
                SchedulerGraph.AddVertex(vertex);
            }

            foreach (SchedulerGraph<Predicate<object>>.Edge predicateEdge in profileGraph.Edges)
                SchedulerGraph.AddEdge(vertexDictionary[predicateEdge.Start], vertexDictionary[predicateEdge.End], new SchedulerGraph<T>.Edge());

            EndBatch();
        }

        public virtual TController Plan(T item)
        {
            ItemsVertex.TryGetValue(item, out SchedulerGraph<T>.Vertex vertex);

            if (vertex == null)
            {
                vertex = Add(item);
                Refresh();
            }

            return CreateController(vertex);
        }

        void IScheduler<T>.Plan(T item) => Plan(item);
        protected abstract TController CreateController(SchedulerGraph<T>.Vertex vertex);

        public virtual void Unplan(T item)
        {
            SchedulerGraph.ClearEdges(ItemsVertex[item]);
            Refresh();
        }

        public void Refresh()
        {
            if (IsBatching)
                return;

            _topologicalOrderVisitor.Process(SchedulerGraph);
        }

        protected override QueueBatchNode<T> CreateBatchNode()
        {
            return new QueueBatchNode<T>(this);
        }

        protected override void OnNodeEnded(QueueBatchNode<T> batchNode, int depth)
        {
            foreach (T item in batchNode.Queue)
                AddItemVertex(item);

            Refresh();
        }

        protected SchedulerGraph<T>.Vertex Add(T item)
        {
            if (ItemsVertex.ContainsKey(item))
                return null;

            SchedulerGraph<T>.Vertex vertex = SchedulerGraph.Vertices.FirstOrDefault(x => x.Predicate != null && x.Predicate(item));
            if (vertex != null)
            {
                ItemsVertex.Add(item, vertex);
                vertex.Items.Add(item);
            }
            else
                vertex = AddItemVertex(item);

            Refresh();
            return vertex;
        }

        protected void Remove(T item)
        {
            SchedulerGraph<T>.Vertex vertex;
            ItemsVertex.TryGetValue(item, out vertex);

            if (vertex != null)
            {
                SchedulerGraph.RemoveVertex(vertex);
                Refresh();
            }
        }

        protected void Clear()
        {
            SchedulerGraph.ClearVertices();
            ItemsVertex.Clear();
            Refresh();
        }

        protected internal SchedulerGraph<T>.Vertex AddItemVertex(T item)
        {
            var vertex = new SchedulerGraph<T>.Vertex(item);
            SchedulerGraph.AddVertex(vertex);
            ItemsVertex.Add(item, vertex);

            return vertex;
        }
    }
}