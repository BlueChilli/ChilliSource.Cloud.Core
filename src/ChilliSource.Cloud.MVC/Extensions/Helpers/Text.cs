using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for text displayed with a label.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="html">An object that contains the HTML attributes.</param>
        /// <param name="labelText">The text for the label.</param>
        /// <param name="textToDisplay">The text to display.</param>
        /// <returns>An HTML string for text displayed with a label.</returns>
        public static MvcHtmlString Text<TModel>(this HtmlHelper<TModel> html, string labelText, string textToDisplay)
        {
            return new MvcHtmlString(
                String.Format(@"<div class=""control-group""><label class=""control-label"">{0}</label>
                        <div class=""controls"" style=""padding-top:5px;"">{1}</div></div>", labelText, textToDisplay));
        }
    }
}