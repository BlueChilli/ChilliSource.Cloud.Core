using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns an tag (eg H2) setup as an content editable field, with a hidden field that contains the value to be submitted.
        /// Used in conjunction with the js file jquery.bluechilli.content-editable.js
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="tag"></param>
        /// <param name="placeholder">Default text to display if a value hasn't been inputed. Will default to label text. Pass null if no placeholder desired.</param>
        /// <param name="charactersLeftSelector">If the property has maxlength or stringlength attribute, optionally pass in a css selector to a number of characters remaining eg <span id="DescriptionLimit">@(80 - Model.Description.Length)</span></param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>Returns an tag (eg H2) setup as an content editable field, with a hidden field that contains the value to be submitted</returns>
        public static MvcHtmlString ContentEditableFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string tag, string placeholder = "", string charactersLeftSelector = null, object htmlAttributes = null)
        {
            var member = expression.Body as MemberExpression;
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);
            attributes["contenteditable"] = Boolean.TrueString.ToLower();
            attributes["data-target"] = member.Member.Name;

            if (placeholder != null)
            {
                placeholder = placeholder.DefaultTo(GetLabelTextFor(html, expression));
                attributes["data-placeholder"] = placeholder;
            }
            ResolveStringLength(member, attributes);
            if (charactersLeftSelector != null)
            {
                attributes["data-characters-left"] = charactersLeftSelector;
            }

            var htmlTag = new TagBuilder(tag);
            var value = metadata.Model == null ? null : metadata.Model.ToString();
            if (!String.IsNullOrEmpty(metadata.DisplayFormatString))
            {
                if (!String.IsNullOrEmpty(value)) value = String.Format("{" + metadata.DisplayFormatString + "}", metadata.Model);
            }
            htmlTag.SetInnerText(value.DefaultTo(placeholder));
            htmlTag.MergeAttributes(attributes);

            var hiddenTarget = html.HiddenFor(expression);

            return MvcHtmlString.Create(htmlTag.ToString(TagRenderMode.Normal) + hiddenTarget.ToString());
        }
    }
}