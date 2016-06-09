using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils.GPS
{
    /* List of data that stores a given maximum number of elements.
    Old elements are pushed */
    public class FifoStorage<T> : IStorage<T>
    {
        public int MaxCount { get; set; } = 0;
        protected List<T> data;

        public event Action<T> OnPop;

        public FifoStorage(int maxSize)
        {
            MaxCount = maxSize;
            data = new List<T>(maxSize + 1);
        }        

        public void Add(T obj)
        {
            if (data.Count < MaxCount)
            {
                data.Add(obj);
            }
            else
            {
                Pop(data[0]);
                Push(obj);
            }
        }

        /* Called to add a new object to the queue */
        void Push(T obj)
        {
            for (int i = 0; i < data.Count - 1; i++)
            {
                data[i] = data[i + 1];
            }
            data[data.Count - 1] = obj;
        }

        /* Called to push an object out of the queue */
        protected virtual void Pop(T obj)
        {
            if (OnPop != null)
            {
                OnPop(obj);
            }
        }

        /* Called to push all the objects out of the queue */
        public void Flush()
        {
            for (int i = 0; i < data.Count; i++)
            {
                Pop(data[i]);
            }
            data.Clear();
        }

        public int Count { get { return data.Count; } }

        public T[] ToArray()
        {
            return data.ToArray();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator(); 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }
    }
}
