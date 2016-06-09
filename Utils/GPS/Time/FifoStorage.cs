using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils.GPS.Time
{
    /* List of data that stores a given maximum number of elements.
    Old elements are pushed */
    public class FifoStorage<T> : IStorage<T>
    {
        public int MaxCount { get; set; } = 0;
        protected Queue<T> data;

        public event Action<T> OnPop;

        public FifoStorage(int maxSize)
        {
            MaxCount = maxSize;
            data = new Queue<T>(maxSize + 1);
        }        

        public void Add(T obj)
        {
            data.Enqueue(obj);
            if (data.Count > MaxCount)
            {
                Pop();
            }
        }

        /* Called to push an object out of the queue */
        protected virtual void Pop()
        {
            var obj = data.Dequeue();
            if (OnPop != null)
            {
                OnPop(obj);
            }
        }

        /* Called to push all the objects out of the queue */
        public void Flush()
        {
            while (data.Count > 0)
            {
                Pop();
            }
        }

        public int Count { get { return data.Count; } }

        public T[] ToArray()
        {
            return data.ToArray<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator(); 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
