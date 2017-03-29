
using ChilliSource.Cloud.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for the icon element rendering with HTML tag &lt;i&gt; by matching the extension of specified file name and size.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="filename">The file name from which to get the extension.
        /// When the file extension is one of the following extensions, a defined CSS class name and associated icon image will be used, otherwise unknown file object ("ufo") CSS class name and associated icon image will be used.
        /// File extensions: "csv", "doc", "docx", "gif", "html", "jpeg", "jpg", "mov", "mp3", "mpeg", "odp", "ods", "odt", "pdf", "png", "ppt", "pptx", "rtf", "swf", "txt", "wmv", "xls", "xlsx", "zip".
        /// </param>
        /// <param name="size">The file size defined by BlueChilli.Web.IconFileTypeSize</param>
        /// <returns>An HTML string for the icon element.</returns>
        public static MvcHtmlString IconFileType(this HtmlHelper html, string filename, IconFileTypeSize size = IconFileTypeSize.Medium)
        {
            var format = @"<i class=""icon-file-{0}{1}""></i>";
            var ext = Path.GetExtension(filename).TrimStart('.').ToLower();
            var extensions = new List<string> { "csv", "doc", "docx", "gif", "html", "jpeg", "jpg", "mov", "mp3", "mpeg", "odp", "ods", "odt", "pdf", "png", "ppt", "pptx", "rtf", "swf", "txt", "wmv", "xls", "xlsx", "zip" };
            if (!extensions.Contains(ext)) ext = "ufo";
            return MvcHtmlString.Empty.Format(format, size.GetDescription(), ext);
        }
    }

    /// <summary>
    /// Enumeration values of BlueChilli.Web.IconFileTypeSize.
    /// </summary>
    public enum IconFileTypeSize
    {
        /// <summary>
        /// Small icon image with dimensions of 20 * 26
        /// </summary>
        [Description("small-")]Small,  //20*26
        /// <summary>
        /// Medium icon image with dimensions of 31 * 40
        /// </summary>
        [Description("")]Medium,       //31*40
        /// <summary>
        /// Large icon image with dimensions of 62 * 80
        /// </summary>
        [Description("large-")]Large   //62*80
    }
}