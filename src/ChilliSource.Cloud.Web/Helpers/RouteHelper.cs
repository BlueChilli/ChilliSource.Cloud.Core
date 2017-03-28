using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web
{
    /// <summary>
    /// Contains methods for System.Web.Routing.Route.
    /// </summary>
    public static class RouteHelper
    {
        /// <summary>
        /// Constant string for action name.
        /// </summary>
        public const string KeyAction = "action";
        /// <summary>
        /// Constant string for area name.
        /// </summary>
        public const string KeyArea = "area";
        /// <summary>
        /// Constant string for controller name.
        /// </summary>
        public const string KeyController = "controller";
        /// <summary>
        /// The list of area, controller and action names.
        /// </summary>
        public static List<string> Keys = new List<string> { KeyArea, KeyController, KeyAction };

        #region Current
        /// <summary>
        /// Gets action from current HTTP context.
        /// </summary>
        /// <returns>A string value of action.</returns>
        public static string CurrentAction()
        {
            return CurrentAction(HttpContext.Current.Request.RequestContext);
        }

        /// <summary>
        /// Gets action from the specified HTTP request.
        /// </summary>
        /// <param name="context">A System.Web.Routing.RequestContext.</param>
        /// <returns>A string value of action.</returns>
        public static string CurrentAction(RequestContext context)
        {
            var routeData = context.RouteData.Values;
            return CurrentAction(routeData);
        }

        /// <summary>
        /// Gets action from the specified System.Web.Routing.RouteValueDictionary.
        /// </summary>
        /// <param name="routeData">A System.Web.Routing.RouteValueDictionary.</param>
        /// <returns>A string value of action.</returns>
        public static string CurrentAction(RouteValueDictionary routeData)
        {
            if (routeData.Keys.Contains(KeyAction))
            {
                return routeData[KeyAction].ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets controller from current HTTP context.
        /// </summary>
        /// <returns>A string value of controller.</returns>
        public static string CurrentController()
        {
            return CurrentController(HttpContext.Current.Request.RequestContext);
        }

        /// <summary>
        /// Gets controller from the specified HTTP request.
        /// </summary>
        /// <param name="context">A System.Web.Routing.RequestContext.</param>
        /// <returns>A string value of controller.</returns>
        public static string CurrentController(RequestContext context)
        {
            var routeData = context.RouteData.Values;
            return CurrentController(routeData);
        }

        /// <summary>
        /// Gets controller from the specified System.Web.Routing.RouteValueDictionary.
        /// </summary>
        /// <param name="routeData">A System.Web.Routing.RouteValueDictionary.</param>
        /// <returns>A string value of controller.</returns>
        public static string CurrentController(RouteValueDictionary routeData)
        {
            if (routeData.Keys.Contains(KeyController))
            {
                return routeData[KeyController].ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets area from current HTTP context.
        /// </summary>
        /// <returns>A string value of area.</returns>
        public static string CurrentArea()
        {
            return CurrentArea(HttpContext.Current.Request.RequestContext);
        }

        /// <summary>
        /// Gets area from the specified HTTP request.
        /// </summary>
        /// <param name="context">A System.Web.Routing.RequestContext.</param>
        /// <returns>A string value of area.</returns>
        public static string CurrentArea(RequestContext context)
        {
            var routeData = context.RouteData.DataTokens;
            return CurrentArea(routeData);
        }

        /// <summary>
        /// Gets area from the specified System.Web.Routing.RouteValueDictionary.
        /// </summary>
        /// <param name="routeData">A System.Web.Routing.RouteValueDictionary.</param>
        /// <returns>A string value of area.</returns>
        public static string CurrentArea(RouteValueDictionary routeData)
        {
            if (routeData.Keys.Contains(KeyArea))
            {
                return routeData[KeyArea].ToString();
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Returns information about the route in the collection that matches the specified values.
        /// </summary>
        /// <param name="uri">A System.Uri.</param>
        /// <returns>An object that contains the values from the route definition.</returns>
        public static RouteData GetRouteDataByUrl(Uri uri)
        {
            return GetRouteDataByUrl(uri.PathAndQuery);
        }

        /// <summary>
        /// Returns information about the route in the collection that matches the specified values.
        /// </summary>
        /// <param name="url">The string of url.</param>
        /// <returns>An object that contains the values from the route definition.</returns>
        public static RouteData GetRouteDataByUrl(string url)
        {
            if (!VirtualPathUtility.IsAppRelative(url))
            {
                if (!VirtualPathUtility.IsAbsolute(url))
                {
                    url = UriExtensions.Parse(url).PathAndQuery;
                }
                url = VirtualPathUtility.ToAppRelative(url);
            }


            return RouteTable.Routes.GetRouteData(new RewritedHttpContextBase(url));
        }

        private class RewritedHttpContextBase : HttpContextBase
        {
            private readonly HttpRequestBase mockHttpRequestBase;

            public RewritedHttpContextBase(string appRelativeUrl)
            {
                this.mockHttpRequestBase = new MockHttpRequestBase(appRelativeUrl);
            }

            public override HttpRequestBase Request
            {
                get
                {
                    return mockHttpRequestBase;
                }
            }

            public override System.Collections.IDictionary Items
            {
                get { return new Dictionary<string, object>(); }
            }

            private class MockHttpRequestBase : HttpRequestBase
            {
                private readonly string appRelativeUrl;

                public MockHttpRequestBase(string appRelativeUrl)
                {
                    this.appRelativeUrl = appRelativeUrl;
                }

                public override string AppRelativeCurrentExecutionFilePath
                {
                    get { return appRelativeUrl; }
                }

                public override string PathInfo
                {
                    get { return ""; }
                }

                public override string HttpMethod
                {
                    get { return "GET"; }
                }

                public override Uri Url
                {
                    get { return new Uri("http://chillisource.mock.com"); }
                }

                public override System.Collections.Specialized.NameValueCollection Headers
                {
                    get { return new NameValueCollection(); }
                }

                public override string ApplicationPath
                {
                    get { return ""; }
                }
            }
        }
    }
}
