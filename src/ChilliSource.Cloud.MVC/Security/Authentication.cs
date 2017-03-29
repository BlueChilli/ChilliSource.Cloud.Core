
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Manages the authentication modules called during the client authentication process.
    /// </summary>
    public class Authentication
    {
        /// <summary>
        /// Helper method for resolving a return URL.
        /// </summary>
        /// <param name="alternative">@MyMenu.MenuItem.Redirect() - the menu to return to if return url is not present or is invalid.</param>
        /// <param name="returnUrl">Defaults to UrlReferrer - pass in a different value to override the default.</param>
        /// <returns>RedirectResult instance from redirect url.</returns>
        public static RedirectResult ResolveReturnUrl(RedirectResult alternative, string returnUrl = "")
        {
            returnUrl = GetReturnUrl(returnUrl);
            if (IsValidReturnUrl(returnUrl))
            {
                if (SetAjaxHeader(returnUrl)) return null;
                return new RedirectResult(returnUrl);
            }
            else if (alternative != null)
            {
                if (SetAjaxHeader(alternative.Url)) return null;
                return alternative;
            }

            return null;
        }

        private static bool SetAjaxHeader(string url)
        {
            if (HttpContext.Current != null)
            {
                var wrapper = new HttpContextWrapper(HttpContext.Current);
                if (wrapper.Request.IsAjaxRequest())
                {
                    HttpContext.Current.Response.Headers["X-Ajax-Redirect"] = url;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the URL specified in the query string.
        /// </summary>
        /// <param name="returnUrl">A string that contains the redirect URL.</param>
        /// <returns>Redirect url.</returns>
        public static string GetReturnUrl(string returnUrl = "")
        {
            if (returnUrl == null) returnUrl = "";
            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                returnUrl = returnUrl.DefaultTo(HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query)["ReturnUrl"], HttpUtility.ParseQueryString(HttpContext.Current.Request.Url.Query)["ReturnUrl"]);
            }
            return returnUrl;
        }

        /// <summary>
        /// Returns true if the URL is valid.
        /// </summary>
        /// <param name="returnUrl">A string that contains the redirect URL.</param>
        /// <returns>True if redirect URL is valid, otherwise False.</returns>
        public static bool IsValidReturnUrl(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl)) return false;
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return (url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && (returnUrl.StartsWith("/") || returnUrl.StartsWith("~"))
                && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"));
        }

        public static bool IsInAnyRole(IList<string> authorizedRoles)
        {
            return IsInAnyRole(authorizedRoles, HttpContext.Current?.User);
        }

        public static bool IsInAnyRole(IList<string> authorizedRoles, IPrincipal user)
        {
            if (user == null)
                return false;

            return user.Identity?.IsAuthenticated == true && (authorizedRoles.Count == 0 || authorizedRoles.Any(r => user.IsInRole(r)));
        }
    }
}
