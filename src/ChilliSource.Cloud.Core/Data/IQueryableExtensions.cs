using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;
using System.Linq.Expressions;

#if NET_4X
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#else
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
#endif

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    ///     Extensions for Entity queries.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        ///     Applies includes to an entity query.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="query">Entity query</param>
        /// <param name="includes">Anonymous function that return the includes to be applied to an entity query.</param>
        /// <returns>The entity query with includes applied or the original entity query if includes is null.</returns>
        public static IQueryable<TEntity> ApplyIncludes<TEntity>(this IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes)
        {
            return includes == null ? query : includes(query);
        }

        /// <summary>
        /// Reduces collection to distinct members by a key property.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the IQueryable source.</typeparam>
        /// <typeparam name="TKey">The type of the key property.</typeparam>
        /// <param name="source">IQueryable source.</param>
        /// <param name="keySelector">A function to determine uniqueness for the distinct operation.</param>
        /// <returns></returns>
        public static IQueryable<TSource> DistinctBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.GroupBy(keySelector).Select(g => g.FirstOrDefault());
        }

        /// <summary>
        /// Returns the first element of a sequence, or a new instance if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>A new instance of TSource when source is empty; otherwise, the first element in source.</returns>
        public static TSource FirstOrNew<TSource>(this IQueryable<TSource> source)
        {
            TSource tSource = source.FirstOrDefault<TSource>();
            if (tSource != null)
            {
                return tSource;
            }
            return Activator.CreateInstance<TSource>();
        }

        /// <summary>
        /// Returns the first element of a sequence, or a new instance if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>A new instance of TSource when source is empty; otherwise, the first element in source.</returns>
        public static TSource FirstOrNew<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            var query = source?.Where(predicate);
            return query.FirstOrNew();
        }
    }
}
