using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web
{
    /// <summary>
    /// Extension methods for System.Object.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts object to System.Web.Routing.RouteValueDictionary.
        /// </summary>
        /// <param name="value">Object to convert.</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        public static RouteValueDictionary ToRouteValues(this object value)
        {
            if (value == null) return new RouteValueDictionary();
            if (value is RouteValueDictionary) return new RouteValueDictionary(value as RouteValueDictionary);

            return new RouteValueDictionary(value.ToDynamic() as IDictionary<string, object>);
        }
    }
}
