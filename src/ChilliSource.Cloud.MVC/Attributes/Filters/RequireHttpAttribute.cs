using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Represents an attribute that forces an secured HTTPS request to be re-sent over HTTP.
    /// </summary>
    public class RequireHttpAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Uses HTTP request instead of HTTPS.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsSecureConnection && 
                !filterContext.ActionDescriptor.IsDefined(typeof(RequireHttpsAttribute), true) &&
                !filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(RequireHttpsAttribute), true))
            {
                UriBuilder builder = new UriBuilder()
                {
                    Scheme = "http",
                    Host = filterContext.HttpContext.Request.Url.Host,
                    Path = filterContext.HttpContext.Request.RawUrl
                };

                filterContext.Result = new RedirectResult(builder.ToString());
                filterContext.Result.ExecuteResult(filterContext);
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
