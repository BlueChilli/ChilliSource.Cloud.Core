
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Represents support for rendering HTML button element (&lt;button&gt; tag) in a view.
    /// </summary>
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for the button element.
        /// </summary>
        /// <typeparam name="TModel">The type of the button model.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="actionName">The name of the action used to generate URL of the button action.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the button action.</param>
        /// <param name="area">The name of the area used to generate URL of the button action.</param>
        /// <param name="routeName">The name of the route used to generate URL of the button action.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the button action.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the button action.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>An HTML-encoded string for the button element.</returns>
        public static MvcHtmlString Button<TModel>(this HtmlHelper<TModel> htmlHelper, string actionName, string controllerName = "", string area = "", string routeName = "", string id = null, object routeValues = null, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            return Button(new UrlHelper(htmlHelper.ViewContext.RequestContext), actionName, controllerName, area, id, routeName, routeValues, displayText, buttonClasses, iconClasses, buttonAttributes);
        }

        /// <summary>
        /// Returns HTML string for the button element.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="actionName">The name of the action used to generate URL of the button action.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the button action.</param>
        /// <param name="area">The name of the area used to generate URL of the button action.</param>
        /// <param name="routeName">The name of the route used to generate URL of the button action.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the button action.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the button action.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>An HTML-encoded string for the button element.</returns>
        public static MvcHtmlString Button(UrlHelper urlHelper, string actionName, string controllerName = "", string area = "", string routeName = "", string id = null, object routeValues = null, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            displayText = (displayText == String.Empty) ? actionName : displayText;
            TagBuilder tag = new TagBuilder("button");
            tag.SetInnerText(displayText);

            if (!String.IsNullOrEmpty(iconClasses))
            {
                var iconTag = new TagBuilder("i");
                iconTag.AddCssClass(iconClasses);
                tag.InnerHtml = iconTag.ToString() + " " + tag.InnerHtml;
            }

            var attributes = buttonAttributes as RouteValueDictionary;
            if (attributes == null) attributes = new RouteValueDictionary(buttonAttributes);
            if (!attributes.ContainsKey("type")) attributes["type"] = "button";
            tag.MergeAttributes(attributes);
            tag.AddCssClass("btn " + buttonClasses);

            if (attributes["onclick"] == null && attributes["type"].ToString() != "submit")
            {
                var href = urlHelper.DefaultAction(actionName, controllerName, area, routeName, id, routeValues);
                tag.MergeAttribute("onclick", String.Format(@"window.location=""{0}""", href));
            }

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        /// <summary>
        /// Returns HTML string for the button element to post form.
        /// </summary>
        /// <typeparam name="TModel">The type of the button model.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="actionName">The name of the action used to generate URL of the button action.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the button action.</param>
        /// <param name="area">The name of the area used to generate URL of the button action.</param>
        /// <param name="routeName">The name of the route used to generate URL of the button action.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the button action.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the button action.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="confirm">JavaScript function to confirm before from post.</param>
        /// <returns>An HTML-encoded string for the button element to post form.</returns>
        /// <remarks>Uses jquery.doPost.js</remarks>
        public static MvcHtmlString ButtonPost<TModel>(this HtmlHelper<TModel> htmlHelper, string actionName, string controllerName = "", string area = "", string routeName = "", string id = "", object routeValues = null, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, string confirm = null)
        {
            return ButtonPost(new UrlHelper(htmlHelper.ViewContext.RequestContext), actionName, controllerName, area, routeName, id, routeValues, displayText, buttonClasses, iconClasses, buttonAttributes, confirm);
        }

        /// <summary>
        /// Returns HTML string for the button element to post form.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="actionName">The name of the action used to generate URL of the button action.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the button action.</param>
        /// <param name="area">The name of the area used to generate URL of the button action.</param>
        /// <param name="routeName">The name of the route used to generate URL of the button action.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the button action.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the button action.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="confirm">JavaScript function to confirm before from post.</param>
        /// <returns>An HTML-encoded string for the button element to post form.</returns>
        /// <remarks>Uses jquery.doPost.js</remarks>
        public static MvcHtmlString ButtonPost(UrlHelper urlHelper, string actionName, string controllerName = "", string area = "", string routeName = "", string id = "", object routeValues = null, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, string confirm = null)
        {
            displayText = (displayText == String.Empty) ? actionName : displayText;
            var href = urlHelper.DefaultAction(actionName, controllerName, area, routeName, id, routeValues);
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(buttonAttributes);

            if (String.IsNullOrEmpty(confirm))
            {
                attributes.Add("onclick", String.Format("$.doPost('{0}', {1});", href, "{}"));
            }
            else
            {
                attributes.Add("onclick", String.Format("if ({0}(this)) $.doPost('{1}', {2});", confirm, href, "{}"));
            }

            return Button(urlHelper, displayText, buttonClasses: buttonClasses, iconClasses: iconClasses, buttonAttributes: attributes);
        }

        /// <summary>
        /// Returns HTML string for the submit button element.
        /// </summary>
        /// <typeparam name="TModel">The type of the button model.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>An HTML-encoded string for the submit button element.</returns>
        public static MvcHtmlString ButtonSubmit<TModel>(this HtmlHelper<TModel> htmlHelper, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            return ButtonSubmit(urlHelper, displayText, buttonClasses, iconClasses, buttonAttributes);
        }

        /// <summary>
        /// Returns HTML string for the submit button element.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>An HTML-encoded string for the submit button element.</returns>
        public static MvcHtmlString ButtonSubmit(UrlHelper urlHelper, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            if (!buttonClasses.Contains("btn-primary")) buttonClasses += " btn-primary";
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(buttonAttributes);
            if (!attributes.ContainsKey("type")) attributes["type"] = "submit";

            return Button(urlHelper, urlHelper.CurrentAction(), displayText: displayText, buttonClasses: buttonClasses.Trim(), iconClasses: iconClasses, buttonAttributes: attributes);
        }

        /// <summary>
        /// Returns HTML string for the disabled button element.
        /// </summary>
        /// <typeparam name="TModel">The type of the button model.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="helpText">The text to display in "alert" popup window when button click event triggered.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>An HTML-encoded string for the disabled button element.</returns>
        public static MvcHtmlString ButtonDisabled<TModel>(this HtmlHelper<TModel> htmlHelper, string displayText, string helpText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            return ButtonDisabled(new UrlHelper(htmlHelper.ViewContext.RequestContext), displayText, helpText, buttonClasses, iconClasses, buttonAttributes);
        }

        /// <summary>
        /// Returns HTML string for the disabled button element.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="helpText">The text to display in "alert" popup window when button click event triggered.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>An HTML-encoded string for the disabled button element.</returns>
        public static MvcHtmlString ButtonDisabled(UrlHelper urlHelper, string displayText, string helpText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(buttonAttributes);

            if (String.IsNullOrEmpty(helpText))
            {
                attributes.Add("disabled", "disabled");
            }
            else
            {
                attributes.Add("onclick", String.Format("alert('{0}');", helpText));
                attributes.Add("rel", "tooltip");
                attributes.Add("title", helpText);
                buttonClasses += String.IsNullOrEmpty(buttonClasses) ? "btn-disabled" : " btn-disabled";
            }

            return Button(urlHelper, urlHelper.CurrentAction(), displayText: displayText, buttonClasses: buttonClasses.Trim(), iconClasses: iconClasses, buttonAttributes: attributes);
        }

        /// <summary>
        /// Returns HTML string for the button element to perform Ajax asynchronous request.
        /// </summary>
        /// <typeparam name="TModel">The type of the button model.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="target">The ID of the HTML element to be rendered after Ajax asynchronous request succeeded.</param>
        /// <param name="actionName">The name of the action used to generate URL of the button action.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the button action.</param>
        /// <param name="area">The name of the area used to generate URL of the button action.</param>
        /// <param name="routeName">The name of the route used to generate URL of the button action.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the button action.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the button action.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="post">True to use HTTP request method "POST", otherwise to use HTTP request method "GET".</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="callbackJs">JavaScript function to execute after Ajax asynchronous request succeeded.</param>
        /// <returns>An HTML-encoded string for the button element to perform Ajax asynchronous request.</returns>
        public static MvcHtmlString ButtonAjax<TModel>(this HtmlHelper<TModel> htmlHelper, string target, string actionName, string controllerName = "", string area = "", string routeName = "", string id = "", object routeValues = null, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, bool post = false, string dynamicData = "", string callbackJs = "")
        {
            return ButtonAjax(new UrlHelper(htmlHelper.ViewContext.RequestContext), target, actionName, controllerName, area, routeName, id, routeValues, displayText, buttonClasses, iconClasses, buttonAttributes, post, dynamicData, callbackJs);
        }

        /// <summary>
        /// Returns HTML string for the button element to perform Ajax asynchronous request.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="target">The ID of the HTML element to be rendered after Ajax asynchronous request succeeded.</param>
        /// <param name="actionName">The name of the action used to generate URL of the button action.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the button action.</param>
        /// <param name="area">The name of the area used to generate URL of the button action.</param>
        /// <param name="routeName">The name of the route used to generate URL of the button action.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the button action.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the button action.</param>
        /// <param name="displayText">The text to display on the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="post">True to use HTTP request method "POST", otherwise to use HTTP request method "GET".</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="callbackJs">JavaScript function to execute after Ajax asynchronous request succeeded.</param>
        /// <returns>An HTML-encoded string for the button element to perform Ajax asynchronous request.</returns>
        public static MvcHtmlString ButtonAjax(UrlHelper urlHelper, string target, string actionName, string controllerName = "", string area = "", string routeName = "", string id = "", object routeValues = null, string displayText = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, bool post = false, string dynamicData = "", string callbackJs= "")
        {
            callbackJs = !String.IsNullOrEmpty(callbackJs) ? callbackJs.Trim() : String.Empty;
            callbackJs = (callbackJs.Length > 0 && !callbackJs.EndsWith(";")) ? String.Concat(callbackJs, ";") : callbackJs;
            displayText = (displayText == String.Empty) ? actionName : displayText;
            var href = urlHelper.DefaultAction(actionName, controllerName, area, routeName, id, routeValues);
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(buttonAttributes);
            attributes.Add("href", "javascript:void(0);");

            var onclick = String.Format(@"$.ajaxLoad('{0}', '{1}', {2}, function() {{ {4} $.onAjaxEnd('button'); }}, '{3}'); $.onAjaxStart('{0}', null, this);",
                    target, href, String.IsNullOrEmpty(dynamicData) ? "null" : dynamicData, post ? "POST" : "GET", callbackJs);
            if (!attributes.ContainsKey("onclick"))
            {
                attributes.Add("onclick", onclick);
            }
            else
            {
                attributes["onclick"] = onclick + attributes["onclick"];
            }

            return Button(urlHelper, displayText, buttonClasses: buttonClasses, iconClasses: iconClasses, buttonAttributes: attributes);
        }
    }
}