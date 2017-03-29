
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

//Named so to not pollute @Html
namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents support for rendering UserVoice script element.
    /// </summary>
    public static class UserVoiceHtmlHelper
    {
        /// <summary>
        /// Returns HTML script for UserVoice.
        /// </summary>
        /// <param name="helper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML script for User Voice.</returns>
        public static MvcHtmlString UserVoice(this HtmlHelper helper)
        {
            var config = ProjectConfigurationSection.GetConfig();

            if (config.UserVoice.Enabled)
            {
                string format = @"<script type=""text/javascript"">var uvOptions = {{}};(function() {{var uv = document.createElement('script'); uv.type = 'text/javascript'; uv.async = true;uv.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'widget.uservoice.com/{0}.js';var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(uv, s);}})();</script>";
                return MvcHtmlString.Create(String.Format(format, config.UserVoice.ApiKey));
            }
            return MvcHtmlString.Empty;
        }
    }
}