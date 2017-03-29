using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Collection of helpers for rendering snippets of code from within partial views which the rendered location of can be controlled from the layout.
    /// </summary>
    public static class CustomScriptsHelper
    {
        private const string _CustomSection = "RenderCustomSection";

        /// <summary>
        /// Register a code template for rendering elsewhere
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to be registered in for example "scripts"</param>
        /// <param name="template">code template</param>
        /// <returns></returns>
        public static HelperResult RegisterCustomSection(this HtmlHelper html, string section, Func<object, HelperResult> template)
        {
            return RegisterCustomSection(html, section, Guid.NewGuid(), template);
        }

        /// <summary>
        /// Register a code template for rendering elsewhere
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to be registered in for example "scripts"</param>
        /// <param name="templateKey">To register templates that should only be rendered once
        /// <param name="template">code template</param>
        /// <returns></returns>
        public static HelperResult RegisterCustomSection(this HtmlHelper html, string section, Guid templateKey, Func<object, HelperResult> template)
        {
            var sections = html.ViewContext.HttpContext.Items[_CustomSection] as Dictionary<string, Dictionary<Guid, string>>;

            if (sections == null)
            {
                sections = new Dictionary<string, Dictionary<Guid, string>>();
                html.ViewContext.HttpContext.Items.Add(_CustomSection, sections);
            }

            Dictionary<Guid, string> content = null;
            if (sections.ContainsKey(section))
            {
                content = sections[section];
            }
            else
            {
                content = new Dictionary<Guid, string>();
                sections.Add(section, content);
            }

            if (!content.ContainsKey(templateKey))
            {
                content.Add(templateKey, template(null).ToHtmlString());
            }

            return new HelperResult(writer => { });
        }

        /// <summary>
        /// Shortcut for registering a custom section for scripts. This is the main type of section registered. 
        /// </summary>
        /// <param name="html"></param>
        /// <param name="template">script template</param>
        /// <returns></returns>
        public static HelperResult RegisterCustomScripts(this HtmlHelper html, Func<object, HelperResult> template)
        {
            return RegisterCustomSection(html, "scripts", template);
        }

        /// <summary>
        /// Render all the registered templates for a section. Usually called in the layout page.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to output for example "scripts"</param>
        /// <returns></returns>
        public static MvcHtmlString RenderCustomSection(this HtmlHelper html, string section)
        {
            var result = new StringBuilder();

            var sections = html.ViewContext.HttpContext.Items[_CustomSection] as Dictionary<string, Dictionary<Guid, string>>;
            if (sections != null)
            {
                if (sections.ContainsKey(section))
                {
                    var content = sections[section];
                    foreach (var item in content)
                    {
                        result.AppendLine(item.Value);
                    }
                }
            }
            return MvcHtmlString.Create(result.ToString());
        }
    }
}