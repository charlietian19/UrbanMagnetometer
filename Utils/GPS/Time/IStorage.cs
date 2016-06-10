using System;
using System.Collections.Generic;

namespace Utils.GPS.Time
{
    public interface IStorage<T> : IList<T>
    {
        /* Maximum number of objects the collection can 
        contain before they are pushed out */
        int MaxCount { get; set; }

        /* Called when the collection is overflowed and 
        one object is pushed out */
        event Action<T> AfterPop;

        /* Called when a new item is added to the collection */
        event Action<T> BeforePush;

        /* Returns the data in the collection as an array */
        T[] ToArray();

        /* Pushes all points out of the collection */
        void Flush();
    }
}
