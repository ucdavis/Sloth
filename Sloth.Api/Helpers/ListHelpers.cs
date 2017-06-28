using System;
using System.Collections.Generic;

namespace Sloth.Api.Helpers
{
    public static class ListHelpers
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (items == null) throw new ArgumentNullException(nameof(items));

            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (list is List<T>)
            {
                ((List<T>)list).AddRange(items);
                return;
            }

            foreach (var item in items)
            {
                list.Add(item);
            }
        }

    }
}
