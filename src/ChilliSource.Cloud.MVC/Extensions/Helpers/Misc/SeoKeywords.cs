
using ChilliSource.Cloud.Core;
using System;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents support for rendering HTML meta tag for SEO key words in a view.
    /// </summary>
    public static class SeoKeywordsHtmlHelper
    {
        /// <summary>
        /// Emits following meta tags: application-name, author, title, description, keywords, and emits html title tag.
        /// http://www.problogger.net/archives/2011/04/27/how-to-select-good-seo-keywords/ 
        /// http://www.google.com/insights/search
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="title">Page title.</param>
        /// <param name="description">Page description.</param>
        /// <param name="keywords">Page keywords comma separated. Phrases are better than single words.</param>
        /// <param name="author">Change the author meta tag. Useful for article pages contributed by users. Their name will appear when shared by FB</param>        
        /// <returns>The meta tag for SEO key words.</returns>
        public static MvcHtmlString SeoKeywords(this HtmlHelper html, string title, string description, string keywords, string author = "Blue Chilli Technology Pty Ltd", bool addProjectNameToTitle = true)
        {
            string format = @"<meta name=""{0}"" content=""{1}"" />";
            var config = ProjectConfigurationSection.GetConfig();
            var sb = new StringBuilder();
            sb.AppendFormat(format, "application-name", config.ProjectDisplayName);
            sb.AppendFormat(format, "author", html.Encode(author));
            sb.AppendFormat(format, "title", html.Encode(title));
            sb.AppendFormat(format, "description", html.Encode(description));
            sb.AppendFormat(format, "keywords", html.Encode(keywords));
            if (String.IsNullOrEmpty(title))
                sb.AppendFormat("<title>{0}</title>", config.ProjectDisplayName);
            else if (addProjectNameToTitle)
                sb.AppendFormat("<title>{0} - {1}</title>", title, config.ProjectDisplayName);
            else   
                sb.AppendFormat("<title>{0}</title>", title);
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}