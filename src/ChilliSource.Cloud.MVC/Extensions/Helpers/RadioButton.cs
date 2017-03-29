using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Core;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for radio buttons for enumeration values.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="inline">True to use field options to specify inline option, otherwise not.</param>
        /// <returns>An HTML string for radio buttons for enumeration values.</returns>
        public static MvcHtmlString RadioButtonForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null, bool inline = false)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
            var values = Enum.GetNames(enumType);
            var names = EnumExtensions.GetDescriptions(enumType);
            var list = values.ToSelectList(names);
            var items = RemoveItemAttribute.Resolve(metadata, list.ToList());
            list = list.ToSelectList();

            return MakeRadio(htmlHelper, expression, htmlAttributes, inline, metadata, list);
        }

        /// <summary>
        /// Returns HTML string for radio buttons for Boolean values.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="trueText">Text for the "true" radio button.</param>
        /// <param name="falseText">Text for the "false" radio button.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="inline">True to use field options to specify inline option, otherwise not.</param>
        /// <returns>An HTML string for radio buttons for Boolean values.</returns>
        public static MvcHtmlString RadioButtonForBool<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string trueText = null, string falseText = null, object htmlAttributes = null, bool inline = false)
        {
            trueText = StringExtensions.DefaultTo(trueText, bool.TrueString);
            falseText = StringExtensions.DefaultTo(falseText, bool.FalseString);
            var names = new string[] { trueText, falseText };
            var values = new string[] { bool.TrueString, bool.FalseString };
            var list = values.ToSelectList(names);

            return htmlHelper.RadioButtonForList(expression, list, htmlAttributes, inline);
        }

        /// <summary>
        /// Returns HTML string for radio buttons for System.Web.Mvc.SelectList.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="list">A list that lets users select one item.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="inline">True to use field options to specify inline option, otherwise not.</param>
        /// <returns>An HTML string for radio buttons for System.Web.Mvc.SelectList.</returns>
        public static MvcHtmlString RadioButtonForList<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, SelectList list, object htmlAttributes = null, bool inline = false)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            return MakeRadio(htmlHelper, expression, htmlAttributes, inline, metaData, list);
        }

        private static MvcHtmlString MakeRadio<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, bool inline, ModelMetadata metaData, SelectList list)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < list.Count(); i++)
            {
                var item = list.ElementAt(i);
                var id = string.Format(
                    "{0}{1}_{2}",
                    String.IsNullOrEmpty(htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix) ? "" : htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix + "_",
                    metaData.PropertyName,
                    item.Text.Replace(" ", "")
                );
                var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);
                attributes.Merge("id", id);

                var radio = htmlHelper.RadioButtonFor(expression, item.Value, attributes).ToHtmlString();
                sb.AppendFormat(
                    @"<label class=""radio{0}"">{1}{2}</label>",
                    inline ? " inline" : "",
                    radio,
                    item.Text
                );
            }
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}