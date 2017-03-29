using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Forces request to be over https protocol. Works with AWS load balancing
    /// </summary>
    public class RequireHttpsWeb : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (IsForwardedSsl(filterContext))
            {
                return;
            }
            base.OnAuthorization(filterContext);
        }

        private static bool IsForwardedSsl(AuthorizationContext actionContext)
        {
            var xForwardedProto = actionContext.HttpContext.Request.Headers["X-Forwarded-Proto"];
            var forwardedSsl = !string.IsNullOrWhiteSpace(xForwardedProto) &&
                string.Equals(xForwardedProto, "https", StringComparison.InvariantCultureIgnoreCase);
            return forwardedSsl;
        }
    }
}
