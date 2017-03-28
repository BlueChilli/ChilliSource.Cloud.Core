using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    /// <summary>
    /// Scope context extension methods
    /// </summary>
    public static class IScopeContextExtensions
    {
        /// <summary>
        /// Executes an action asynchronously on an instance within a scope defined by IScopeContextHelper.<br/>
        /// The scope is automatically created and disposed.
        /// </summary>
        /// <typeparam name="T">A object type</typeparam>
        /// <param name="helper">A scope context helper</param>
        /// <param name="action">An action to be executed asynchronously.</param>
        /// <returns>Returns a running Task</returns>
        public static Task ExecuteAsync<T>(this IScopeContextHelper helper, Action<T> action)
        {
            return helper.ExecuteAsync<T>(null, action);
        }

        /// <summary>
        /// Executes an action asynchronously on an instance within a scope defined by IScopeContextHelper.<br/>
        /// The scope is automatically created and disposed.
        /// </summary>
        /// <typeparam name="T">A object type</typeparam>
        /// <param name="helper">A scope context helper</param>
        /// <param name="scopeSetup">(Optional)A delegate to setup the scope. Usually used to set Singleton values. e.g. scope =&gt; scope.SetSingletonValue&lt;T&gt;(value)</param>
        /// <param name="action">An action to be executed asynchronously.</param>
        /// <returns>Returns a running Task</returns>
        public static Task ExecuteAsync<T>(this IScopeContextHelper helper, Action<IScopeContext> scopeSetup, Action<T> action)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    helper.Execute(scopeSetup, action);
                }
                catch (Exception ex)
                {
                    ex.LogException();
                    throw;
                }
            });
        }

        /// <summary>
        /// Executes an action on an instance within a scope defined by IScopeContextHelper.<br/>
        /// The scope is automatically created and disposed.
        /// </summary>
        /// <typeparam name="T">A object type</typeparam>
        /// <param name="helper">A scope context helper</param>
        /// <param name="action">An action to be executed.</param>
        public static void Execute<T>(this IScopeContextHelper helper, Action<T> action)
        {
            helper.Execute<T>(null, action);
        }

        /// <summary>
        /// Executes an action on an instance within a scope defined by IScopeContextHelper.<br/>
        /// The scope is automatically created and disposed.
        /// </summary>
        /// <typeparam name="T">A object type</typeparam>
        /// <param name="helper">A scope context helper</param>
        /// <param name="scopeSetup">(Optional)A delegate to setup the scope. Usually used to set Singleton values. e.g. scope =&gt; scope.SetSingletonValue&lt;T&gt;(value)</param>
        /// <param name="action">An action to be executed.</param>
        public static void Execute<T>(this IScopeContextHelper helper, Action<IScopeContext> scopeSetup, Action<T> action)
        {
            using (var scope = helper.CreateScope())
            {
                if (scopeSetup != null)
                    scopeSetup(scope);

                var instance = scope.Get<T>();
                action(instance);
            }
        }
    }
}
