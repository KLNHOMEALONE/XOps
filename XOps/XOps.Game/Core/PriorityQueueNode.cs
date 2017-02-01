using System;

namespace XOps.Core
{
    public class PriorityQueueNode<T> : IComparable
    {
        public T Item { get; private set; }
        public float Priority { get; private set; }

        public PriorityQueueNode(T item, float priority)
        {
            Item = item;
            Priority = priority;
        }

        public int CompareTo(object obj)
        {
            return Priority.CompareTo((obj as PriorityQueueNode<T>).Priority);
        }
    }
}