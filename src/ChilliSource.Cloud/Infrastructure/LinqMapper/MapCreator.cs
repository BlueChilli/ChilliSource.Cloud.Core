using ChilliSource.Cloud.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Infrastructure.LinqMapper
{
    internal class MapperKey
    {
        int _hashCode;
        private MapperKey(Type tSource, Type tDest)
        {
            this.TSource = tSource;
            this.TDest = tDest;
            _hashCode = _GetHashCode();
        }

        public Type TSource { get; private set; }
        public Type TDest { get; private set; }

        private int _GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + TSource.GetHashCode();
                hash = hash * 23 + TDest.GetHashCode();
                return hash;
            }
        }
        public override int GetHashCode() { return _hashCode; }

        public override bool Equals(object obj)
        {
            var cObj = obj as MapperKey;
            if (cObj == null)
                return false;

            return cObj.TSource == this.TSource && cObj.TDest == this.TDest;
        }

        static readonly ConcurrentDictionary<Type, MapperKeysDestHolder> _SourceKeys = new ConcurrentDictionary<Type, MapperKeysDestHolder>();

        public static MapperKey Get(Type tSource, Type tDest)
        {
            MapperKeysDestHolder holder;
            if (!_SourceKeys.TryGetValue(tSource, out holder))
            {
                _SourceKeys[tSource] = holder = new MapperKeysDestHolder(tSource); //it's ok not to sync.
            }

            return holder.Get(tDest);
        }

        internal class MapperKeysDestHolder
        {
            readonly ConcurrentDictionary<Type, MapperKey> _DestKeys = new ConcurrentDictionary<Type, MapperKey>();

            public MapperKeysDestHolder(Type tSource)
            {
                this.TSource = tSource;
            }

            public Type TSource { get; private set; }

            public MapperKey Get(Type tDest)
            {
                MapperKey mapperKey;
                if (!_DestKeys.TryGetValue(tDest, out mapperKey))
                {
                    _DestKeys[tDest] = mapperKey = new MapperKey(this.TSource, tDest); //it's ok not to sync.
                }

                return mapperKey;
            }
        }
    }

    //Optimizes GetMap call to only process each MapperKey once.
    internal class GetMapCallContext
    {
        Dictionary<MapperKey, LambdaExpression> _cache = new Dictionary<MapperKey, LambdaExpression>(1);

        public LambdaExpression Set(MapperKey key, LambdaExpression value) { _cache[key] = value; return value; }
        public bool TryGet(MapperKey key, out LambdaExpression expression) { return _cache.TryGetValue(key, out expression); }
    }

    internal interface IMapCreator
    {
        IExpressionBuilder CreateUnexpandedMap(IObjectContext context);
    }

    internal interface IExtendedMapCreator : IMapCreator
    {
        void Add(IMapCreator mapCreator);
        void AddFirst(IMapCreator mapCreator);
        void IgnoreMembers(IList<string> ignoreMembers);
        void IgnoreRuntimeMembers(Func<IObjectContext, IEnumerable<string>> runtimeDelegate);
    }  

    internal class StaticMapCreator<TSource, TDest> : IMapCreator
        where TDest : class, new()
    {
        Expression<Func<TSource, TDest>> _expression;
        public StaticMapCreator(ParameterExpression sourceParameter, Expression<Func<TSource, TDest>> expression)
        {
            if (sourceParameter == null || expression == null)
                throw new ArgumentNullException("StaticMapCreator: sourceParameter || expression");

            //Save static map WITHOUT expanding 'InvokeMap' calls (which will happen in runtime)
            //Sets the source parameter during configuration set-up, so we don't need to replace it later.
            var paramReplacer = new SourceParamReplacer(expression.Parameters[0], sourceParameter);
            _expression = (Expression<Func<TSource, TDest>>)paramReplacer.Visit(expression);
        }

        public IExpressionBuilder CreateUnexpandedMap(IObjectContext context)
        {
            return new ExpressionBuilder<TSource, TDest>(_expression);
        }
    }

    internal class DefaultMapCreator<TSource, TDest> : IMapCreator
       where TDest : class, new()
    {
        List<string> _ignoreMembers;
        Lazy<Expression<Func<TSource, TDest>>> _staticMap;

        public DefaultMapCreator(ParameterExpression sourceParam, IList<string> ignoreMembers)
        {
            //Creates and stores the default source parameter, so it can be reused.
            this.SourceParameter = sourceParam;
            this._ignoreMembers = ignoreMembers.ToList();

            //Delays the map creation to the first CreateUnexpandedMap() call
            //This allows dependent .CreateMap<> rules to be created in any order.
            _staticMap = new Lazy<Expression<Func<TSource, TDest>>>(() => CreateDefaultMap());
        }

        public void IgnoreMembers(IList<string> ignoreMembers)
        {
            this._ignoreMembers = this._ignoreMembers.Concat(ignoreMembers).ToList();
            _staticMap = new Lazy<Expression<Func<TSource, TDest>>>(() => CreateDefaultMap());
        }

        private Expression<Func<TSource, TDest>> CreateDefaultMap()
        {
            var sourceType = typeof(TSource);
            var destType = typeof(TDest);

            var propertyBindings = new Dictionary<string, MemberAssignment>();
            foreach(var propertyBinding in LinqMapper.GetPropertyBindings(sourceType, destType, this.SourceParameter)
                                            .Where(m => !this._ignoreMembers.Contains(m.Member.Name)))
            {
                propertyBindings.Add(propertyBinding.Member.Name, propertyBinding);
            }

            var newExp = ExpressionNewCreator<TDest>.NewExpression;
            var body = (Expression)Expression.MemberInit(newExp, propertyBindings.Values);

            return Expression.Lambda<Func<TSource, TDest>>(body, this.SourceParameter);
        }

        private ParameterExpression SourceParameter { get; set; }

        public IExpressionBuilder CreateUnexpandedMap(IObjectContext context)
        {
            return new ExpressionBuilder<TSource, TDest>(_staticMap.Value);
        }
    }

    internal class RuntimeMapCreator<TContext, TSource, TDest> : IMapCreator
     where TContext : class
     where TDest : class, new()
    {
        Func<TContext, Expression<Func<TSource, TDest>>> _runtimeMapCreator;

        public RuntimeMapCreator(Func<TContext, Expression<Func<TSource, TDest>>> runtimeMapCreator)
        {
            if (runtimeMapCreator == null)
                throw new ArgumentNullException("runtimeMapCreator");

            _runtimeMapCreator = runtimeMapCreator;
        }

        public IExpressionBuilder CreateUnexpandedMap(IObjectContext context)
        {
            var contextValue = context?.GetContext<TContext>();
            var runtimeMap = _runtimeMapCreator(contextValue);

            return new ExpressionBuilder<TSource, TDest>(runtimeMap);
        }
    }

    internal class CastBaseMapCreator<TBaseSource, TBaseDest, TSource, TDest> : IMapCreator
      where TDest : class, new()
    {
        MapperKey _basekey;
        IExtendedMapCreator _baseMapCreator = null;

        public CastBaseMapCreator()
        {
            _basekey = MapperKey.Get(typeof(TBaseSource), typeof(TBaseDest));
        }

        private IMapCreator GetBaseMapCreator()
        {
            if (_baseMapCreator != null)
                return _baseMapCreator;

            return (_baseMapCreator = LinqMapper.GetMapCreator(_basekey));
        }

        public IExpressionBuilder CreateUnexpandedMap(IObjectContext context)
        {
            var map = GetBaseMapCreator().CreateUnexpandedMap(context);
            return map.CastTo<TSource, TDest>();
        }
    }

    internal interface IExtendedAction
    {
        IExpressionBuilder Run(IExpressionBuilder exp, IObjectContext context);
    }

    internal class MapCreatorExtendedAction : IExtendedAction
    {
        IMapCreator _mapCreator;
        public MapCreatorExtendedAction(IMapCreator mapCreator)
        {
            _mapCreator = mapCreator;
        }

        public IExpressionBuilder Run(IExpressionBuilder exp, IObjectContext context)
        {
            var next = _mapCreator.CreateUnexpandedMap(context);
            exp.ExtendWith(next);

            return exp;
        }
    }

    internal class IgnoreMembersExtendedAction : IExtendedAction
    {
        Func<IObjectContext, IEnumerable<string>> _runtimeDelegate;
        public IgnoreMembersExtendedAction(Func<IObjectContext, IEnumerable<string>> runtimeDelegate)
        {
            _runtimeDelegate = runtimeDelegate;
        }

        public IExpressionBuilder Run(IExpressionBuilder exp, IObjectContext context)
        {
            var members = _runtimeDelegate(context).ToArray();
            exp.RemoveMembers(members);

            return exp;
        }
    }

    internal class ExtendedMapCreator<TSource, TDest> : IExtendedMapCreator
        where TDest : class, new()
    {
        DefaultMapCreator<TSource, TDest> _defaultCreator;
        List<IExtendedAction> _extendedActions = new List<IExtendedAction>();

        public ExtendedMapCreator(Expression<Func<TSource, TDest>> customMapping = null)
        {
            IList<string> ignoreMembers;
            ParameterExpression sourceParam = null;
            if (customMapping != null)
            {
                sourceParam = customMapping.Parameters[0];
                ignoreMembers = LinqMapper.GetBindingExpressions(customMapping).Select(e => e.Member.Name).ToList();
            }
            else
            {
                sourceParam = Expression.Parameter(typeof(TSource), "src");
                ignoreMembers = ArrayExtensions.EmptyArray<string>();
            }

            _defaultCreator = new DefaultMapCreator<TSource, TDest>(sourceParam, ignoreMembers);

            if (customMapping != null)
            {
                this.Add(new StaticMapCreator<TSource, TDest>(sourceParam, customMapping));
            }
        }

        public void Add(IMapCreator mapCreator)
        {
            _extendedActions.Add(new MapCreatorExtendedAction(mapCreator));
        }

        public void AddFirst(IMapCreator mapCreator)
        {
            _extendedActions.Insert(0, new MapCreatorExtendedAction(mapCreator));
        }

        public void IgnoreMembers(IList<string> ignoreMembers)
        {
            //Static ignore, so remove from the default creator.
            _defaultCreator.IgnoreMembers(ignoreMembers);

            var members = ignoreMembers.ToArray();
            this.IgnoreRuntimeMembers((ctx) => members);
        }

        public void IgnoreRuntimeMembers(Func<IObjectContext, IEnumerable<string>> runtimeDelegate)
        {
            _extendedActions.Add(new IgnoreMembersExtendedAction(runtimeDelegate));
        }

        public IExpressionBuilder CreateUnexpandedMap(IObjectContext context)
        {
            var map = _defaultCreator.CreateUnexpandedMap(context);

            if (_extendedActions.Count == 0)
                return map;

            var length = _extendedActions.Count;
            for (int i = 0; i < length; i++)
            {
                map = _extendedActions[i].Run(map, context);
            }

            return map;
        }
    }

    internal static class ExpressionNewCreator<T>
    {
        private static readonly NewExpression _newInstance = Expression.New(typeof(T));
        public static NewExpression NewExpression { get { return _newInstance; } }
    }
}
