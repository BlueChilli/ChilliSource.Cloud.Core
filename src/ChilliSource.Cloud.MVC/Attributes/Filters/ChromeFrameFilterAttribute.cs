using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{    
    /// <summary>
    /// For this to work with Ajax calls the following meta tag must be specified in the parent page. Or add it to your layout page.
    /// &lt;meta http-equiv="X-UA-Compatible" content="chrome=1"&gt;
    /// @Html.ChromeFrameMeta()
    /// </summary>
    public class ChromeFrameFilterAttribute : ActionFilterAttribute
    {
        private int SupportedFromVersion { get; set; }

        /// <summary>
        /// Ensures IE client browser has Chrome frame installed for old versions of IE.
        /// </summary>
        /// <param name="IESupportedFromVersion">The major version from which IE browsers are supported by your application. Defaults to IE 7 and above. Consider only supporting IE 9 and above.</param>
        public ChromeFrameFilterAttribute(int IESupportedFromVersion = 7)
        {
            SupportedFromVersion = IESupportedFromVersion;
        }

        /// <summary>
        /// Uses chrome frame if available otherwise use latest rendering engine.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;

            if (request.Browser.Browser.Trim().ToUpperInvariant().Equals("IE"))
            {
                //Use chrome frame if available otherwise use latest rendering engine
                if (response.Headers["X-UA-Compatible"] == null) response.AddHeader("X-UA-Compatible", "IE=Edge,chrome=1");
                if (request.Browser.MajorVersion < SupportedFromVersion && !request.UserAgent.Contains("chromeframe"))
                {
                    filterContext.Result = new ViewResult { ViewName = "InstallChromeFrame" };
                }
            }
        }
    }
}

namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Contains methods for Chrome frame.
    /// </summary>
    public static partial class ChromeFrameHelpers
    {
        /// <summary>
        /// Turns on Chrome frame if it is installed. Only needs to be used if the site has Ajax calls.
        /// </summary>
        /// <returns>An HTML-encoded string.</returns>
        public static MvcHtmlString ChromeFrameMeta(this HtmlHelper html)
        {
            if (html.ViewContext.RequestContext.HttpContext.Request.Browser.Browser.Trim().ToUpperInvariant().Equals("IE"))
            {
                return MvcHtmlString.Create(@"<meta http-equiv=""X-UA-Compatible"" content=""IE=edge,chrome=1"">");
            }
            return MvcHtmlString.Empty;
        }
    }
}