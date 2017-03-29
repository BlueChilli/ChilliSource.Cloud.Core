using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using ChilliSource.Cloud.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Base implementation of MyCustomAuthorizeAttribute. 
    /// Adds supports for : Dual attributes (controller attribute overridden by action attribute).
    /// AllowAnonymous on an action where authorise is on the controller.
    /// </summary>
    public class CustomAuthorizeAttribute : CustomAuthorizeBaseAttribute
    {
        /// <summary>
        /// Gets or sets additional roles.
        /// </summary>
        public string[] AdditionalRoles { get; set; }
        /// <summary>
        /// Check whether has additional roles defined.
        /// </summary>
        /// <returns></returns>
        public bool HasAdditionalRoles() { return this.AdditionalRoles != null && this.AdditionalRoles.Length > 0; }

        /// <summary>
        /// Initialize a new instance of BlueChilli.Web.CustomAuthorizeAttribute.
        /// </summary>
        public CustomAuthorizeAttribute() : base() { }

        private IEnumerable<string> GetAuthorizedRoles(CustomAuthorizeAttribute parentAttribute)
        {
            List<string> authorizedRoles = new List<string>();

            authorizedRoles.AddRange(this.Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            if (this.HasMultipleRoles())
                authorizedRoles.AddRange(MultipleRoles);

            if (this.HasAdditionalRoles())
            {
                authorizedRoles.AddRange(this.AdditionalRoles);

                if (!this.HasRoles() && !this.HasMultipleRoles() && parentAttribute != null)
                {
                    //Add controller roles
                    authorizedRoles.AddRange(parentAttribute.GetAuthorizedRoles(null));
                }
            }

            return authorizedRoles.Distinct();
        }

        /// <summary>
        /// Gets authorized roles defined in CustomAuthorize attribute
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <param name="redirectTo">Outs the redirectTo url defined in an action or controller (action overrides).</param>
        /// <returns>A collection of strings for authorized roles.</returns>
        public IList<string> GetAuthorizedRoles(AuthorizationContext filterContext, out string redirectTo)
        {
            IEnumerable<string> authorized = new string[] { };

            var controllerAttrs = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(CustomAuthorizeAttribute), true)
                .Cast<CustomAuthorizeAttribute>();
            var controllerAttr = controllerAttrs.FirstOrDefault();

            var actionAttrs = filterContext.ActionDescriptor.GetCustomAttributes(typeof(CustomAuthorizeAttribute), false)
                .Cast<CustomAuthorizeAttribute>().ToList();

            if (actionAttrs.Count > 0)
            {
                var actionAttr = actionAttrs[0];
                authorized = actionAttr.GetAuthorizedRoles(controllerAttr);

                redirectTo = actionAttr.RedirectTo;
            }
            else
            {
                redirectTo = this.RedirectTo;

                if (controllerAttr != null)
                    authorized = controllerAttr.GetAuthorizedRoles(null);
            }

            return authorized.ToList();
        }

        /// <summary>
        /// Checks whether the AllowAnonymous attribute has been defined for the current the action or controller.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <returns>True when AllowAnonymous attribute has been defined for the current the action or controller, otherwise false.</returns>
        public bool AllowsAnonymous(AuthorizationContext filterContext)
        {
            return filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), false) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);
        }

        /// <summary>
        /// Called when a process requests authorization.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            OnAuthorizationInternal(filterContext);
        }

        /// <summary>
        /// Called when a process requests authorization.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <returns>Returns the redirection result or null</returns>
        protected AuthorizationRedirectResult OnAuthorizationInternal(AuthorizationContext filterContext)
        {
            string redirectTo;
            var authorizedRoles = GetAuthorizedRoles(filterContext, out redirectTo);

            if (!AllowsAnonymous(filterContext) && !Authentication.IsInAnyRole(authorizedRoles, filterContext.HttpContext?.User))
            {
                return HandleRedirect(filterContext, redirectTo, this.ReturnUrl);
            }

            return null;
        }
    }

    /// <summary>
    /// Represents an attribute that is used to restrict access by callers to an action method.
    /// </summary>
    public abstract class CustomAuthorizeBaseAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Gets or sets the URL for redirection.
        /// </summary>
        public string RedirectTo { get; set; }
        /// <summary>
        /// Gets or sets the URL for return.
        /// </summary>
        public string ReturnUrl { get; set; }
        /// <summary>
        /// Gets or sets an array of roles.
        /// </summary>
        public string[] MultipleRoles { get; set; }
        /// <summary>
        /// Checks whether the specified authorize attribute has roles.
        /// </summary>
        /// <returns>True when the specified authorize attribute has roles, otherwise false.</returns>
        protected bool HasRoles() { return !String.IsNullOrEmpty(this.Roles); }
        /// <summary>
        /// Checks whether the specified authorize attribute has multiple roles.
        /// </summary>
        /// <returns>True when the specified authorize attribute has multiple roles, otherwise false.</returns>
        protected bool HasMultipleRoles() { return this.MultipleRoles != null && this.MultipleRoles.Length > 0; }

        private static UrlHelper UrlHelper { get { return new UrlHelper(HttpContext.Current.Request.RequestContext); } }

        /// <summary>
        /// Initialise a new instance of BlueChilli.Web.MyCustomAuthorizeAttribute.
        /// </summary>
        public CustomAuthorizeBaseAttribute()
        {
            RedirectTo = UrlHelper.Content(FormsAuthentication.LoginUrl);
            ReturnUrl = "";
        }

        /// <summary>
        /// Redirects page to the redirectTo URL when authorization failed, also works for Ajax request.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <param name="redirectTo">Url to redirect to</param>
        /// <param name="returnUrl">Return Url</param>
        /// <returns></returns>
        public AuthorizationRedirectResult HandleRedirect(AuthorizationContext filterContext, string redirectTo, string returnUrl)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest() && String.IsNullOrEmpty(returnUrl))
                returnUrl = UrlHelper.DefaultAction("");

            returnUrl = filterContext.RequestContext.TransformUrl(returnUrl);

            redirectTo = UriExtensions.Parse(redirectTo, new { ReturnUrl = returnUrl }).AbsoluteUri;
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.Headers["X-Ajax-Redirect"] = redirectTo;
                filterContext.Result = new EmptyResult();
            }
            else
            {
                filterContext.Result = new RedirectResult(redirectTo);
            }

            return new AuthorizationRedirectResult() { RedirectTo = redirectTo, ReturnUrl = returnUrl };
        }

        /// <summary>
        /// Redirects page to the redirectTo URL when authorization failed, also works for Ajax request.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public virtual void HandleRedirect(AuthorizationContext filterContext)
        {
            HandleRedirect(filterContext, this.RedirectTo, this.ReturnUrl);
        }
    }

    public class AuthorizationRedirectResult
    {
        public string RedirectTo { get; set; }
        public string ReturnUrl { get; set; }
    }
}