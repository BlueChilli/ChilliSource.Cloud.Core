using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
#endif

namespace ChilliSource.Cloud.Core.LinqMapper
{
    /// <summary>
    /// Auto applies an after map action on a query result. The action needs to be registered using Materializer.RegisterAfterMap() .
    /// </summary>
    public static class MaterializerExtensions
    {
        /// <summary>
        /// <para>Auto applies an after map action on a query result when materializing objects. </para>
        /// <para>The context MUST must be set via .Context() if required by the registered action.</para>
        /// <para>Registered actions on interfaces and base classes are also automaticaly applied in this order: Interface actions, Base Class (top to bottom) actions, Target type action.</para>
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The projection type</typeparam>
        /// <param name="query">A source query</param>
        /// <returns>A materializer wrapper for the result</returns>
        public static IQueryMaterializer<TSource, TDest> Materialize<TSource, TDest>(this IQueryable<TSource> query)
            where TSource : class
             where TDest : class, new()
        {
            return new QueryMaterializer<TSource, TDest>(query);
        }

        /// <summary>
        /// Exexutes the registered mapping on a query, applies any registered after map actions, and runs query.FirstOrDefault() on the result
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The projection type</typeparam>
        /// <param name="materializer">A materializer wrapper</param>
        /// <returns>A projection instance</returns>
        public static TDest FirstOrDefault<TSource, TDest>(this IQueryMaterializer<TSource, TDest> materializer)
        {
            return materializer.To(q => q.FirstOrDefault());
        }

        /// <summary>
        /// Exexutes the registered mapping on a query, applies any registered after map actions, and runs query.FirstOrDefault() on the result
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The projection type</typeparam>
        /// <param name="materializer">A materializer wrapper</param>
        /// <returns>A projection instance</returns>
        public static Task<TDest> FirstOrDefaultAsync<TSource, TDest>(this IQueryMaterializer<TSource, TDest> materializer)
        {
            return materializer.To(q => q.FirstOrDefaultAsync());
        }

        /// <summary>
        /// Exexutes the registered mapping on a query, applies any registered after map actions, and runs query.ToArray() on the result
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The projection type</typeparam>
        /// <param name="materializer">A materializer wrapper</param>
        /// <returns>A projection array</returns>
        public static TDest[] ToArray<TSource, TDest>(this IQueryMaterializer<TSource, TDest> materializer)
        {
            return materializer.To(q => q.ToArray());
        }

        /// <summary>
        /// Exexutes the registered mapping on a query, applies any registered after map actions, and runs query.ToArray() on the result
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The projection type</typeparam>
        /// <param name="materializer">A materializer wrapper</param>
        /// <returns>A projection array</returns>
        public static Task<TDest[]> ToArrayAsync<TSource, TDest>(this IQueryMaterializer<TSource, TDest> materializer)
        {
            return materializer.To(q => q.ToArrayAsync());
        }
    }
}
