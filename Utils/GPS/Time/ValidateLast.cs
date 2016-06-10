using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.GPS.Time
{
    public class ValidateLast<T>
    {
        /* Returns true if applying condition to the last n objects 
        of collection returns true */
        public static bool And(T obj, IList<T> all, int n,
            Func<T, T, bool> condition)
        {
            if (n > all.Count)
            {
                n = all.Count;
            }

            for (int i = 0; i < n; i++)
            {
                if (!condition(obj, all[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /* Returns true if applying condition to at least one of the last
        members of collection returns true */
        public static bool Or(T obj, IList<T> all, int n,
            Func<T, T, bool> condition)
        {
            if (n > all.Count)
            {
                n = all.Count;
            }

            for (int i = 0; i < n; i++)
            {
                if (condition(obj, all[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
