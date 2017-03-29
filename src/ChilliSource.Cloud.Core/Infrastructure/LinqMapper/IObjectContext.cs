using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.LinqMapper
{
    /// <summary>
    /// This class holds context instances used by LinqMapper
    /// </summary>
    public interface IObjectContext
    {
        /// <summary>
        /// Sets a typed value
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="value">A value</param>
        /// <param name="override">Flag that allows overriding an existing value</param>
        void SetContext<T>(T value);

        /// <summary>
        /// Gets a typed value
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <returns>The typed value</returns>
        T GetContext<T>();


        /// <summary>
        /// Gets a typed value
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>        
        /// <param name="value">The typed value if exists</param>
        /// <returns>Whether the value exists</returns>
        bool TryGetContext<T>(out T value);
    }

    internal class ContextContainer : IObjectContext
    {
        static readonly Type ThisType = typeof(IObjectContext);
        Dictionary<Type, object> _contexts = null;

        public ContextContainer() { }

        public void SetContext<T>(T value)
        {
            SetContextOfType(typeof(T), value);
        }

        public T GetContext<T>()
        {
            var type = typeof(T);
            if (type == ThisType)
                return (T)(object)this;

            return (T)GetContextOfType(type);
        }

        private void SetContextOfType(Type type, object value)
        {
            if (_contexts == null)
                _contexts = new Dictionary<Type, object>();

            _contexts[type] = value;
        }

        private object GetContextOfType(Type type)
        {
            if (_contexts == null || !_contexts.ContainsKey(type))
                throw new ApplicationException($"Context value not found for type [{type.FullName}]");

            return _contexts[type];
        }

        public bool TryGetContext<T>(out T value)
        {
            var type = typeof(T);
            if (type == ThisType)
            {
                value = (T)(object)this;
                return true;
            }
            object objValue;
            if (_contexts == null || !_contexts.TryGetValue(type, out objValue))
            {
                value = default(T);
                return false;
            }

            value = (T)objValue;
            return true;
        }
    }
}
