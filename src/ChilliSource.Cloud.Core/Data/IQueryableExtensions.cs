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
using AutoMapper;
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

        /// <summary>
        /// Seeks a list for the requested page and returns a PagedList object.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="query">Element query</param>
        /// <param name="sortBy">[Not used]</param>
        /// <param name="page">Requested page</param>
        /// <param name="pageSize">Number of elements on each page.</param>        
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <returns>A PagedList object, containing the elements on the request page.</returns>
        public static PagedList<T> ToPagedList<T>(this IQueryable<T> query, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false)
        {
            return ToPagedListInternal(query, page, pageSize, previousPageIfEmpty, isAsync: false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// (Async) Seeks a list for the requested page and returns a PagedList object.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="query">Element query</param>
        /// <param name="sortBy">[Not used]</param>
        /// <param name="page">Requested page</param>
        /// <param name="pageSize">Number of elements on each page.</param>        
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <returns>A PagedList object, containing the elements on the request page.</returns>
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false)
        {
            return await ToPagedListInternal(query, page, pageSize, previousPageIfEmpty, isAsync: true);
        }

        /// <summary>
        /// Pagination on a set of elements.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>
        /// <param name="sortBy">Not used</param>
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        public static PagedList<T> GetPagedList<T>(this IQueryable<T> set, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false, bool readOnly = true)
        where T : class
        {
            return ToPagedListInternal(set, page, pageSize, previousPageIfEmpty, isAsync: false, readOnly: readOnly).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Pagination on a set of elements.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>
        /// <param name="sortBy">Not used</param>
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        public static async Task<PagedList<T>> GetPagedListAsync<T>(this IQueryable<T> set, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false, bool readOnly = true)
            where T : class
        {
            return await ToPagedListInternal(set, page, pageSize, previousPageIfEmpty, isAsync: true, readOnly: readOnly);
        }

        private static bool CheckAsyncSupported<T>(IQueryable<T> query)
        {
#if NET_4X
            return query?.Provider is IDbAsyncQueryProvider;
#else
            return query?.Provider is IAsyncQueryProvider;
#endif
        }

        private static async Task<PagedList<T>> ToPagedListInternal<T>(IQueryable<T> query, int page, int pageSize, bool previousPageIfEmpty, bool isAsync, bool readOnly)
            where T : class
        {
            if (readOnly)
            {
                query = query.AsNoTracking();
            }

            return await ToPagedListInternal(query, page, pageSize, previousPageIfEmpty, isAsync: isAsync);
        }

        private static async Task<PagedList<T>> ToPagedListInternal<T>(IQueryable<T> query, int page, int pageSize, bool previousPageIfEmpty, bool isAsync)
        {
            if (isAsync)
            {
                isAsync = CheckAsyncSupported(query);
            }

            var currentPage = Math.Max(1, page);
            var elements = await TakePageInternal(query, currentPage, pageSize, isAsync).IgnoreContext();

            // The page is fetched before the count so that the count can often be skipped
            // altogether. A short but non-empty page is by definition the last page, so the
            // total is (rows skipped + rows returned). An empty first page means an empty set.
            // Only a full page - or an empty page past the first - still has to ask the database.
            var skipped = (long)(currentPage - 1) * pageSize;
            int count;

            if (elements.Count > 0 && elements.Count < pageSize && skipped + elements.Count <= int.MaxValue)
            {
                count = (int)(skipped + elements.Count);
            }
            else if (elements.Count == 0 && currentPage == 1)
            {
                count = 0;
            }
            else
            {
                count = isAsync ? await query.CountAsync().IgnoreContext()
                                : query.Count();
            }

            var viewModel = new PagedList<T>
            {
                PageCount = (int)Math.Ceiling((float)count / pageSize),
                PageSize = pageSize,
                TotalCount = count,
                UnfilteredCount = count,
                CurrentPage = page
            };

            if (elements.Count == 0 && currentPage > viewModel.PageCount)
            {
                // The requested page is past the end of the set.
                if (!previousPageIfEmpty) return viewModel;

                var lastPage = Math.Max(1, viewModel.PageCount);
                if (lastPage != currentPage)
                {
                    currentPage = lastPage;
                    elements = await TakePageInternal(query, currentPage, pageSize, isAsync).IgnoreContext();
                }
            }

            viewModel.CurrentPage = Math.Max(1, Math.Min(currentPage, viewModel.PageCount));
            viewModel.AddRange(elements);

            return viewModel;
        }

        private static async Task<List<T>> TakePageInternal<T>(IQueryable<T> query, int page, int pageSize, bool isAsync)
        {
            var skip = page == 1 ? query : query.Skip((page - 1) * pageSize);

            if (pageSize != int.MaxValue)
            {
                skip = skip.Take(pageSize);
            }

            return isAsync ? await skip.ToListAsync().IgnoreContext()
                           : skip.ToList();
        }

#if NET_10X
        /// <summary>
        /// Transform a list of T (usually data model) into a paged list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>       
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether data entities will be used for read-only operations. If true, entities will not be added to the current Data Context.</param>
        public static PagedList<TViewModel> GetPagedList<TEntity, TViewModel>(this IQueryable<TEntity> set, IMapper mapper, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false, bool readOnly = true)
            where TEntity : class
        {
            return GetPagedListInternal<TEntity, TViewModel>(set, mapper, page, pageSize, previousPageIfEmpty, isAsync: false, readOnly: readOnly).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a paged list of TX (usually view model) using AutoMapper
        /// Instead of asking for page x, ask for an index and will return the page this index is on
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="index">Index of item to be returned in page x</param>
        /// <param name="pageSize">Size of each page</param>
        public static PagedList<TViewModel> GetPagedListByIndex<TEntity, TViewModel>(this IQueryable<TEntity> set, IMapper mapper, int index, int pageSize = 10)
            where TEntity : class
        {
            int page = index == -1 ? 1 : (index / pageSize) + 1;
            return GetPagedList<TEntity, TViewModel>(set, mapper, page, pageSize);
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a paged list of TX (usually view model) using AutoMapper
        /// Instead of asking for page x, ask for an index and will return the page this index is on
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="index">Index of item to be returned in page x</param>
        /// <param name="pageSize">Size of each page</param>
        public static Task<PagedList<TViewModel>> GetPagedListByIndexAsync<TEntity, TViewModel>(this IQueryable<TEntity> set, IMapper mapper, int index, int pageSize = 10)
            where TEntity : class
        {
            int page = index == -1 ? 1 : (index / pageSize) + 1;
            return GetPagedListAsync<TEntity, TViewModel>(set, mapper, page, pageSize);
        }

        /// <summary>
        /// (Async) Transform a list of T (usually data model) into a paged list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>       
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether data entities will be used for read-only operations. If true, entities will not be added to the current Data Context.</param>
        public static async Task<PagedList<TViewModel>> GetPagedListAsync<TEntity, TViewModel>(this IQueryable<TEntity> set, IMapper mapper, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false, bool readOnly = true)
            where TEntity : class
        {
            return await GetPagedListInternal<TEntity, TViewModel>(set, mapper, page, pageSize, previousPageIfEmpty, isAsync: true, readOnly: readOnly);
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="entity">Source list</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        public static List<TViewModel> GetList<TEntity, TViewModel>(this IQueryable<TEntity> query, IMapper mapper, bool readOnly = true)
            where TEntity : class
        {
            return GetListInternal<TEntity, TViewModel>(query, mapper, isAsync: false, readOnly: readOnly).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="entity">Source list</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        public static async Task<List<TViewModel>> GetListAsync<TEntity, TViewModel>(this IQueryable<TEntity> query, IMapper mapper, bool readOnly = true)
            where TEntity : class
        {
            return await GetListInternal<TEntity, TViewModel>(query, mapper, isAsync: true, readOnly: readOnly);
        }

        private static async Task<PagedList<TViewModel>> GetPagedListInternal<TEntity, TViewModel>(IQueryable<TEntity> set, IMapper mapper, int page, int pageSize, bool previousPageIfEmpty, bool isAsync, bool readOnly)
            where TEntity : class
        {
            var setPaged = await ToPagedListInternal(set, page, pageSize, previousPageIfEmpty, isAsync: isAsync, readOnly: readOnly)
                                    .IgnoreContext();

            if (typeof(TViewModel) == typeof(TEntity))
            {
                return (PagedList<TViewModel>)(object)setPaged;
            }

            var viewModelPaged = PagedList<TViewModel>.CreateFrom<TEntity>(setPaged);

            mapper.Map(setPaged, viewModelPaged);
            return viewModelPaged;
        }

        private static async Task<List<TViewModel>> GetListInternal<TEntity, TViewModel>(IQueryable<TEntity> query, IMapper mapper, bool isAsync, bool readOnly)
            where TEntity : class
        {
            var viewModel = new List<TViewModel>();
            if (readOnly)
            {
                query = query.AsNoTracking();
            }

            var elements = isAsync ? await query.ToListAsync().IgnoreContext()
                                   : query.ToList();

            mapper.Map(elements, viewModel);

            return viewModel;
        }

        /// <summary>
        /// Sets the UnfilteredCount property of a PagedList based on the provided scope and filtered queries.
        /// </summary>
        /// <typeparam name="TDest"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="data"></param>
        /// <param name="scope"></param>
        /// <param name="filtered"></param>
        /// <returns></returns>
        public static PagedList<TDest> WithScope<TDest, TSource>(
            this PagedList<TDest> data, IQueryable<TSource> scope, IQueryable<TSource> filtered)
        {
            data.UnfilteredCount = ReferenceEquals(scope, filtered) ? data.TotalCount : scope.Count();
            return data;
        }
#endif

    }
}
