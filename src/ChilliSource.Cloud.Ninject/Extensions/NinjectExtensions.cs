using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    /// <summary>
    /// Extension methods for Ninject.
    /// </summary>
    public static class NinjectExtensions
    {
        /// <summary>
        /// Runs the scope action method to the specified binding syntax.
        /// </summary>
        /// <param name="Syntax">The syntax.</param>
        /// <param name="ScopeAction">A scope action method.</param>
        public static void InScopeAction(this IBindingSyntax Syntax, Action<IBindingSyntax> ScopeAction)
        {
            if (ScopeAction != null)
                ScopeAction(Syntax);
        }
    }
}
