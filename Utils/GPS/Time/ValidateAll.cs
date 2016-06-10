using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.GPS.Time
{
    public class ValidateAll<T>
    {
        /* Returns true if applying condition to every member of 
        collection returns true */
        public static bool And(T obj, IEnumerable<T> all, 
            Func<T, T, bool> condition)
        {
            foreach (var that in all)
            {
                if (!condition(obj, that))
                {
                    return false;
                }
            }
            return true;
        }

        /* Returns true if applying condition to at least one member
        of collection returns true */
        public static bool Or(T obj, IEnumerable<T> all,
            Func<T, T, bool> condition)
        {
            foreach (var that in all)
            {
                if (condition(obj, that))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
