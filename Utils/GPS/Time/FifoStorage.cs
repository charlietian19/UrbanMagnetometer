using System;
using System.Collections.Generic;

namespace Utils.GPS
{
    /* List of data that stores a given maximum number of elements.
    Old elements are pushed */
    public class FifoStorage<T> : List<T>, IStorage<T>
    {
        public int MaxCount { get; set; } = 0;

        public event Action<T> OnPop;

        public FifoStorage(int maxSize) : base(maxSize)
        {            
            MaxCount = maxSize;
        }        

        public new void Add(T obj)
        {
            if (Count < MaxCount)
            {
                base.Add(obj);
            }
            else
            {
                var res = this[0];                
                Push(obj);
                Pop(res);
            }
        }

        /* Called to add a new object to the queue */
        void Push(T obj)
        {
            for (int i = 0; i < Count - 1; i++)
            {
                this[i] = this[i + 1];
            }
            this[Count - 1] = obj;
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
            for (int i = 0; i < Count; i++)
            {
                Pop(this[i]);
            }
            Clear();
        }        
    }
}
