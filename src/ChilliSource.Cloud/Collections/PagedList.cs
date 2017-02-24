using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Collections
{
    /// <summary>
    /// Represents a page of loaded elements (from a bigger set of elements)
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        /// <summary>
        /// Copies paging information from one instance to another. Elements are not copied.
        /// </summary>
        /// <typeparam name="TSource">Origin type</typeparam>
        /// <param name="list">Original list</param>
        /// <returns>New list</returns>
        public static PagedList<T> CreateFrom<TSource>(PagedList<TSource> list)
        {
            if (list == null)
            {
                return null;
            }

            return new PagedList<T>()
            {
                PageCount = list.PageCount,
                CurrentPage = list.CurrentPage,
                PageSize = list.PageSize,
                TotalCount = list.TotalCount
            };
        }
        /// <summary>
        /// Number of pages
        /// </summary>
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        /// <summary>
        /// Total number of elements
        /// </summary>
        public int TotalCount { get; set; }
    }
}
