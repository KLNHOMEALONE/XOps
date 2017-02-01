namespace XOps.Core
{
    public interface IPriorityQueue<T>
    {
        /// <summary>
        /// Number of items in the queue.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Method adds item to the queue.
        /// </summary>
        void Enqueue(T item, int priority);
        /// <summary>
        /// Method returns item with the LOWEST priority value.
        /// </summary>
        T Dequeue();
    }
}