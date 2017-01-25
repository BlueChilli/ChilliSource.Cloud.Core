using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ChilliSource.Cloud.Data
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Using EF, save (insert or update) a view model mapped to a data model to the database
        /// Used for simple saving when no record ownership checking need to be performed
        /// </summary>
        /// <typeparam name="TViewModel">Source Type</typeparam>
        /// <typeparam name="TEntity">Destination Type</typeparam>
        /// <param name="set">Destination table</param>
        /// <param name="viewModel">Data to be saved</param>
        /// <param name="keyValues">Values of PK to find item to be saved (partial update or full update depending on view model mapping)</param>
        /// <returns>Returns view model mapped from saved data model (identity column inserted id's filled in)</returns>
        public static TViewModel Save<TViewModel, TEntity>(this DbContext context, TViewModel viewModel, params object[] keyValues) where TEntity : class, new()
        {
            var set = context.Set<TEntity>();

            var entity = set.Find(keyValues);
            if (entity == null)
                entity = new TEntity();

            return Save<TViewModel, TEntity, TEntity>(context, viewModel, entity);
        }

        /// <summary>
        /// Adds or updates an entity in the database from a ViewModel, only if a certain condition is satisfied. The Entity class needs to implement IPersistableObj interface.
        /// </summary>
        /// <typeparam name="TViewModel">View model type</typeparam>
        /// <typeparam name="TDbSet">Base entity type (i.e DbSet type)</typeparam>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="set">Entity data set (e.g. Context.Drills)</param>
        /// <param name="viewModel">View model instance</param>
        /// <param name="filter">Condition to be satisfied [e.g (UserDrill d) => d.AccountId == 5]</param>
        /// <param name="key">Entity key value</param>
        /// <param name="includes">Includes needed to map the entity to the view model type.</param>
        /// <returns>The saved entity mapped to the view model type or null if the condition is not satisfied.</returns>
        public static TViewModel SaveIfAllowed<TViewModel, TDbSet, TEntity>(this DbContext context, TViewModel viewModel, Expression<Func<TEntity, bool>> filter, int key, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
            where TDbSet : class, IPersistableObj
            where TEntity : class, TDbSet, new()
        {
            var set = context.Set<TDbSet>();

            var entity = set.OfType<TEntity>().Where(e => e.Id == key)
                .ApplyIncludes(includes)
                .FirstOrDefault();

            if (entity == null)
            {
                using (var tr = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    entity = set.Create<TEntity>();
                    Mapper.Map(viewModel, entity);
                    set.Add(entity);
                    context.SaveChanges();
                    var newId = entity.Id;

                    //apply filter on newly added entity.
                    var savedData = set.OfType<TEntity>().Where(filter).Where(e => e.Id == newId)
                        .ApplyIncludes(includes)
                        .FirstOrDefault();

                    if (savedData == null) return default(TViewModel);

                    tr.Complete();
                }
            }
            else
            {
                var allowed = set.OfType<TEntity>().Where(filter).Where(e => e.Id == key).Any();
                if (!allowed) return default(TViewModel);

                Mapper.Map(viewModel, entity);
                context.SaveChanges();
            }

            // map back to viewModel (to get any identity column inserted ID)
            Mapper.Map(entity, viewModel);

            return viewModel;
        }

        /// <summary>
        /// Using EF, save (insert or update) a view model mapped to a data model to the database
        /// Shortcut logic for conditional logic (record ownership checking) for updates, and base values for insert
        /// </summary>
        /// <typeparam name="TViewModel">Source type</typeparam>
        /// <typeparam name="TDbSet">Destination table type</typeparam>
        /// <typeparam name="TEntity">Destination type</typeparam>
        /// <param name="set">Destination table</param>
        /// <param name="viewModel">Data to be saved</param>
        /// <param name="filter"></param>
        /// <param name="insertBase"></param>
        /// <returns>Returns view model mapped from saved data model (identity column inserted id's filled in)</returns>
        public static TViewModel Save<TViewModel, TDbSet, TEntity>(this DbContext context, TViewModel viewModel, Expression<Func<TEntity, bool>> filter, TEntity insertBase = null)
            where TDbSet : class
            where TEntity : class, TDbSet, new()
        {
            var set = context.Set<TDbSet>();

            var entity = set.OfType<TEntity>().FirstOrDefault(filter);

            if (entity == null)
                entity = insertBase == null ? new TEntity() : insertBase;

            return Save<TViewModel, TDbSet, TEntity>(context, viewModel, entity);
        }

        /// <summary>
        /// Using EF, save (insert or update) a view model mapped to a loaded data model to the database. 
        /// </summary>
        /// <typeparam name="TViewModel">Source type</typeparam>
        /// <typeparam name="TDbSet">Destination table type</typeparam>
        /// <typeparam name="TEntity">Destination type</typeparam>
        /// <param name="set">Destination table</param>
        /// <param name="viewModel">Data to be saved</param>
        /// <param name="entity">Data to be merge into, which is then saved</param>
        /// <returns></returns>
        public static TViewModel Save<TViewModel, TDbSet, TEntity>(this DbContext context, TViewModel viewModel, TEntity entity)
            where TDbSet : class
            where TEntity : class, TDbSet, new()
        {
            var set = context.Set<TDbSet>();

            // map and save
            Mapper.Map(viewModel, entity);
            set.AddOrUpdate(entity);
            context.SaveChanges();

            // map back to viewModel (to get any identity column inserted ID)
            Mapper.Map(entity, viewModel);

            return viewModel;
        }

        /// <summary>
        /// Delete an item via PK
        /// </summary>
        /// <typeparam name="TEntity">Destination type</typeparam>
        /// <param name="set">Destination table</param>
        /// <param name="keyValues">Values of PK to find item to be deleted</param>
        public static void Delete<TEntity>(this DbContext context, params object[] keyValues) where TEntity : class, new()
        {
            var set = context.Set<TEntity>();

            var entity = set.Find(keyValues);
            Delete(context, entity);
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        /// <typeparam name="TEntity">Destination type</typeparam>
        /// <param name="set">Destination table</param>
        /// <param name="entity">Item to be deleted</param>
        public static void Delete<TEntity>(this DbContext context, TEntity entity) where TEntity : class, new()
        {
            var set = context.Set<TEntity>();

            if (entity != null)
            {
                set.Remove(entity);
                context.SaveChanges();
            }
        }
    }
}
