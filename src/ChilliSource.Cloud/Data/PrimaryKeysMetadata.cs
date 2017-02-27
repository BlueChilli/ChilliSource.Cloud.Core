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
    internal static class PrimaryKeysMetadataFactory<TEntity>
        where TEntity : class
    {
        public static IPrimaryKeysMetadata<TEntity> GetForContext(DbContext context)
        {
            return (IPrimaryKeysMetadata<TEntity>)Get((dynamic)context);
        }

        private static IPrimaryKeysMetadata<TEntity> Get<TDbContext>(TDbContext context)
            where TDbContext : DbContext
        {
            return PrimaryKeysMetadata<TDbContext, TEntity>.GetInstance(context);
        }
    }

    internal interface IPrimaryKeysMetadata<T>
    {
        object[] GetPrimaryKeys(T entity);
        Expression<Func<T, bool>> FilterByKeys(object[] keyValues);
    }

    internal class PrimaryKeysMetadata<TDbContext, TEntity> : IPrimaryKeysMetadata<TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        private static IPrimaryKeysMetadata<TEntity> _instance = null;
        private LambdaExpression[] _expressions;
        Func<TEntity, object>[] _keyGetters;

        private PrimaryKeysMetadata(Func<TEntity, object>[] keyGetters, LambdaExpression[] expressions)
        {
            _keyGetters = keyGetters;
            _expressions = expressions;
        }


        public static IPrimaryKeysMetadata<TEntity> GetInstance(TDbContext context)
        {
            if (_instance != null)
                return _instance;
            return (_instance = Init(context));
        }

        private static IPrimaryKeysMetadata<TEntity> Init(TDbContext context)
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

                return expression;
            }).ToArray();

            return new PrimaryKeysMetadata<TDbContext, TEntity>(getters, expressions);
        }

        private static IList<EdmMember> GetPrimaryKeysDefinition(DbContext context)
        {
            var objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<TEntity>();
            return objectSet.EntitySet.ElementType.KeyMembers;
        }

        public object[] GetPrimaryKeys(TEntity entity)
        {
            var result = new object[_keyGetters.Length];
            for (int i = 0; i < _keyGetters.Length; i++)
            {
                result[i] = _keyGetters[i].Invoke((TEntity)entity);
            }

            return result;
        }

        public Expression<Func<TEntity, bool>> FilterByKeys(object[] keyValues)
        {
            if (keyValues == null || keyValues.Length != _expressions.Length)
                return (TEntity e) => false;

            var filter = PredicateBuilder.New<TEntity>();

            for (int i = 0; i < _expressions.Length; i++)
            {
                var propertyExp = _expressions[i];
                var value = keyValues[i];

                var propertyCompare = CreatePropertyCompareExpression(propertyExp, value);

                filter = (i == 0) ? filter.Start(propertyCompare)
                                    : filter.And(propertyCompare);
            }

            return filter;
        }

        private Expression<Func<TEntity, bool>> CreatePropertyCompareExpression(LambdaExpression propertyExp, object value)
        {
            var param = propertyExp.Parameters[0];
            var propertyAcessor = propertyExp.Body as MemberExpression;
            var propertyType = (propertyAcessor.Member as PropertyInfo).PropertyType;

            var equalsExp = Expression.Equal(propertyAcessor, Expression.Constant(value, propertyType));

            return (Expression<Func<TEntity, bool>>)Expression.Lambda(equalsExp, param);
        }
    }
}
