using AutoMapper;
using ChilliSource.Cloud.DataStructures;
using ChilliSource.Cloud.Extensions;
using ChilliSource.Cloud.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data
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
            return TaskHelper.GetResultSafeSync(() => ToPagedListAsync(query, page, pageSize, previousPageIfEmpty));
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
            var count = await query.CountAsync()
                              .IgnoreContext();

            var viewModel = new PagedList<T>
            {
                PageCount = (int)Math.Ceiling((float)count / pageSize),
                PageSize = pageSize,
                TotalCount = count,
                CurrentPage = page
            };

            if (previousPageIfEmpty || page <= viewModel.PageCount)
            {
                viewModel.CurrentPage = Math.Max(1, Math.Min(page, viewModel.PageCount));

                IQueryable<T> skip = null;

                if (viewModel.CurrentPage == 1 && pageSize == int.MaxValue)
                {
                    skip = query;
                }
                else
                {
                    skip = query.Skip((viewModel.CurrentPage - 1) * pageSize);
                }

                var elements = await skip.Take(pageSize).ToListAsync()
                                    .IgnoreContext();
                viewModel.AddRange(elements);
            }

            return viewModel;
        }

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
        public static PagedList<TViewModel> GetPagedList<TEntity, TViewModel>(this IQueryable<TEntity> set, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false, bool readOnly = true)
            where TEntity : class
        {
            return TaskHelper.GetResultSafeSync(() => GetPagedListAsync<TEntity, TViewModel>(set, page, pageSize, previousPageIfEmpty, readOnly));
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
        public static async Task<PagedList<TViewModel>> GetPagedListAsync<TEntity, TViewModel>(this IQueryable<TEntity> set, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false, bool readOnly = true)
            where TEntity : class
        {
            PagedList<TEntity> setPaged = await GetPagedListAsync(set, page, pageSize, previousPageIfEmpty, readOnly)
                                                .IgnoreContext();

            if (typeof(TViewModel) == typeof(TEntity))
            {
                return (PagedList<TViewModel>)(object)setPaged;
            }

            var viewModelPaged = PagedList<TViewModel>.CreateFrom<TEntity>(setPaged);

            Mapper.Map(setPaged, viewModelPaged);
            return viewModelPaged;
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
            if (readOnly)
                set = set.AsNoTracking();

            return IQueryableExtensions.ToPagedList(set, page, pageSize, previousPageIfEmpty);
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
        public static Task<PagedList<T>> GetPagedListAsync<T>(this IQueryable<T> set, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false, bool readOnly = true)
            where T : class
        {
            if (readOnly)
                set = set.AsNoTracking();

            return IQueryableExtensions.ToPagedListAsync(set, page, pageSize, previousPageIfEmpty);
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
        public static PagedList<TViewModel> GetPagedListByIndex<TEntity, TViewModel>(this IQueryable<TEntity> set, int index, int pageSize = 10)
            where TEntity : class
        {
            int page = index == -1 ? 1 : (index / pageSize) + 1;
            return GetPagedList<TEntity, TViewModel>(set, page, pageSize);
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
        public static Task<PagedList<TViewModel>> GetPagedListByIndexAsync<TEntity, TViewModel>(this IQueryable<TEntity> set, int index, int pageSize = 10)
            where TEntity : class
        {
            int page = index == -1 ? 1 : (index / pageSize) + 1;
            return GetPagedListAsync<TEntity, TViewModel>(set, page, pageSize);
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="entity">Source list</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        public static List<TViewModel> GetList<TEntity, TViewModel>(this IQueryable<TEntity> query, bool readOnly = true)
            where TEntity : class
        {
            return TaskHelper.GetResultSafeSync(() => GetListAsync<TEntity, TViewModel>(query, readOnly));
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="entity">Source list</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        public static async Task<List<TViewModel>> GetListAsync<TEntity, TViewModel>(this IQueryable<TEntity> query, bool readOnly = true)
            where TEntity : class
        {
            var viewModel = new List<TViewModel>();
            if (readOnly)
                query = query.AsNoTracking();

            var elements = await query.ToListAsync()
                                    .IgnoreContext();

            Mapper.Map(elements, viewModel);

            return viewModel;
        }        
    }
}
