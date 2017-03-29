
using ChilliSource.Cloud.Core;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents support for rendering project display name in a view.
    /// </summary>
    public static class ProjectConfigurationHelper
    {
        /// <summary>
        /// Returns HTML string containing the project display name from ChilliSource web project configuration file.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML string containing the project display name.</returns>
        public static MvcHtmlString ProjectDisplayName(this HtmlHelper htmlHelper)
        {
            var config = ProjectConfigurationSection.GetConfig();
            return new MvcHtmlString(config.ProjectDisplayName);
        }
    }
}