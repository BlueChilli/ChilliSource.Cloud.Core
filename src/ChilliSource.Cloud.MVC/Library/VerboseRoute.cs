using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Encapsulates functions for defining a route and for obtaining information about the route.
    /// </summary>
    public class VerboseRoute : Route
    {
        /// <summary>
        /// Initializes a new instance of the VerboseRoute class by using the specified url pattern, default parameter values and namespaces.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use for any parameters that are missing in the URL.</param>
        /// <param name="namespaces">Namespace values that are passed to the route handler.</param>
        public VerboseRoute(string url, object defaults, string[] namespaces = null)
            : base(url, new RouteValueDictionary(defaults), new MvcRouteHandler())
        {
            base.DataTokens = new RouteValueDictionary();
            if (namespaces != null)
            {
                base.DataTokens["Namespaces"] = namespaces;
            }
        }

        /// <summary>
        /// Returns information about the requested route.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <returns>An instance of RouteData object.</returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var routeData = base.GetRouteData(httpContext);
            if (routeData == null) return null;
            return routeData;
        }

        /// <summary>
        /// Returns information about the url that's associated with the route.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the requested route.</param>
        /// <param name="values">An object that contains the parameters for a route.</param>
        /// <returns>An instance of VirtualPathData object.</returns>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            string action = values[RouteHelper.KeyAction] as string;
            var data = base.GetVirtualPath(requestContext, values);
            if (String.IsNullOrEmpty(action) || values[RouteHelper.KeyAction].Equals(base.Defaults[RouteHelper.KeyAction]))
            {
                values[RouteHelper.KeyAction] = base.Defaults[RouteHelper.KeyAction];
                var indexQuery = data.VirtualPath.IndexOf("?");
                var query = indexQuery >= 0 ? data.VirtualPath.Substring(indexQuery) : "";
                //leaves the query string untouched.
                data.VirtualPath = string.Concat(base.Url.TransformWith(values.ToDictionary(), removeUnused: true), query);
            }
            return data;
        }
    }
}
