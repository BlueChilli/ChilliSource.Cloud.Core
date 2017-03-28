using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    /// <summary>
    /// Extension methods for System.Collections.Generic.List&lt;T&gt;.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Adds the item to specified collection list or updates if exists by the key selector function.
        /// </summary>
        /// <typeparam name="T">The type of the elements of list.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The System.Collections.Generic.List&lt;T&gt;.</param>
        /// <param name="item">The item to add or update.</param>
        /// <param name="keySelector">A function to select the key.</param>
        public static void AddOrUpdate<T, TKey>(this List<T> source, T item, Func<T, TKey> keySelector)
        {
            if (item == null)
                throw new ArgumentException("item is null");

            var itemKey = keySelector(item);
            for (int i = 0; i < source.Count; i++)
            {
                var otherKey = keySelector(source[i]);
                if ((otherKey == null && itemKey == null) || (otherKey?.Equals(itemKey) == true))
                {
                    source[i] = item;
                    return;
                }
            }

            source.Add(item);
        }
    }
}
