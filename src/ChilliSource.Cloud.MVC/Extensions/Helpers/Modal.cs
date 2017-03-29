using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Represents support for creating modal window.
    /// </summary>
    public static class ModalHelper
    {
        /// <summary>
        /// Creates model container which will host a partial view.
        /// </summary>
        /// <param name="helper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="id">Id of modal.</param>
        /// <param name="title">The title for modal window.</param>
        /// <param name="showClose">True to display close button, otherwise not.</param>
        /// <param name="showPrint">True to display print button, otherwise not.</param>
        /// <returns>An HTML string for the modal window.</returns>
        public static MvcHtmlString ModalContainer(this HtmlHelper helper, string id, string title = "", bool showClose = true, bool showPrint = false)
        {
            return new MvcHtmlString(ModalContainer(id, title, showClose, showPrint));
        }

        /// <summary>
        /// Creates model container which will host a partial view.
        /// </summary>
        /// <param name="id">Id of modal.</param>
        /// <param name="title">The title for modal window.</param>
        /// <param name="showClose">True to display close button, otherwise not.</param>
        /// <param name="showPrint">True to display print button, otherwise not.</param>
        /// <returns>The string for the modal window.</returns>
        public static string ModalContainer(string id, string title = "", bool showClose = true, bool showPrint = false)
        {
            string format = @"<div id=""{0}"" class=""modal hide""><div class=""modal-header"">{1}{2}{3}</div><div class=""modal-body"" id=""{0}_content""></div></div>";
            string titleFormat = "<h3>{0}</h3>";
            string close = @"<a class=""close"" data-dismiss=""modal"">×</a>";
            string print = @"<a class=""print"" onclick=""window.print();""></a>";

            if (!showClose) close = "";
            if (!showPrint) print = "";
            titleFormat = String.IsNullOrEmpty(title) ? "" : String.Format(titleFormat, title);

            return String.Format(format, id, close, print, titleFormat);
        }

        /// <summary>
        /// Returns HTML string of the link to open modal window.
        /// </summary>
        /// <param name="helper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="id">The id of the link.</param>
        /// <param name="url">The URL of the link.</param>
        /// <param name="title">The title of the link.</param>
        /// <param name="width">The width of the modal window.</param>
        /// <param name="height">The height of the modal window.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML string of the link to open modal window.</returns>
        public static MvcHtmlString ModalOpen(this HtmlHelper helper, string id, string url, string title = "", int? width = null, int? height = null, object htmlAttributes = null)
        {
            return new MvcHtmlString(ModalOpen(id, url, title, width, height));
        }

        private static string AppendJsonData(string source, string value)
        {
            if (!string.IsNullOrEmpty(source))
                return String.Format("{0}, {1}", source, value);

            return value;
        }

        /// <summary>
        /// Returns HTML string of the link to open modal window.
        /// </summary>
        /// <param name="id">The id of the link.</param>
        /// <param name="url">The URL of the link.</param>
        /// <param name="title">The title of the link.</param>
        /// <param name="width">The width of the modal window.</param>
        /// <param name="height">The height of the modal window.</param>
        /// <param name="iconClasses">CSS class for the icon of the link.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="commandOnly">True to return link without "onclick" event, otherwise with "onclick" event.</param>
        /// <param name="dynamicData">The dynamic data passed to "ajaxLoad" function.</param>
        /// <param name="BackgroundDrop">True to add CSS "backdrop: 'static'", otherwise not.</param>
        /// <param name="EscapeKey">True to add CSS "keyboard: false"</param>
        /// <returns>An HTML string of the link to open modal window.</returns>
        public static string ModalOpen(string id, string url, string title = "", int? width = null, int? height = null, string iconClasses = "", object htmlAttributes = null, bool commandOnly = false, string dynamicData = "", bool BackgroundDrop = true, bool EscapeKey = true)
        {
            string options = "";
            if (!BackgroundDrop) options = AppendJsonData(options, "backdrop: 'static'");
            if (!EscapeKey) options = AppendJsonData(options, "keyboard: false");
            options = (options.Length == 0) ? "'show'" : String.Format("{{ {0} }}", options);

            string onclick = @"$.ajaxLoad('{0}_content', '{1}', {2}, function() {{ $('#{0}').modal({5}){3}{4}; }});";
            string widthCss = ".css({{'width':'{0}'}})";
            string heightCss = ".css({{'height':'{0}'}})"; //FYI Max height is 500px!

            widthCss = width.HasValue ? String.Format(widthCss, width.Value) : "";
            heightCss = height.HasValue ? String.Format(heightCss, height.Value) : "";

            TagBuilder tag = new TagBuilder("a");
            tag.InnerHtml = title;
            if (!String.IsNullOrEmpty(iconClasses))
            {
                var iconTag = new TagBuilder("i");
                iconTag.AddCssClass(iconClasses);
                tag.InnerHtml = iconTag.ToString() + " " + tag.InnerHtml;
            }
            string onclickFormatted = String.Format(onclick, id, url, String.IsNullOrEmpty(dynamicData) ? "null" : dynamicData, widthCss, heightCss, options);
            if (commandOnly) return onclickFormatted;
            tag.Attributes.Add("onclick", onclickFormatted);
            tag.Attributes.Add("href", "javascript:void(0);");
            if (htmlAttributes != null) tag.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            return tag.ToString(TagRenderMode.Normal);
        }
    }
}