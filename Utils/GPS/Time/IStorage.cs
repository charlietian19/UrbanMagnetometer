using System;
using System.Collections.Generic;

namespace Utils.GPS.Time
{
    public interface IStorage<T> : IEnumerable<T>
    {
        /* Maximum number of objects the collection can 
        contain before they are pushed out */
        int MaxCount { get; set; }

        /* Number of objects currently in the collection*/
        int Count { get; }

        /* Called when the collection is overflowed and 
        one object is pushed out */
        event Action<T> OnPop;

        /* Adds an object to the collection */
        void Add(T data);

        /* Returns the data in the collection as an array */
        T[] ToArray();

        /* Pushes all points out of the collection */
        void Flush();
    }
}
