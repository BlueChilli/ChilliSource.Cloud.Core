
using ChilliSource.Cloud.Core;
using System;
using System.Web.Mvc;

//Named so to not pollute @Html
namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents support for rendering BugHerd script element.
    /// </summary>
    public static class BugHerdHtmlHelper
    {
        /// <summary>
        /// Returns HTML string for the BugHerd script element.
        /// </summary>
        /// <param name="helper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="useAlternativeApiKey">True to use alternative API key in ChilliSource web project configuration file, otherwise to use API key.</param>
        /// <returns>An HTML string for the BugHerd script element.</returns>
        public static MvcHtmlString BugHerd(this HtmlHelper helper, bool useAlternativeApiKey = false)
        {
            var config = ProjectConfigurationSection.GetConfig().BugHerd;

            if (config.Enabled)
            {
                string format = @"<script type='text/javascript'>(function (d, t) {{ var bh = d.createElement(t), s = d.getElementsByTagName(t)[0]; bh.type = 'text/javascript'; bh.src = '//www.bugherd.com/sidebarv2.js?apikey={0}'; s.parentNode.insertBefore(bh, s); }})(document, 'script');</script>";
                return MvcHtmlString.Create(String.Format(format, useAlternativeApiKey ? config.AlternativeApiKey : config.ApiKey));
            }
            return MvcHtmlString.Empty;
        }
    }
}