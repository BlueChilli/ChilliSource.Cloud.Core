
using System;
using System.Web.Mvc;

//Named so to not pollute @Html
namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Contains extension methods of System.Web.Mvc.HtmlHelper for assembly information.
    /// </summary>
    public static class AssemblyHtmlHelper
    {
        /// <summary>
        /// Returns the Current Version from the AssemblyInfo.cs file. For example [assembly: AssemblyVersion("1.0.*")] will change on each compile
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML-encoded string.</returns>
        public static MvcHtmlString CurrentWebVersion(this HtmlHelper helper)
        {
            var version = AssemblyHelper.GetWebApplicationAssembly().GetName().Version;
            return new MvcHtmlString(version.ToString());
        }

        /// <summary>
        /// Returns version date if following set in AssemblyInfo.cs file.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>A System.DateTime.</returns>
        public static DateTime CurrentWebVersionDate(this HtmlHelper helper)
        {
            var version = AssemblyHelper.GetWebApplicationAssembly().GetName().Version;
            return AssemblyHelper.GetVersionDate(version);
        }
    }
}