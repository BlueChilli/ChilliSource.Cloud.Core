using ChilliSource.Cloud.Core.DateTimeKindModels;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChilliSource.Cloud.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeKindAttribute : Attribute
    {
        private readonly DateTimeKind _kind;

        public DateTimeKindAttribute(DateTimeKind kind)
        {
            _kind = kind;
        }

        public DateTimeKind Kind
        {
            get { return _kind; }
        }

        public static void Apply(object entity)
        {
            if (entity == null)
                return;

            var metadata = MetadataFactory.GetForType(entity.GetType());
            metadata.ApplyAttributeAction(entity);
        }
    }
}

namespace ChilliSource.Cloud.Core.DateTimeKindModels
{
    internal class MetadataFactory
    {
        private readonly static ConcurrentDictionary<Type, IMetadataInfo> _dict = new ConcurrentDictionary<Type, IMetadataInfo>();

        public static IMetadataInfo GetForType(Type type)
        {
            if (type == null)
                throw new ApplicationException("Type cannot be null");

            return _dict.GetOrAdd(type, EntryCreator);
        }

        private static IMetadataInfo EntryCreator(Type type)
        {
            var entryType = typeof(MetadataInfo<>).MakeGenericType(type);

            //Not a problem to use reflection here as this will be called only once per type
            return (IMetadataInfo)Activator.CreateInstance(entryType, nonPublic: true);
        }
    }

    internal interface IMetadataInfo
    {
        Type Type { get; }
        Action<object> ApplyAttributeAction { get; }
    }

    internal class MetadataInfo<T> : IMetadataInfo
    {
        public Type Type { get; private set; }
        public Action<object> ApplyAttributeAction { get; private set; }

        internal MetadataInfo()
        {
            var properties = typeof(T).GetProperties()
                              .Where(x => x.GetIndexParameters().Length == 0 //indexed properties not supported
                                          && (x.PropertyType == typeof(DateTime) || x.PropertyType == typeof(DateTime?))
                                          && x.GetGetMethod() != null && x.GetSetMethod() != null) //property acessors
                              .Select(x => new { property = x, attribute = x.GetCustomAttribute<DateTimeKindAttribute>() })
                              .Where(a => a.attribute != null)
                              .ToList();

            var propertyActions = properties.Select(a => CreateActionForProperty(a.property, a.attribute)).ToArray();

            this.Type = typeof(T);
            this.ApplyAttributeAction = CombineActions(propertyActions);
        }

        private static Action<object> CombineActions(params Action<T>[] propertyActions)
        {
            if (propertyActions.Length == 0)
                return (object target) => { }; //empty action

            //You can place a breakpoint inside the action below
            return (object target) =>
            {
                var entity = (T)target;
                for (int i = 0; i < propertyActions.Length; i++)
                    propertyActions[i](entity);
            };
        }

        private static Action<T> CreateActionForProperty(PropertyInfo property, DateTimeKindAttribute attribute)
        {
            var kind = attribute.Kind;

            if (property.PropertyType == typeof(DateTime?))
            {
                //creates delegates only once, before the real execution
                var getDatetimeNullable = CreateGetterDelegate<DateTime?>(property);
                var setDatetimeNullable = CreateSetterDelegate<DateTime?>(property);

                //You can place a breakpoint inside the action below
                return (T entity) =>
                {
                    var date = getDatetimeNullable(entity);
                    if (date == null)
                        return;

                    setDatetimeNullable(entity, DateTime.SpecifyKind(date.Value, kind));
                };
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                //creates delegates only once, before the real execution
                var getDatetime = CreateGetterDelegate<DateTime>(property);
                var setDatetime = CreateSetterDelegate<DateTime>(property);

                //You can place a breakpoint inside the action below
                return (T entity) =>
                {
                    var date = getDatetime(entity);

                    setDatetime(entity, DateTime.SpecifyKind(date, kind));
                };
            }
            else
            {
                throw new NotSupportedException("DateTimeKindAttribute - property type: " + property.PropertyType.FullName);
            }
        }

        private static Func<T, TProperty> CreateGetterDelegate<TProperty>(PropertyInfo propertyInfo)
        {
            //Creates a Func<> equivalent to:
            //(T entity) => entity.PropertyX;

            if (propertyInfo.PropertyType != typeof(TProperty))
                throw new ApplicationException("Property type does not match.");

            var paramEntity = Expression.Parameter(typeof(T), "entity");
            var propertyExp = Expression.Property(paramEntity, propertyInfo);
            var lambdaExp = Expression.Lambda<Func<T, TProperty>>(propertyExp, paramEntity);

            return lambdaExp.Compile();
        }

        private static Action<T, TProperty> CreateSetterDelegate<TProperty>(PropertyInfo propertyInfo)
        {
            //Creates an Action<> equivalent to:
            //(T entity, TProperty value) => entity.PropertyX = value;

            if (propertyInfo.PropertyType != typeof(TProperty))
                throw new ApplicationException("Property type does not match.");

            var paramEntity = Expression.Parameter(typeof(T), "entity");
            var paramValue = Expression.Parameter(typeof(TProperty), "value");

            var setterExp = Expression.Call(paramEntity, propertyInfo.GetSetMethod(), paramValue);
            var lambdaExp = Expression.Lambda<Action<T, TProperty>>(setterExp, paramEntity, paramValue);

            return lambdaExp.Compile();
        }
    }
}