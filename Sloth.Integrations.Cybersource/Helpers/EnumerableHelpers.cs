using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Integrations.Cybersource.Helpers
{
    public static class EnumerableHelpers
    {
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var i = -1;
            foreach (var item in source)
            {
                i++;
                if (predicate(item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
