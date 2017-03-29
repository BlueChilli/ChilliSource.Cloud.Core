using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;


namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for label tag with specified CSS class "control-label" for BootStrap. 
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="labelText">The label text.</param>
        /// <param name="fieldOptions">An object that contains additional options for label field.</param>
        /// <returns>An HTML string for label.</returns>
        public static MvcHtmlString BootStrapLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText = null, FieldOptions fieldOptions = null)
        {
            if (fieldOptions == null) fieldOptions = new FieldOptions();
            if (fieldOptions.Label.IsIn(FieldLabel.None, FieldLabel.Placeholder)) return MvcHtmlString.Empty;
            fieldOptions.LabelText = fieldOptions.LabelText.DefaultTo(labelText, GetLabelTextFor(html, expression));
            var className = !String.IsNullOrEmpty(fieldOptions.LabelClass) ? String.Format("control-label {0}", fieldOptions.LabelClass) : "control-label";

            return LabelFor(html, expression, new { @class = className }, fieldOptions.LabelText);
        }

        /// <summary>
        /// Returns HTML string for label tag.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="labelText">The label text.</param>
        /// <returns>An HTML string for label.</returns>
        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes, string labelText = null)
        {
            return LabelFor(html, expression, new RouteValueDictionary(htmlAttributes), labelText);
        }

        /// <summary>
        /// Returns HTML string for label tag.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">A System.Collections.Generic.IDictionary&lt;string, object&gt; that contains the HTML attributes.</param>
        /// <param name="labelText">The label text.</param>
        /// <returns>An HTML string for label.</returns>
        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes, string labelText = null)
        {
            labelText = labelText ?? GetLabelTextFor(html, expression);
            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var member = expression.Body as MemberExpression;

            TagBuilder tag = new TagBuilder("label");
            tag.MergeAttributes(htmlAttributes);

            var mandatory = member.Member.GetAttribute<RequiredAttribute>(false);
            if (mandatory != null)
            {
                tag.AddCssClass("mandatory");
            }

            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            tag.InnerHtml = labelText;
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        private static string GetLabelTextFor<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string labelText = (metadata.AdditionalValues.SingleOrDefault(m => m.Key == "Label").Value as string);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            return labelText ?? metadata.DisplayName ?? metadata.PropertyName.ToSentenceCase(true) ?? htmlFieldName.Split('.').Last();
        }
    }
}