using ChilliSource.Cloud.Infrastructure.LinqMapper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Infrastructure.Materializer
{
    public enum TreeTraversal
    {
        RootFirst = 1,
        ChildrenFirst
    }

    /// <summary>
    /// Allows the execution of pre-defined actions on projections obtained via query.Materialze&lt;,&gt;() method.
    /// </summary>
    public static class Materializer
    {
        static ConcurrentDictionary<Type, IRecursiveAction> _actionInfos = new ConcurrentDictionary<Type, IRecursiveAction>();

        static Materializer()
        {
            //For compatibility
            Materializer.TreeTraversal = TreeTraversal.ChildrenFirst;
        }

        /// <summary>
        /// Defines the visiting order for objects.
        /// </summary>
        public static TreeTraversal TreeTraversal { get; set; }

        /// <summary>
        /// Register an action (with a specific context type) on a target type to be executed after mapping query objects.
        /// </summary>
        /// <typeparam name="T">The target type</typeparam>
        /// <typeparam name="TContext">The context type</typeparam>
        /// <param name="action">An action delagate</param>
        public static void RegisterAfterMap<T, TContext>(Action<T, TContext> action)
        {
            var type = typeof(T);
            if (_actionInfos.ContainsKey(type))
                throw new ApplicationException($"Type [{type.FullName}] already registered.");

            _actionInfos[type] = new RecursiveActionWrapper<T, TContext>(action);
        }

        /// <summary>
        /// Register an action on a target type to be executed after mapping query objects.
        /// </summary>
        /// <typeparam name="T">The target type</typeparam>
        /// <param name="action">An action delagate</param>
        public static void RegisterAfterMap<T>(Action<T> action)
        {
            var type = typeof(T);
            if (_actionInfos.ContainsKey(type))
                throw new ApplicationException($"Type [{type.FullName}] already registered.");

            _actionInfos[type] = new RecursiveActionWrapper<T>(action);
        }

        /// <summary>
        /// <para>Applies a previously registered action on a object. The contextCotainer object MUST contain any context objects required by the registered action.</para>
        /// <para>Registered actions on interfaces and base classes are also automaticaly applied in this order: Interface actions, Base Class (top to bottom) actions, Target type action.</para>
        /// </summary>
        /// <typeparam name="T">The target type</typeparam>
        /// <param name="item">The target instance</param>
        /// <param name="contextContainer">A context container created by LinqMapper.CreateContext() or null</param>
        /// <returns>The target instance</returns>
        public static T ApplyAfterMap<T>(T item, IObjectContext contextContainer = null)
        {
            if (item == null)
                return item;

            contextContainer = contextContainer ?? LinqMapper.LinqMapper.CreateContext();
            var runtimeInfo = InternalCache.GetRuntimeInfo(typeof(T));
            if (runtimeInfo != null)
            {
                //make sure to dispose tracker
                using (var tracker = new MaterializerTracker())
                {
                    if (Materializer.TreeTraversal == TreeTraversal.ChildrenFirst)
                    {
                        runtimeInfo.ApplyOn(item, contextContainer, tracker);
                    }
                    else
                    {
                        runtimeInfo.ApplyOnRootFirst(item, contextContainer, tracker);
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// Removes all after map actions
        /// </summary>
        public static void Reset()
        {
            InternalCache.Reset();
            _actionInfos.Clear();
        }

        private static IRecursiveAction GetRuntimeActionForType(Type type)
        {
            var actions = GetApplicableActions(type).ToArray();
            if (actions.Length == 0)
                return null;
            else if (actions.Length == 1)
                return actions[0];
            else
                return new CompositeActionWrapper(actions);
        }

        private static IEnumerable<IRecursiveAction> GetApplicableActions(Type type)
        {
            // action Order -> Interfaces, Base Types, Concrete type
            foreach (var baseType in GetInterfacesAndBaseTypes(type).Reverse())
            {
                IRecursiveAction actionInfo;
                if (_actionInfos.TryGetValue(baseType, out actionInfo))
                    yield return actionInfo;
            }
        }

        private static IEnumerable<Type> GetInterfacesAndBaseTypes(Type type)
        {
            yield return type;

            var baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }

            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces) { yield return iface; }
        }

        internal interface IRecursiveAction
        {
            void ApplyAction(object target, IObjectContext context);
        }

        internal class RecursiveActionWrapper<T> : IRecursiveAction
        {
            Action<T> _action;
            public RecursiveActionWrapper(Action<T> action)
            {
                _action = action;
            }

            public void ApplyAction(object target, IObjectContext context)
            {
                _action((T)target);
            }
        }

        internal class RecursiveActionWrapper<T, TContext> : IRecursiveAction
        {
            Action<T, TContext> _action;
            Type _contextType;
            public RecursiveActionWrapper(Action<T, TContext> action)
            {
                _action = action;
                _contextType = typeof(TContext);
            }

            public void ApplyAction(object target, IObjectContext context)
            {
                var contextValue = context.GetContext<TContext>();
                if (contextValue == null)
                    throw new ApplicationException($"Recursive action context of type [{this._contextType.FullName}] for type [{typeof(T).FullName}] cannot be null.");

                _action((T)target, contextValue);
            }
        }

        internal class CompositeActionWrapper : IRecursiveAction
        {
            IRecursiveAction[] _actions;
            public CompositeActionWrapper(IRecursiveAction[] actions)
            {
                if (actions == null)
                    throw new ArgumentNullException("actions");

                _actions = actions;
            }

            public void ApplyAction(object target, IObjectContext context)
            {
                var length = _actions.Length;
                for (int i = 0; i < length; i++)
                {
                    _actions[i].ApplyAction(target, context);
                }
            }
        }

        internal static class InternalCache
        {
            static ConcurrentDictionary<Type, RuntimeInfo> _cache = new ConcurrentDictionary<Type, RuntimeInfo>();

            public static bool TryGet(Type type, out RuntimeInfo runtimeInfo)
            {
                return _cache.TryGetValue(type, out runtimeInfo);
            }

            public static RuntimeInfo GetRuntimeInfo(Type type)
            {
                RuntimeInfo value = null;
                if (_cache.TryGetValue(type, out value))
                    return value;

                var newCreatedInfo = new Dictionary<Type, RuntimeInfo>();
                RuntimeInfo.Create(type, newCreatedInfo);

                var reducedDict = new Dictionary<Type, RuntimeInfo>();

                foreach (var element in newCreatedInfo)
                {
                    if (!_cache.ContainsKey(element.Key))
                        _cache[element.Key] = element.Value?.Reduce(reducedDict);
                }

                return _cache[type];
            }

            public static void Reset()
            {
                _cache.Clear();
            }
        }

        internal class RuntimeMemberInfo
        {
            public IMemberInfoExt MemberInfo { get; set; }
            public RuntimeInfo RuntimeInfo { get; set; }
        }

        internal class RuntimeInfo
        {
            Type _type;
            IRecursiveAction _actionInfo;
            RuntimeInfo _runtimeCollectionElementInfo;
            RuntimeMemberInfo[] _runtimeMembers;

            private RuntimeInfo() { }

            public static RuntimeInfo Create(Type type, Dictionary<Type, RuntimeInfo> tempCache)
            {
                if (type == null)
                    return null;

                if ((type.IsValueType && !type.IsGenericType) || type == typeof(string))
                {
                    tempCache[type] = null;
                    return null;
                }

                RuntimeInfo existing = null;
                if (InternalCache.TryGet(type, out existing))
                    return existing;

                if (tempCache.TryGetValue(type, out existing))
                    return existing;

                RuntimeInfo runtimeInfo = null;
                tempCache[type] = runtimeInfo = new RuntimeInfo()
                {
                    _type = type,
                    _actionInfo = GetRuntimeActionForType(type)
                };

                runtimeInfo._runtimeCollectionElementInfo = RuntimeInfo.Create(GetGenericCollectionElementType(type), tempCache);
                runtimeInfo._runtimeMembers = CreateRuntimeMemberInfo(type, tempCache).ToArray();

                return runtimeInfo;
            }

            static readonly Type _genericCollectionDefinition = typeof(IEnumerable<>);
            static Type GetGenericCollectionElementType(Type type)
            {
                foreach (var interfaceType in type.GetInterfaces().Where(t => t.IsGenericType))
                {
                    var genericDefinition = interfaceType.GetGenericTypeDefinition();
                    if (genericDefinition == _genericCollectionDefinition)
                    {
                        return interfaceType.GetGenericArguments().FirstOrDefault();
                    }
                }

                return null;
            }

            static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            private static IEnumerable<RuntimeMemberInfo> CreateRuntimeMemberInfo(Type type, Dictionary<Type, RuntimeInfo> tempCache)
            {
                var typeMembers = type.GetFields(bindingFlags).Where(f => !f.FieldType.IsValueType && f.FieldType != typeof(string))
                                    .Cast<MemberInfo>()
                                    .Concat(type.GetProperties(bindingFlags).Where(p => !p.PropertyType.IsValueType && p.PropertyType != typeof(string)))
                                    .Select(m => MemberInfoFactory.Create(m))
                                    .Where(m => m != null)
                                    .ToArray();

                foreach (var memberInfo in typeMembers)
                {
                    // Don't loop through dictionary twice.
                    if (memberInfo.MemberName == "Keys" && memberInfo.FieldOrPropertyType.Name == "KeyCollection")
                        continue;

                    if (memberInfo.MemberName == "Values" && memberInfo.FieldOrPropertyType.Name == "ValueCollection")
                        continue;

                    var runtimeInfo = RuntimeInfo.Create(memberInfo.FieldOrPropertyType, tempCache);

                    if (runtimeInfo != null)
                        yield return new RuntimeMemberInfo() { MemberInfo = memberInfo, RuntimeInfo = runtimeInfo };
                }
            }

            private bool HasRuntimeActionRecursive(Stack<Type> typeStack, int recursionLevel)
            {
                if (_actionInfo != null)
                {
                    return true;
                }

                if (recursionLevel > 3 && typeStack.Contains(this._type))
                {
                    return false; //breaks recursion
                }

                typeStack.Push(this._type);
                try
                {
                    if (_runtimeCollectionElementInfo?.HasRuntimeActionRecursive(typeStack, recursionLevel + 1) == true)
                    {
                        return true;
                    }

                    foreach (var runtimeMember in _runtimeMembers)
                    {
                        if (runtimeMember.RuntimeInfo?.HasRuntimeActionRecursive(typeStack, recursionLevel + 1) == true)
                            return true;
                    }

                    return false;
                }
                finally
                {
                    typeStack.Pop();
                }
            }

            public RuntimeInfo Reduce(Dictionary<Type, RuntimeInfo> reducedDict)
            {
                var typeStack = new Stack<Type>();

                return this.ReduceInternal(reducedDict, typeStack);
            }

            private RuntimeInfo ReduceInternal(Dictionary<Type, RuntimeInfo> reducedDict, Stack<Type> typeStack)
            {
                RuntimeInfo existing;
                if (reducedDict.TryGetValue(this._type, out existing))
                {
                    return existing;
                }

                //There's no registered action in this type tree?
                if (!this.HasRuntimeActionRecursive(typeStack, 0))
                {
                    reducedDict[this._type] = null;
                    return null;
                }

                //Adds element to the dictionary to break recursion as soon as possible.
                reducedDict[this._type] = this;

                //reduces collection element
                _runtimeCollectionElementInfo = _runtimeCollectionElementInfo?.ReduceInternal(reducedDict, typeStack);

                //reduces members
                _runtimeMembers = _runtimeMembers.Select(r =>
                {
                    var reduced = r.RuntimeInfo?.ReduceInternal(reducedDict, typeStack);
                    if (reduced == null)
                        return null;

                    r.RuntimeInfo = reduced;
                    return r;
                }).Where(r => r != null).ToArray();

                return this;
            }

            public void ApplyOn(object item, IObjectContext context, MaterializerTracker tracker)
            {
                if (item == null)
                    return;

                //Returns if object has already been processed in this context.
                if (!tracker.BeginTrackObject(item))
                    return;

                if (_runtimeCollectionElementInfo != null && item is IEnumerable)
                {
                    foreach (var element in (item as IEnumerable))
                    {
                        _runtimeCollectionElementInfo.ApplyOn(element, context, tracker);
                    }
                }

                if (_runtimeMembers.Length > 0)
                {
                    foreach (var runtimeMember in _runtimeMembers)
                    {
                        var memberValue = runtimeMember.MemberInfo.GetFieldOrPropertyValue(item);
                        runtimeMember.RuntimeInfo.ApplyOn(memberValue, context, tracker);
                    }
                }

                if (_actionInfo != null)
                {
                    _actionInfo.ApplyAction(item, context);
                }
            }

            public void ApplyOnRootFirst(object item, IObjectContext context, MaterializerTracker tracker)
            {
                if (item == null)
                    return;

                //Returns if object has already been processed in this context.
                if (!tracker.BeginTrackObject(item))
                    return;

                if (_actionInfo != null)
                {
                    _actionInfo.ApplyAction(item, context);
                }

                if (_runtimeCollectionElementInfo != null && item is IEnumerable)
                {
                    foreach (var element in (item as IEnumerable))
                    {
                        _runtimeCollectionElementInfo.ApplyOnRootFirst(element, context, tracker);
                    }
                }

                if (_runtimeMembers.Length > 0)
                {
                    foreach (var runtimeMember in _runtimeMembers)
                    {
                        var memberValue = runtimeMember.MemberInfo.GetFieldOrPropertyValue(item);
                        runtimeMember.RuntimeInfo.ApplyOnRootFirst(memberValue, context, tracker);
                    }
                }
            }
        }


        internal interface IMemberInfoExt
        {
            string MemberName { get; }
            Type FieldOrPropertyType { get; }
            object GetFieldOrPropertyValue(object target);
        }

        internal class MemberInfoFactory
        {
            public static IMemberInfoExt Create(MemberInfo memberInfo)
            {
                var memberType = (memberInfo as FieldInfo)?.FieldType ?? (memberInfo as PropertyInfo)?.PropertyType;
                if ((memberInfo as PropertyInfo)?.GetIndexParameters().Length != 0)
                    return null;  // not supported

                var infoType = typeof(MemberInfoExt<,>).MakeGenericType(memberInfo.DeclaringType, memberType);
                return (IMemberInfoExt)Activator.CreateInstance(infoType, memberInfo);
            }
        }

        internal class MemberInfoExt<TTarget, TMember> : IMemberInfoExt
        {
            MemberInfo _inner;
            Type _memberType;
            Func<TTarget, TMember> _getterDelegate;
            public MemberInfoExt(MemberInfo memberInfo)
            {
                _inner = memberInfo;
                _memberType = typeof(TMember);

                if (memberInfo is FieldInfo)
                {
                    //** generates IL to get field value - super fast
                    _getterDelegate = CreateGetFieldDelegate(memberInfo as FieldInfo);
                }
                else if (memberInfo is PropertyInfo)
                {
                    //** generates IL to get property value - super fast                    
                    _getterDelegate = CreateGetPropertyDelegate(memberInfo as PropertyInfo);
                }
            }

            public static Func<TTarget, TMember> CreateGetFieldDelegate(FieldInfo fieldInfo)
            {
                //Creates a field expression like (TTarget a) => a.Field
                var paramExp = Expression.Parameter(typeof(TTarget), "a");
                var fieldExp = Expression.Field(paramExp, fieldInfo);
                var lambdaExp = Expression.Lambda<Func<TTarget, TMember>>(fieldExp, paramExp);

                return lambdaExp.Compile();
            }

            public static Func<TTarget, TMember> CreateGetPropertyDelegate(PropertyInfo propertyInfo)
            {
                //Creates a property expression like (TTarget a) => a.Property
                var paramExp = Expression.Parameter(typeof(TTarget), "a");
                var propertyExp = Expression.Property(paramExp, propertyInfo);
                var lambdaExp = Expression.Lambda<Func<TTarget, TMember>>(propertyExp, paramExp);

                return lambdaExp.Compile();
            }

            public Type FieldOrPropertyType { get { return _memberType; } }

            public string MemberName { get { return _inner.Name; } }

            public object GetFieldOrPropertyValue(object target)
            {
                if (target == null)
                    return null;

                return _getterDelegate((TTarget)target);
            }
        }
    }
}
