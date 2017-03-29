
using ChilliSource.Cloud.Web;
using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for read only field inside div tags with and CSS classes "control-group" and "controls".
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="labelText">The label text of the input field.</param>
        /// <returns>>An HTML string for read only field.</returns>
        public static MvcHtmlString FieldReadOnlyFor<TModel, TField>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TField>> expression, object htmlAttributes = null, string labelText = null)
        {
            var div = new TagBuilder("div");
            div.AddCssClass("control-group");

            var label = htmlHelper.BootStrapLabelFor(expression, labelText);
            var divInput = new TagBuilder("div");
            divInput.AddCssClass("controls");

            var routeValues = new RouteValueDictionary(htmlAttributes);
            routeValues.Add("disabled", "disabled");
            routeValues.Merge("class", "disabled");

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (!String.IsNullOrEmpty(metadata.DisplayFormatString))
            {
                routeValues.Add("Value", String.Format("{" + metadata.DisplayFormatString + "}", metadata.Model));
            }

            var textBox = htmlHelper.TextBoxFor(expression, routeValues);

            divInput.InnerHtml = textBox.ToHtmlString();
            div.InnerHtml = label.ToHtmlString() + divInput.ToString();

            return MvcHtmlString.Create(div.ToString(TagRenderMode.Normal));
        }
    }
}