using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Contains methods for System.Reflection.Assembly.
    /// </summary>
    internal class AssemblyHelper
    {
        #region GetAssembly
        private const string AspNetNamespace = "ASP";

        /// <summary>
        /// Gets an System.Reflection.Assembly which is representing the current running application.
        /// </summary>
        /// <returns>An System.Reflection.Assembly which is representing the current running application.</returns>
        public static Assembly GetApplicationAssembly()
        {
            // Try the EntryAssembly, this doesn't work for ASP.NET classic pipeline (untested on integrated)
            Assembly assembly = Assembly.GetEntryAssembly();

            if (assembly == null) assembly = GetWebApplicationAssembly();

            // Fallback to executing assembly
            return assembly ?? (Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Gets an System.Reflection.Assembly which is representing the current web application.
        /// </summary>
        /// <returns>An System.Reflection.Assembly which is representing the current web application.</returns>
        public static Assembly GetWebApplicationAssembly()
        {
            var context = HttpContext.Current;
            if (context == null) return null;

            var application = context.ApplicationInstance;
            if (application == null) return null;

            Type type = application.GetType();
            while (type != null && type != typeof(object) && type.Namespace == AspNetNamespace)
                type = type.BaseType;

            return type.Assembly;
        }
        #endregion

        /// <summary>
        /// Gets version date if following set in AssemblyInfo.cs
        /// [assembly: AssemblyVersion("1.0.*")].
        /// </summary>
        /// <param name="version">Teh System.Version to check.</param>
        /// <returns>A System.DateTime of the version specified.</returns>
        public static DateTime GetVersionDate(Version version)
        {
            var ticksForDays = TimeSpan.TicksPerDay * version.Build; // days since 1 January 2000
            var ticksForSeconds = TimeSpan.TicksPerSecond * 2 * version.Revision; // seconds since midnight, (multiply by 2 to get original)
            return new DateTime(2000, 1, 1).Add(new TimeSpan(ticksForDays + ticksForSeconds));
        }
    }
}
