using System;
using System.Collections.Generic;
using OverGraphed;

namespace Taskete
{
    public class SchedulerGraph<T> : Graph<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge>
    {
        public class Vertex : Vertex<Vertex, Edge>, IVisitable<TopologicalOrderVisitor<T>, Vertex, Vertex, Edge>
        {
            public Predicate<object> Predicate { get; set; }
            public IList<T> Items { get; private set; }
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