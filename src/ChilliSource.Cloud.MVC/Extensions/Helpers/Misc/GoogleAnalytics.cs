using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents support for rendering Google Analytics script element.
    /// </summary>
    public static class GoogleAnalyticsHtmlHelper
    {
        private static GoogleAnalyticsElement config = ProjectConfigurationSection.GetConfig().GoogleAnalytics;

        /// <summary>
        /// Must use using statement to populate the complete html and must set 'enabled' configuration property in GoogleAnalyticsElement to true.
        /// Usage: e.g. @using(Html.GoogleAnalytics) {//put your custom tracking code here. e.g.  _gaq.push(['_trackPageview']);}
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="supportAdvertising">True to use "doubleclick.net" URL instead of the "google-analytics.com" URL, otherwise not.</param>
        /// <returns>An HTML string for the Google Analytics scripts.</returns>
        public static IDisposable GoogleAnalytics(this HtmlHelper htmlHelper, bool supportAdvertising = false)
        {
            if (config == null)
                throw new ArgumentNullException("config", "You must enable the google analytics in project config to use this html helper");

            if (config.Enabled)
            {

                return new DisposableWrapper(
                    () => htmlHelper.ViewContext.Writer.Write(htmlHelper.GoogleAnalyticsForBegin()),
                    () => htmlHelper.ViewContext.Writer.Write(htmlHelper.GoogleAnalyticsForEnd(supportAdvertising))
                );
                
            }
            // since you can't remove the texts inside wrapper so comments out if not enabled
            Action begin = () => htmlHelper.ViewContext.Writer.Write("<!--");
            Action end = () => htmlHelper.ViewContext.Writer.Write("-->");

            return new DisposableWrapper(begin, end);
        }

        private static MvcHtmlString GoogleAnalyticsForBegin(this HtmlHelper html)
        {           
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine(@"<script type=""text/javascript"">");
            htmlBuilder.AppendLine(@"var _gaq = _gaq || [];");
            htmlBuilder.AppendLine(string.Format(@"_gaq.push(['_setAccount', '{0}']);", config.Account));
            return new MvcHtmlString(htmlBuilder.ToString());
        }

        private static MvcHtmlString GoogleAnalyticsForEnd(this HtmlHelper html, bool supportAdvertising = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("(function () {");
            htmlBuilder.AppendLine("var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;");
            if (supportAdvertising)
            {
                htmlBuilder.AppendLine("ga.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'stats.g.doubleclick.net/dc.js';");
            }
            else
            {
                htmlBuilder.AppendLine("ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';");
            }
            htmlBuilder.AppendLine("var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);");
            htmlBuilder.AppendLine("})();");
            htmlBuilder.AppendLine("</script>");

            return new MvcHtmlString(htmlBuilder.ToString());
        }

        /// <summary>
        /// Must use using statement to populate the complete html and must set 'enabled' configuration property in GoogleAnalyticsElement to true.
        /// Usage: e.g. @using(Html.GoogleAnalytics) {//put your custom tracking code here. e.g. ga('send', 'pageview');}
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML string for the Google Analytics scripts.</returns>
        public static IDisposable GoogleAnalyticsUniversal(this HtmlHelper htmlHelper)
        {
            if (config == null)
                throw new ArgumentNullException("config", "You must enable the google analytics in project config to use this html helper");

            if (config.Enabled)
            {

                return new DisposableWrapper(
                    () => htmlHelper.ViewContext.Writer.Write(htmlHelper.GoogleAnalyticsUniversalForBegin()),
                    () => htmlHelper.ViewContext.Writer.Write(htmlHelper.GoogleAnalyticsUniversalForEnd())
                );

            }
            // since you can't remove the texts inside wrapper so comments out if not enabled
            Action begin = () => htmlHelper.ViewContext.Writer.Write("<!--");
            Action end = () => htmlHelper.ViewContext.Writer.Write("-->");

            return new DisposableWrapper(begin, end);
        }

        private static MvcHtmlString GoogleAnalyticsUniversalForBegin(this HtmlHelper html)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine(@"<script type=""text/javascript"">");  
            htmlBuilder.AppendLine(@"(function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){");
            htmlBuilder.AppendLine(@"(i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),");
            htmlBuilder.AppendLine(@"m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)");
            htmlBuilder.AppendLine(@"})(window,document,'script','//www.google-analytics.com/analytics.js','ga');");
            htmlBuilder.AppendLine(string.Format(@"ga('create', '{0}', 'auto');", config.Account));
            return new MvcHtmlString(htmlBuilder.ToString());

        }

        private static MvcHtmlString GoogleAnalyticsUniversalForEnd(this HtmlHelper html)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("</script>");

            return new MvcHtmlString(htmlBuilder.ToString());
        }
    }
}
