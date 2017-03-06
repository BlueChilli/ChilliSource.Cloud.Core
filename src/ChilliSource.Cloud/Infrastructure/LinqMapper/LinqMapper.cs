using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LinqKit;
using System.Collections.Concurrent;
using ChilliSource.Cloud.Extensions;

namespace ChilliSource.Cloud.Infrastructure.LinqMapper
{
    /// <summary>
    /// Allows customisations of Linq Maps.
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TDest">The destination type</typeparam>
    public interface ILinqMapperSyntax<TSource, TDest>
    {
        /// <summary>
        /// Creates a map that will be resolved in runtime using a typed context object.
        /// </summary>
        /// <typeparam name="TContext">The context type</typeparam>
        /// <param name="runtimeMapCreator">A delegate to create a map in runtime</param>
        /// <returns>The linq mapper syntax</returns>
        ILinqMapperSyntax<TSource, TDest> CreateRuntimeMap<TContext>(Func<TContext, Expression<Func<TSource, TDest>>> runtimeMapCreator)
            where TContext : class;

        /// <summary>
        /// Creates a map that will be resolved in runtime.
        /// </summary>
        /// <param name="runtimeMapCreator">A delegate to create a map in runtime</param>
        /// <returns>The linq mapper syntax</returns>
        ILinqMapperSyntax<TSource, TDest> CreateRuntimeMap(Func<IMaterializerContext, Expression<Func<TSource, TDest>>> runtimeMapCreator);

        /// <summary>
        /// Includes all properties from a base map, except those defined in this map.
        /// </summary>
        /// <typeparam name="TBaseSource">The base source type</typeparam>
        /// <typeparam name="TBaseDest">The base destination type</typeparam>
        /// <returns>The linq mapper syntax</returns>
        ILinqMapperSyntax<TSource, TDest> IncludeBase<TBaseSource, TBaseDest>();

        /// <summary>
        /// Ignores the default mapping behaviour for one or more members.
        /// </summary>
        /// <param name="members">Members to be ignored</param>
        /// <returns>The linq mapper syntax</returns>
        ILinqMapperSyntax<TSource, TDest> IgnoreMembers(params Expression<Func<TDest, object>>[] members);

        /// <summary>
        /// Ignores the default mapping behaviour for one or more members.
        /// </summary>
        /// <param name="members">Members to be ignored</param>
        /// <returns>The linq mapper syntax</returns>
        ILinqMapperSyntax<TSource, TDest> IgnoreMembers(params string[] members);

        /// <summary>
        /// Runs a delegate in runtime to decide which members to ignore
        /// </summary>
        /// <param name="runtimeDelegate">delegate that provides members to be ignored</param>
        /// <returns>The linq mapper syntax</returns>
        ILinqMapperSyntax<TSource, TDest> IgnoreRuntimeMembers<TContext>(Func<TContext, IEnumerable<string>> runtimeDelegate);

        /// <summary>
        /// Runs a delegate in runtime to decide which members to ignore
        /// </summary>
        /// <param name="runtimeDelegate">delegate that provides members to be ignored</param>
        /// <returns>The linq mapper syntax</returns>
        ILinqMapperSyntax<TSource, TDest> IgnoreRuntimeMembers(Func<IMaterializerContext, IEnumerable<string>> runtimeDelegate);
    }

    /// <summary>
    /// Container for Linq Map Expressions.
    /// </summary>
    public static class LinqMapper
    {
        private static ConcurrentDictionary<MapperKey, IExtendedMapCreator> _Maps = new ConcurrentDictionary<MapperKey, IExtendedMapCreator>();

        internal class LinqMapperSyntax<TSource, TDest> : ILinqMapperSyntax<TSource, TDest>
            where TDest : class, new()
        {
            MapperKey _key;
            public LinqMapperSyntax(MapperKey key)
            {
                _key = key;
            }

            public ILinqMapperSyntax<TSource, TDest> CreateRuntimeMap(Func<IMaterializerContext, Expression<Func<TSource, TDest>>> runtimeMapCreator)
            {
                return this.CreateRuntimeMap<IMaterializerContext>(runtimeMapCreator);
            }

            public ILinqMapperSyntax<TSource, TDest> CreateRuntimeMap<TContext>(Func<TContext, Expression<Func<TSource, TDest>>> runtimeMapCreator)
                where TContext : class
            {
                if (runtimeMapCreator == null)
                    return this;

                var map = LinqMapper.GetMapCreator(_key);
                var runtimeMap = new RuntimeMapCreator<TContext, TSource, TDest>(runtimeMapCreator);
                map.Add(runtimeMap);

                return this;
            }

            public ILinqMapperSyntax<TSource, TDest> IgnoreMembers(params Expression<Func<TDest, object>>[] members)
            {
                if (members == null || members.Length == 0)
                    return this;

                var memberNames = members.Select(m => GetMemberName(m)).ToList();
                var map = LinqMapper.GetMapCreator(_key);
                map.IgnoreMembers(memberNames);

                return this;
            }

            public ILinqMapperSyntax<TSource, TDest> IgnoreMembers(params string[] members)
            {
                var map = LinqMapper.GetMapCreator(_key);
                map.IgnoreMembers(members);

                return this;
            }

            public ILinqMapperSyntax<TSource, TDest> IgnoreRuntimeMembers<TContext>(Func<TContext, IEnumerable<string>> runtimeIgnore)
            {
                var runtimeDelegate = TransformIgnoreRuntime(runtimeIgnore);
                return this.IgnoreRuntimeMembers(runtimeDelegate);
            }

            public ILinqMapperSyntax<TSource, TDest> IgnoreRuntimeMembers(Func<IMaterializerContext, IEnumerable<string>> runtimeIgnore)
            {
                if (runtimeIgnore == null)
                    throw new ArgumentNullException("runtimeDelegate");

                var map = LinqMapper.GetMapCreator(_key);
                map.IgnoreRuntimeMembers(runtimeIgnore);

                return this;
            }

            private static Func<IMaterializerContext, IEnumerable<string>> TransformIgnoreRuntime<TContext>(Func<TContext, IEnumerable<string>> runtimeDelegate)
            {
                if (runtimeDelegate == null)
                    return null;

                return (IMaterializerContext ctx) =>
                {
                    var value = ctx.GetContext<TContext>();
                    return runtimeDelegate(value);
                };
            }

            private string GetMemberName(Expression<Func<TDest, object>> exp)
            {
                var memberExpression = (exp?.Body as UnaryExpression)?.Operand as MemberExpression ??
                                        (exp?.Body as MemberExpression);
                var propertyName = (memberExpression?.Member as PropertyInfo).Name
                                    ?? (memberExpression?.Member as FieldInfo).Name;

                if (propertyName == null)
                    throw new ApplicationException($"Member name not found in expression: {exp?.ToString()}");

                return propertyName;
            }

            public ILinqMapperSyntax<TSource, TDest> IncludeBase<TBaseSource, TBaseDest>()
            {
                if (!typeof(TBaseSource).IsAssignableFrom(typeof(TSource)) || !typeof(TBaseDest).IsAssignableFrom(typeof(TDest))
                    || (typeof(TBaseSource) == typeof(TSource) && typeof(TBaseDest) == typeof(TDest)))
                {
                    throw new ArgumentException($"One or more base types [{typeof(TBaseSource).FullName}, {typeof(TBaseDest).FullName}] are not compatible with the current map types [{typeof(TSource).FullName}, {typeof(TDest).FullName}].");
                }

                var map = LinqMapper.GetMapCreator(_key);
                var baseMap = new CastBaseMapCreator<TBaseSource, TBaseDest, TSource, TDest>();

                //Base map happens first
                map.AddFirst(baseMap);

                return this;
            }
        }

        /// <summary>
        /// Retrieves a map expression previously created by CreateMap().
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The destination type</typeparam>
        /// <returns>A map expression</returns>
        public static Expression<Func<TSource, TDest>> GetMap<TSource, TDest>(IMaterializerContext context = null)
        {
            return (Expression<Func<TSource, TDest>>)GetMap(typeof(TSource), typeof(TDest), context);
        }

        public static IMaterializerContext CreateContext()
        {
            return new ContextContainer();
        }

        /// <summary>
        /// Retrieves a map expression previously created by CreateMap().
        /// </summary>
        /// <param name="tSource">The source type</param>
        /// <param name="tDest">The destination type</param>
        /// <returns>A map expression</returns>
        public static LambdaExpression GetMap(Type tSource, Type tDest, IMaterializerContext context = null)
        {
            return GetMapInternal(tSource, tDest, resetCallContext: true, context: context);
        }

        internal static LambdaExpression GetMapInternal(Type tSource, Type tDest, bool resetCallContext, IMaterializerContext context = null)
        {
            var key = MapperKey.Get(tSource, tDest);
            LambdaExpression expression = null;
            if (context == null)
                context = CreateContext();

            GetMapCallContext callContext = null;
            if (resetCallContext || !context.TryGetContext<GetMapCallContext>(out callContext))
                context.SetContext<GetMapCallContext>(callContext = new GetMapCallContext());

            if (callContext.TryGet(key, out expression))
                return expression;

            var mapCreator = GetMapCreator(key);
            var expBuilder = mapCreator.CreateUnexpandedMap(context);
            var exp = expBuilder.Build();
            expression = ExpandInternal(exp, context);

            return callContext.Set(key, expression);
        }

        internal static IExtendedMapCreator GetMapCreator(MapperKey key)
        {
            IExtendedMapCreator mapCreator;
            if (!LinqMapper._Maps.TryGetValue(key, out mapCreator))
            {
                throw new ApplicationException(String.Format("Linq map not found for types [{0}; {1}]", key.TSource.FullName, key.TDest.FullName));
            }

            return mapCreator;
        }

        /// <summary>
        /// Creates a dynamic lambda expression that maps one type to another. Use GetMap() to obtain the expression. 
        /// <para />Mapping conventions: 
        /// <para />* Auto-maps properties with matching names.
        /// <para />* Auto-maps second level properties prefixed with the first level property name. (i.e. dest.OrganisationName matched to source.Organisation.Name )
        /// <para />* When auto-mapping, the property types must match or there must be an existing map for them, otherwise the property will be ignored.
        /// <para />* Mapping conventions can be overriden via customMapping parameter.
        /// <para />* Expressions can be reused by calling InvokeMap() when overriding a property. (e.g. (TSource s) => new TDest(){ Property = s.AnotherProperty.InvokeMap&lt;Ta,Tb&gt;() } )   
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The destination type</typeparam>
        /// <param name="customMapping">A mapping expression which overrides the default mapping conventions for each property. <para /> (e.g. LinqMapper.CreateMap&lt;Tx,Ty&gt;((Tx x) => new Ty(){ Id = x.Id + 1 }); )</param>        
        public static ILinqMapperSyntax<TSource, TDest> CreateMap<TSource, TDest>(Expression<Func<TSource, TDest>> customMapping = null)
            where TDest : class, new()
        {
            var key = MapperKey.Get(typeof(TSource), typeof(TDest));

            if (_Maps.ContainsKey(key))
            {
                throw new ApplicationException(String.Format("Linq map already exists for types [{0}; {1}]", key.TSource.FullName, key.TDest.FullName));
            }

            _Maps[key] = new ExtendedMapCreator<TSource, TDest>(customMapping);
            return new LinqMapperSyntax<TSource, TDest>(key);
        }

        /// <summary>
        ///     Extends a Linq expression by replacing or appending property expressions.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The destination type</typeparam>
        /// <param name="map">The original Linq expression</param>
        /// <param name="extendedMap">The extended Linq expression which will override the property expressions of the original expression.</param>
        /// <returns></returns>
        public static Expression<Func<TSource, TDest>> ExtendMap<TSource, TDest>(Expression<Func<TSource, TDest>> map, Expression<Func<TSource, TDest>> extendedMap, IMaterializerContext context = null)
            where TDest : class, new()
        {
            if (map == null && extendedMap == null)
                return null;

            if (map == null)
                map = (TSource src) => new TDest();

            if (extendedMap == null)
                return map;

            var builder = new ExpressionBuilder<TSource, TDest>(map);
            builder.ExtendWith(new ExpressionBuilder<TSource, TDest>(extendedMap));

            var lambda = builder.Build();

            return Expand(lambda, context);
        }

        /// <summary>
        ///   Expands any .InvokeMap() call in the Linq Expression.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The destination type</typeparam>
        /// <param name="expression">Linq expression</param>
        /// <returns>The expanded linq expression</returns>
        public static Expression<Func<TSource, TDest>> Expand<TSource, TDest>(Expression<Func<TSource, TDest>> expression, IMaterializerContext context = null)
        {
            return (Expression<Func<TSource, TDest>>)ExpandInternal(expression, context);
        }

        internal static LambdaExpression ExpandInternal(LambdaExpression expression, IMaterializerContext context = null)
        {
            while (true)
            {
                var visitor = new TransformInvokeMapExpression(context);
                expression = (LambdaExpression)visitor.Visit(expression);

                if (!visitor.ExpressionWasReplaced)
                    return (LambdaExpression)expression.Expand();
            }
        }

        internal static IEnumerable<MemberAssignment> GetPropertyBindings(Type sourceType, Type destType, ParameterExpression sourceParamExp, Dictionary<string, MemberAssignment> existingBindings = null)
        {
            var sourcePropertyExps = getSourceProperties(sourceType, sourceParamExp);

            var setProperties = destType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite == true);

            foreach (var destPropertyInfo in setProperties)
            {
                if (existingBindings != null && existingBindings.ContainsKey(destPropertyInfo.Name))
                {
                    yield return existingBindings[destPropertyInfo.Name];
                }
                else if (sourcePropertyExps.ContainsKey(destPropertyInfo.Name))
                {
                    var sourcePropertyExp = sourcePropertyExps[destPropertyInfo.Name];
                    MemberAssignment memberAssignment = null;
                    if (CreatePropertyBinding(sourcePropertyExp, destPropertyInfo, out memberAssignment))
                    {
                        yield return memberAssignment;
                    }
                }
            }
        }

        private static Dictionary<string, MemberExpression> getSourceProperties(Type sourceType, Expression sourceExp, string prefix = null, int depth = 0)
        {
            var sourcePropertyInfos = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public)
                                                .ToDictionary(p => prefix + p.Name, p => Expression.Property(sourceExp, p));

            if (depth < 1)
            {
                foreach (var sourcePropertyInfo in sourcePropertyInfos.ToList())
                {
                    var propertyType = ((PropertyInfo)sourcePropertyInfo.Value.Member).PropertyType;

                    if (IsPrimitive(propertyType) || typeof(IEnumerable).IsAssignableFrom(propertyType))
                        continue;

                    var secondLevelProperties = getSourceProperties(propertyType, sourcePropertyInfo.Value, prefix: sourcePropertyInfo.Key, depth: depth + 1);
                    foreach (var secondlevelProperty in secondLevelProperties)
                    {
                        if (!sourcePropertyInfos.ContainsKey(secondlevelProperty.Key))
                            sourcePropertyInfos[secondlevelProperty.Key] = secondlevelProperty.Value;
                    }
                }
            }

            return sourcePropertyInfos;
        }

        private static bool CreatePropertyBinding(MemberExpression sourcePropertyExp, PropertyInfo destPropertyInfo, out MemberAssignment memberAssignment)
        {
            memberAssignment = null;
            var sourcePropertyType = ((PropertyInfo)sourcePropertyExp.Member).PropertyType;
            var destPropertyType = destPropertyInfo.PropertyType;

            var mapping = TryCreateMappingExpression(sourcePropertyExp, sourcePropertyType, destPropertyType);
            if (mapping != null)
                memberAssignment = Expression.Bind(destPropertyInfo, mapping);

            return memberAssignment != null;
        }

        private static Expression TryCreateMappingExpression(Expression sourceExpression, Type sourceType, Type destType)
        {
            var key = MapperKey.Get(sourceType, destType);
            if (_Maps.ContainsKey(key))
            {
                //Dynamically creates expression: sourceExpression.InvokeMap<TSource, TDest>() 
                var invokeMapMethodInfo = invokeMapTemplate.MakeGenericMethod(key.TSource, key.TDest);

                return Expression.Call(invokeMapMethodInfo, sourceExpression);
            }
            else if (sourceType == destType && IsPrimitive(sourceType))
            {
                //the sourceExpression is compatible with the destination
                return sourceExpression;
            }

            return TryCreateMappingNullableDest(sourceExpression, sourceType, destType) ??
                   TryCreateMappingNullableSource(sourceExpression, sourceType, destType) ??
                   TryCreateMappingCollection(sourceExpression, sourceType, destType);
        }

        private static Expression TryCreateMappingNullableDest(Expression sourceExpression, Type sourceType, Type destType)
        {
            if (!destType.IsValueType)
                return null;

            var innerValueType = Nullable.GetUnderlyingType(destType);
            if (sourceType == innerValueType)
            {
                //the sourceExpression can be safely converted to dest: (T?)dest = Convert(T)
                return Expression.Convert(sourceExpression, destType);
            }

            return null;
        }

        private static Expression TryCreateMappingNullableSource(Expression sourceExpression, Type sourceType, Type destType)
        {
            if (!sourceType.IsValueType)
                return null;

            //gets T out of Nullable<T>, if it exists
            var innerValueType = Nullable.GetUnderlyingType(sourceType);
            if (innerValueType == null)
                return null;

            //creates coalesce exp: expression ?? default(T)
            var defaultValue = Activator.CreateInstance(innerValueType);
            var newSource = Expression.Coalesce(sourceExpression, Expression.Constant(defaultValue, innerValueType));

            return TryCreateMappingExpression(newSource, innerValueType, destType);
        }

        static readonly Type OpenIEnumerableType = typeof(IEnumerable<>);
        static readonly Type OpenIListType = typeof(IList<>);

        private static Expression TryCreateMappingCollection(Expression sourceExpression, Type sourceType, Type destType)
        {
            Type closedEnumerable;
            Type closedList;
            var sourceProperty = (sourceExpression as MemberExpression)?.Member as PropertyInfo;

            //enumerableParamType = T from IEnumerable<T>
            var enumerableParamType = GetClosedGenericArguments(sourceType, OpenIEnumerableType, out closedEnumerable).FirstOrDefault();
            if (closedEnumerable == null)
                return null;

            //listParamType = T from IList<T>
            var listParamType = GetClosedGenericArguments(destType, OpenIListType, out closedList).FirstOrDefault();
            var isList = destType.IsGenericType && destType.GetGenericTypeDefinition().Equals(typeof(List<>));
            var isArray = destType.IsArray;
            if (closedList == null || (!isList && !isArray))
                return null;

            var sourceParam = (sourceProperty != null) ? Expression.Parameter(enumerableParamType, "p_" + sourceProperty.Name)
                                    : Expression.Parameter(enumerableParamType);

            var elementMapping = TryCreateMappingExpression(sourceParam, enumerableParamType, listParamType);
            if (elementMapping == null)
                return null;

            var lambdaElement = Expression.Lambda(elementMapping, sourceParam);
            var listMapping = isList ? (LambdaExpression)createToListTemplate.MakeGenericMethod(enumerableParamType, listParamType).Invoke(null, new object[] { lambdaElement })
                                    : (LambdaExpression)createToArrayTemplate.MakeGenericMethod(enumerableParamType, listParamType).Invoke(null, new object[] { lambdaElement });


            var invokeMethodInfo = TransformInvokeMapExpression.LinqKitInvokeTemplate.MakeGenericMethod(closedEnumerable, destType);
            var invokeExp = (Expression)Expression.Call(invokeMethodInfo, listMapping, sourceExpression);
            var result = invokeExp.Expand();

            return result;
        }

        private static readonly MethodInfo createToListTemplate = typeof(LinqMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                                                         .Where(method => method.Name == "CreateCollectionToList" && method.GetGenericArguments().Length == 2).FirstOrDefault();
        private static readonly MethodInfo createToArrayTemplate = typeof(LinqMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                                                         .Where(method => method.Name == "CreateCollectionToArray" && method.GetGenericArguments().Length == 2).FirstOrDefault();

        private static Expression<Func<IEnumerable<TSource>, List<TDest>>> CreateCollectionToList<TSource, TDest>(
                                Expression<Func<TSource, TDest>> mappingExpression)
        {
            return (IEnumerable<TSource> collection) => collection.AsQueryable().Select(mappingExpression).ToList();
        }

        private static Expression<Func<IEnumerable<TSource>, TDest[]>> CreateCollectionToArray<TSource, TDest>(
                                Expression<Func<TSource, TDest>> mappingExpression)
        {
            return (IEnumerable<TSource> collection) => collection.AsQueryable().Select(mappingExpression).ToArray();
        }

        private static IEnumerable<Type> GetInterfacesAndBaseTypes(Type type)
        {
            yield return type;

            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces) { yield return iface; }

            var baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        private static Type[] GetClosedGenericArguments(Type type, Type openGenericType, out Type closedGenericType)
        {
            closedGenericType = null;
            if (type == null)
                return ArrayExtensions.EmptyArray<Type>();

            foreach (var baseType in GetInterfacesAndBaseTypes(type).Where(t => t.IsGenericType))
            {
                var genericDefinition = baseType.GetGenericTypeDefinition();
                if (genericDefinition == openGenericType)
                {
                    closedGenericType = baseType;
                    return baseType.GetGenericArguments();
                }
            }

            return ArrayExtensions.EmptyArray<Type>();
        }

        private static bool IsPrimitive(Type type)
        {
            return type.IsValueType || type == typeof(string);
        }

        internal static IEnumerable<MemberAssignment> GetBindingExpressions<TSource, TDest>(Expression<Func<TSource, TDest>> customMapping)
        {
            var memberInit = customMapping.Body as MemberInitExpression;
            if (memberInit == null)
                return Enumerable.Empty<MemberAssignment>();

            return memberInit.Bindings.OfType<MemberAssignment>();
        }

        internal static Dictionary<string, MemberAssignment> GetBindingExpressionsDictionary<TSource, TDest>(Expression<Func<TSource, TDest>> customMapping)
        {
            var memberInit = customMapping.Body as MemberInitExpression;
            if (memberInit == null)
                return new Dictionary<string, MemberAssignment>(0);

            var bindings = memberInit.Bindings;
            var dict = new Dictionary<string, MemberAssignment>(bindings.Count);
            
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i] as MemberAssignment;
                if (binding != null)
                {
                    dict.Add(binding.Member.Name, binding);
                }
            }
            return dict;
        }

        internal static SortedList<string, MemberAssignment> GetBindingExpressionsSortedList<TSource, TDest>(Expression<Func<TSource, TDest>> customMapping)
        {
            var memberInit = customMapping.Body as MemberInitExpression;
            if (memberInit == null)
                return new SortedList<string, MemberAssignment>(0);

            var bindings = memberInit.Bindings;
            var list = new SortedList<string, MemberAssignment>(bindings.Count);
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i] as MemberAssignment;
                if (binding != null)
                {
                    list.Add(binding.Member.Name, binding);
                }
            }
            return list;
        }

        internal static readonly MethodInfo invokeMapTemplate = typeof(LinqMapper).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                    .Where(method => method.Name == "InvokeMap" && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 1).FirstOrDefault();

        internal static readonly MethodInfo invokeRuntimeMapTemplate = typeof(LinqMapper).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                        .Where(method => method.Name == "InvokeMap" && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2).FirstOrDefault();

        /// <summary>
        /// <para>Invokes an existing map previously created by LinqMapper.Create().</para>
        /// <para>This method should ONLY be called inside a Linq query. It serves as a placeholder and will always throw an exception.</para>
        /// </summary>
        public static TDest InvokeMap<TSource, TDest>(this TSource expression)
        {
            throw new ApplicationException("This method cannot be called outside a linq query.");
        }

        /// <summary>
        /// This method should ONLY be called inside a Linq query. It serves as a placeholder and will always throw an exception.      
        /// </summary>
        public static TDest InvokeMap<TSource, TDest>(this TSource expression, Expression<Func<TSource, TDest>> mapExpression)
        {
            throw new ApplicationException("This method cannot be called outside a linq query.");
        }

        /// <summary>
        /// Removes all maps.
        /// </summary>
        public static void Reset()
        {
            _Maps.Clear();
        }
    }

    // transforms a InvokeMap() call to a LinqKit.Extensions.Invoke call
    internal class TransformInvokeMapExpression : System.Linq.Expressions.ExpressionVisitor
    {
        IMaterializerContext _context;
        public TransformInvokeMapExpression(IMaterializerContext context)
        {
            _context = context;
        }

        public bool ExpressionWasReplaced { get; set; }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (isInvokeMapExpression(expression))
            {
                var invokeMapExp = expression as MethodCallExpression;
                var innerExp = invokeMapExp.Arguments[0]; // e.g. src.Member.Property
                var genericArguments = invokeMapExp.Method.GetGenericArguments();

                var existingMap = invokeMapExp.Arguments.Count == 1 ? LinqMapper.GetMapInternal(genericArguments[0], genericArguments[1], resetCallContext: false, context: _context)
                                    : invokeMapExp.Arguments[1];

                var invokeMethodInfo = LinqKitInvokeTemplate.MakeGenericMethod(genericArguments[0], genericArguments[1]);
                var invokeExp = Expression.Call(invokeMethodInfo, existingMap, innerExp);
                this.ExpressionWasReplaced = true;

                return invokeExp.Expand();
            }

            return base.VisitMethodCall(expression);
        }

        public static readonly MethodInfo LinqKitInvokeTemplate = typeof(LinqKit.Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                         .Where(method => method.Name == "Invoke" && method.GetGenericArguments().Length == 2).FirstOrDefault();

        public static readonly Type LinqMapperType = typeof(LinqMapper);

        private static bool isInvokeMapExpression(MethodCallExpression callExp)
        {
            var method = callExp.Method;
            if (method.DeclaringType != LinqMapperType || !method.IsGenericMethod)
                return false;

            var genericMethod = method.GetGenericMethodDefinition();
            return genericMethod.Equals(LinqMapper.invokeMapTemplate) || genericMethod.Equals(LinqMapper.invokeRuntimeMapTemplate);
        }
    }
}
