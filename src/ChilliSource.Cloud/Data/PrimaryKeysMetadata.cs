using LinqKit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data
{
    internal static class PrimaryKeysMetadataFactory
    {
        public static IPrimaryKeysMetadata GetForInstance<TEntity>(DbContext context)
        {
            return (IPrimaryKeysMetadata)Get((dynamic)context);
        }

        private static IPrimaryKeysMetadata Get<TDbContext, TEntity>(TDbContext context)
            where TDbContext : DbContext
            where TEntity : class
        {
            return PrimaryKeysMetadata<TDbContext, TEntity>.GetInstance(context);
        }
    }

    internal interface IPrimaryKeysMetadata
    {
        object[] GetPrimaryKeys(object entity);
        TQuery FilterByKeys<TQuery>(TQuery set, object[] keyValues) where TQuery : IQueryable;
    }

    internal class PrimaryKeysMetadata<TDbContext, TEntity> : IPrimaryKeysMetadata
        where TDbContext : DbContext
        where TEntity : class
    {
        Func<TEntity, object>[] _keyGetters;
        private PrimaryKeysMetadata(Func<TEntity, object>[] keyGetters, LambdaExpression[] expressions)
        {
            _keyGetters = keyGetters;
            _expressions = expressions;
        }


        private static IPrimaryKeysMetadata _instance = null;
        private Func<TEntity, object>[] getters;
        private LambdaExpression[] _expressions;

        public static IPrimaryKeysMetadata GetInstance(TDbContext context)
        {
            if (_instance != null)
                return _instance;
            return (_instance = Init(context));
        }

        private static IPrimaryKeysMetadata Init(TDbContext context)
        {
            Type type = typeof(TEntity);

            var keysDefinition = GetPrimaryKeysDefinition(context);
            var getters = keysDefinition.Select(k =>
            {
                var property = type.GetProperty(k.Name);
                if (property == null)
                    throw new ApplicationException($"Property info not found for key name: {k.Name}");

                var parameter = Expression.Parameter(type, "entity");
                var propertyExp = Expression.Property(parameter, property);
                var castExp = Expression.Convert(propertyExp, typeof(object));
                var expression = Expression.Lambda<Func<TEntity, object>>(castExp, parameter);
                //Compiles lambda getter exp: (TEntity entity) => (Object) entity.PropertyName
                return expression.Compile();
            }).ToArray();

            var expressions = keysDefinition.Select(k =>
            {
                var property = type.GetProperty(k.Name);
                if (property == null)
                    throw new ApplicationException($"Property info not found for key name: {k.Name}");

                var parameter = Expression.Parameter(type, "entity");
                var propertyExp = Expression.Property(parameter, property);
                var expression = Expression.Lambda(propertyExp, parameter);

                //Compiles lambda getter exp: (TEntity entity) => (Object) entity.PropertyName
                return expression;
            }).ToArray();

            return new PrimaryKeysMetadata<TDbContext, TEntity>(getters, expressions);
        }

        private static IList<EdmMember> GetPrimaryKeysDefinition(DbContext context)
        {
            var objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<TEntity>();
            return objectSet.EntitySet.ElementType.KeyMembers;
        }

        public object[] GetPrimaryKeys(object entity)
        {
            var result = new object[_keyGetters.Length];
            for (int i = 0; i < _keyGetters.Length; i++)
            {
                result[i] = _keyGetters[i].Invoke((TEntity)entity);
            }

            return result;
        }

        public IQueryable<TEntity> FilterByKeysTyped(IQueryable<TEntity> set, object[] keyValues)
        {
            var filter = PredicateBuilder.New<TEntity>();

            for (int i = 0; i < _expressions.Length; i++)
            {
                var propertyExp = _expressions[i];
                var value = keyValues[i];

                var propertyCompare = CreatePropertyCompareExpression(propertyExp, value);

                filter = (i == 0) ? filter.Start(propertyCompare)
                                    : filter.And(propertyCompare);
            }

            return set.Where(filter);
        }

        private Expression<Func<TEntity, bool>> CreatePropertyCompareExpression(LambdaExpression propertyExp, object value)
        {
            var param = propertyExp.Parameters[0];
            var propertyAcessor = propertyExp.Body as MemberExpression;
            var propertyType = (propertyAcessor.Member as PropertyInfo).PropertyType;

            var equalsExp = Expression.Equal(propertyAcessor, Expression.Constant(value, propertyType));

            return (Expression<Func<TEntity, bool>>)Expression.Lambda(equalsExp, param);
        }

        public TQuery FilterByKeys<TQuery>(TQuery set, object[] keyValues) where TQuery : IQueryable
        {
            return (TQuery)FilterByKeysTyped((IQueryable<TEntity>)set, keyValues);
        }
    }
}
