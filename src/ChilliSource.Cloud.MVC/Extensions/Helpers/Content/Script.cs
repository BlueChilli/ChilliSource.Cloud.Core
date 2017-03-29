using ChilliSource.Cloud.Core;
using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for the script element.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="filename">The JavaScript file name.</param>
        /// <returns>An HTML string for the script element.</returns>
        public static MvcHtmlString Script(this HtmlHelper html, string filename)
        {
            string format = @"<script src=""{0}"" type=""text/javascript""></script>";

            return new MvcHtmlString(String.Format(format, ResolveFilenameToUrl(html, DirectoryType.Scripts, filename)));
        }

        private static MvcHtmlString ScriptOnJqueryReadyStart(this HtmlHelper html)
        {
            return MvcHtmlString.Create(html.ViewContext.RequestContext.HttpContext.Request.IsAjaxRequest() ? "" : "$(function () {");
        }

        private static MvcHtmlString ScriptOnJqueryReadyEnd(this HtmlHelper html)
        {
            return MvcHtmlString.Create(html.ViewContext.RequestContext.HttpContext.Request.IsAjaxRequest() ? "" : "});");
        }

        /// <summary>
        /// Returns HTML string for the script element of Google map API with key and library parameters from ChilliSource web project configuration file.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML string for the script element of Google map API.</returns>
        public static MvcHtmlString ScriptGoogleMapApi(this HtmlHelper htmlHelper)
        {
            var config = ProjectConfigurationSection.GetConfig();
            var key = config.GoogleApis.ApiKey(config.ProjectEnvironment);

            var libraries = config.GoogleApis.Libraries;
            var librariesParam = "";

            if (!String.IsNullOrWhiteSpace(libraries))
            {
                librariesParam = String.Format("libraries={0}&", libraries);
            }

            return MvcHtmlString.Create(@"<script type=""text/javascript"" src=""//maps.googleapis.com/maps/api/js?{1}key={0}&sensor=false&language=en""></script>".FormatWith(key, librariesParam));
        }
    }
}