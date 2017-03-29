using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    //Example code usage in RouteConfig
    //var domainRoute = "";
    //switch (ProjectConfigurationSection.GetConfig().ProjectEnvironment)
    //{
    //    case ProjectEnvironment.Development: domainRoute = "{trainerSubdomain}.trainer.dev.bluechilli.com"; break;
    //    case ProjectEnvironment.Staging: domainRoute = "{trainerSubdomain}.trainer.staging.bluechilli.com"; break;
    //    case ProjectEnvironment.Production: domainRoute = "{trainerSubdomain}.ptapp.com"; break;
    //}

    //routes.Add("DomainRoute", new DomainRoute(
    //    domainRoute,     // Domain with parameters
    //    "{action}/{id}", // URL with parameters
    //    new { trainerSubdomain = "ptapp", controller = "Member", action = "Lead", id = "" }  // Parameter defaults
    //));

    /// <summary>
    /// Manages the Domain Routing.
    /// Code based from here http://blog.maartenballiauw.be/post/2009/05/20/ASPNET-MVC-Domain-Routing.aspx.
    /// </summary>
    public class DomainRoute : Route
    {
        private Regex domainRegex;
        private Regex pathRegex;

        /// <summary>
        /// Domain name.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Initializes a new instance of the DomainRoute class by using the specified domain, URL and route value defaults.
        /// </summary>
        /// <param name="domain">The Domain name.</param>
        /// <param name="url">The URL string.</param>
        /// <param name="defaults">The key/value default values for a route.</param>
        public DomainRoute(string domain, string url, RouteValueDictionary defaults)
            : base(url, defaults, new MvcRouteHandler())
        {
            Domain = domain;
        }

        /// <summary>
        /// Initializes a new instance of the DomainRoute class by using the specified domain, URL, route value defaults and route handler.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        /// <param name="url">The URL string.</param>
        /// <param name="defaults">The key/value default values for a route.</param>
        /// <param name="routeHandler">An object that processes the request.</param>
        public DomainRoute(string domain, string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
            Domain = domain;
        }

        /// <summary>
        /// Initializes a new instance of the DomainRoute class by using the specified domain, URL and route value defaults.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        /// <param name="url">The URL string.</param>
        /// <param name="defaults">The key/value default values for a route.</param>
        public DomainRoute(string domain, string url, object defaults)
            : base(url, new RouteValueDictionary(defaults), new MvcRouteHandler())
        {
            Domain = domain;
        }

        /// <summary>
        /// Initializes a new instance of the DomainRoute class by using the specified domain, URL, route value defaults and route handler.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        /// <param name="url">The URL string.</param>
        /// <param name="defaults">The key/value default values for a route.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public DomainRoute(string domain, string url, object defaults, IRouteHandler routeHandler)
            : base(url, new RouteValueDictionary(defaults), routeHandler)
        {
            Domain = domain;
        }

        /// <summary>
        /// Returns information about the route in the collection that matches the specified values.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <returns>An object that contains the values from the route definition.</returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            // Build regex
            domainRegex = CreateRegex(Domain);
            pathRegex = CreateRegex(Url);

            // Request information
            string requestDomain = httpContext.Request.Headers["host"];
            if (!string.IsNullOrEmpty(requestDomain))
            {
                if (requestDomain.IndexOf(":") > 0)
                {
                    requestDomain = requestDomain.Substring(0, requestDomain.IndexOf(":"));
                }
            }
            else
            {
                requestDomain = httpContext.Request.Url.Host;
            }
            string requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;

            // Match domain and route
            Match domainMatch = domainRegex.Match(requestDomain);
            Match pathMatch = pathRegex.Match(requestPath);

            // Route data
            RouteData data = null;
            if (!requestDomain.StartsWith("www") && domainMatch.Success && pathMatch.Success)
            {
                data = new RouteData(this, RouteHandler);

                // Add defaults first
                if (Defaults != null)
                {
                    foreach (KeyValuePair<string, object> item in Defaults)
                    {
                        data.Values[item.Key] = item.Value;
                    }
                }

                // Iterate matching domain groups
                for (int i = 1; i < domainMatch.Groups.Count; i++)
                {
                    Group group = domainMatch.Groups[i];
                    if (group.Success)
                    {
                        string key = domainRegex.GroupNameFromNumber(i);

                        if (!string.IsNullOrEmpty(key) && !char.IsNumber(key, 0))
                        {
                            if (!string.IsNullOrEmpty(group.Value))
                            {
                                data.Values[key] = group.Value;
                            }
                        }
                    }
                }

                // Iterate matching path groups
                for (int i = 1; i < pathMatch.Groups.Count; i++)
                {
                    Group group = pathMatch.Groups[i];
                    if (group.Success)
                    {
                        string key = pathRegex.GroupNameFromNumber(i);

                        if (!string.IsNullOrEmpty(key) && !char.IsNumber(key, 0))
                        {
                            if (!string.IsNullOrEmpty(group.Value))
                            {
                                data.Values[key] = group.Value;
                            }
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Returns information about the URL path that is associated with the specified context, and parameter values.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the requested route.</param>
        /// <param name="values">An object that contains the parameters for a route.</param>
        /// <returns>An object that contains information about the URL path that is associated with the route.</returns>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return base.GetVirtualPath(requestContext, RemoveDomainTokens(values));
        }

        /// <summary>
        /// Returns route data that is associated with the specified context, and parameter values.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the requested route.</param>
        /// <param name="values">An object that contains the parameters for a route.</param>
        /// <returns>An object that contains information about domain route data.</returns>
        public DomainRouteData GetDomainData(RequestContext requestContext, RouteValueDictionary values)
        {
            // Build hostname
            string hostname = Domain;
            foreach (KeyValuePair<string, object> pair in values)
            {
                hostname = hostname.Replace("{" + pair.Key + "}", pair.Value.ToString());
            }

            // Return domain data
            return new DomainRouteData
            {
                Protocol = "http",
                HostName = hostname,
                Fragment = ""
            };
        }

        private Regex CreateRegex(string source)
        {
            // Perform replacements
            source = source.Replace("/", @"\/?");
            source = source.Replace(".", @"\.?");
            source = source.Replace("-", @"\-?");
            source = source.Replace("{", @"(?<");
            source = source.Replace("}", @">([a-zA-Z0-9_]*))");

            return new Regex("^" + source + "$");
        }

        private RouteValueDictionary RemoveDomainTokens(RouteValueDictionary values)
        {
            Regex tokenRegex = new Regex(@"({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?({[a-zA-Z0-9_]*})*-?\.?\/?");
            Match tokenMatch = tokenRegex.Match(Domain);
            for (int i = 0; i < tokenMatch.Groups.Count; i++)
            {
                Group group = tokenMatch.Groups[i];
                if (group.Success)
                {
                    string key = group.Value.Replace("{", "").Replace("}", "");
                    if (values.ContainsKey(key))
                        values.Remove(key);
                }
            }

            return values;
        }
    }

    /// <summary>
    /// Encapsulates information about route data.
    /// </summary>
    public class DomainRouteData
    {
        /// <summary>
        /// Gets or set the protocol.
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Gets or sets the fragment.
        /// </summary>
        public string Fragment { get; set; }
    }
}