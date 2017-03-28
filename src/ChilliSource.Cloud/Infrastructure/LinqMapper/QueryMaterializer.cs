using ChilliSource.Cloud.LinqMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.LinqMapper
{
    internal class QueryMaterializer<TSource, TDest> : IQueryMaterializer<TSource, TDest>
        where TSource : class
        where TDest : class, new()
    {
        IQueryable<TSource> _source;
        IObjectContext _materializerContext;
        Func<IQueryable<TDest>, IQueryable<TDest>> _queryAction = null;

        public QueryMaterializer(IQueryable<TSource> source)
        {
            _source = source;
            _materializerContext = LinqMapper.CreateContext();
        }

        private IQueryable<TDest> GetDestQuery()
        {
            var map = LinqMapper.GetMap<TSource, TDest>(_materializerContext);
            var query = _source.Select(map);

            if (_queryAction != null)
                query = _queryAction(query);

            return query;
        }

        public Task<TResult> To<TResult>(Func<IQueryable<TDest>, Task<TResult>> runAction)
        {
            var resultTask = runAction(GetDestQuery());
            return MaterializeTask(resultTask);
        }

        private Task<TResult> MaterializeTask<TResult>(Task<TResult> resultTask)
        {
            return resultTask.ContinueWith(t => Materializer.ApplyAfterMap(t.Result, _materializerContext), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public TResult To<TResult>(Func<IQueryable<TDest>, TResult> runAction)
        {
            var result = runAction(GetDestQuery());
            if (result is Task)
            {
                //Only Task<T> tasks are valid
                if (result.GetType() == typeof(Task))
                {
                    throw new ApplicationException("QueryMaterializer: The task is not valid because it doesn't produce any result.");
                }

                return (TResult)MaterializeTask((dynamic)result);
            }

            return Materializer.ApplyAfterMap(result, _materializerContext);
        }

        public override string ToString()
        {
            return GetDestQuery().ToString();
        }

        public IQueryMaterializer<TSource, TDest> Context<TContext>(TContext value)
        {
            _materializerContext.SetContext<TContext>(value);
            return this;
        }

        public IQueryMaterializer<TSource, TDest> Query(Func<IQueryable<TDest>, IQueryable<TDest>> queryAction)
        {
            if (_queryAction == null)
            {
                _queryAction = queryAction;
            }
            else
            {
                var currentAction = _queryAction;
                _queryAction = (IQueryable<TDest> q) => queryAction(currentAction(q));
            }

            return this;
        }
    }

    /// <summary>
    /// The materializer wrapper contract definition. It is obtained via query.Materialize extension method.
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TDest">The projection type</typeparam>
    public interface IQueryMaterializer<TSource, TDest>
    {
        /// <summary>
        /// Applies a custom action (e.g. filterting or ordering) on the internal query.
        /// </summary>
        /// <param name="queryAction">A query delegate action</param>
        /// <returns>This instance</returns>
        IQueryMaterializer<TSource, TDest> Query(Func<IQueryable<TDest>, IQueryable<TDest>> queryAction);

        /// <summary>
        /// Sets a value in the current context to be used by the map and after map actions.
        /// </summary>
        /// <typeparam name="TContext">The current context type</typeparam>
        /// <param name="value">The current context value</param>
        /// <returns>This instance</returns>
        IQueryMaterializer<TSource, TDest> Context<TContext>(TContext value);

        /// <summary>
        /// Exexutes a registered mapping on a query, runs the specified delegate to produce a result and applies any registered after map actions
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="runAction">A delegate action on IQueryable&lt;TDest&gt;</param>
        /// <returns>A result object</returns>
        TResult To<TResult>(Func<IQueryable<TDest>, TResult> runAction);


        /// <summary>
        /// Exexutes a registered mapping on a query, applies any registered after map actions, and runs the specified delegate to produce a result
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="runAction">An async delegate action on IQueryable&lt;TDest&gt;</param>
        /// <returns></returns>
        Task<TResult> To<TResult>(Func<IQueryable<TDest>, Task<TResult>> runAction);
    }
}
