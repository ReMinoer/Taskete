using System;
using System.Collections.Generic;
using OverGraphed;

namespace Taskete
{
    public class SchedulerGraph<T> : AutoGraph<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge>
    {
        public class Vertex : SimpleDirectedVertex<Vertex, Edge>
        {
            public Predicate<object> Predicate { get; }
            public IList<T> Items { get; }
            public Priority Priority { get; set; }

            public Vertex(Predicate<object> insertPredicate)
            {
                Predicate = insertPredicate;

                Items = new List<T>();
                Priority = Priority.Normal;
            }

            public Vertex(T item)
            {
                Items = new List<T> { item };

                Predicate = null;
                Priority = Priority.Normal;
            }

            public void Accept(TopologicalOrderVisitor<T> visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Edge : Edge<Vertex, Edge>
        {
        }
    }
}