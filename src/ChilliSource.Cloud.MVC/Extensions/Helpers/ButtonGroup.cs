using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for a group of buttons for enumeration values.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The property of the value.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="selectList">A collection of System.Web.Mvc.SelectList.</param>
        /// <returns>An HTML string for a group of buttons for enumeration values.</returns>
        /// <remarks>In almost all cases consume this function via FieldFor or FieldInnerFor and place a ButtonGroupAttribute on your property.</remarks>
        public static MvcHtmlString ButtonGroupForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null, IEnumerable<SelectListItem> selectList = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var list = selectList;
            if (selectList == null)
            {
                Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
                var values = Enum.GetNames(enumType).ToList();
                var names = EnumExtensions.GetDescriptions(enumType).ToList();
                list = values.ToSelectList(names);
                var items = RemoveItemAttribute.Resolve(metadata, list.ToList());
                list = new SelectList(items, "Value", "Text");
            }
            return MakeButtonGroup(htmlHelper, expression, htmlAttributes, metadata, list.ToSelectList());
        }

        /// <summary>
        /// Returns HTML string for a group of buttons for Boolean value.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The property of the value.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="trueText">Text for the true value.</param>
        /// <param name="falseText">Text for the false value.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML string for a group of buttons for Boolean value.</returns>
        public static MvcHtmlString ButtonGroupForBool<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string trueText = null, string falseText = null, object htmlAttributes = null)
        {
            trueText = StringExtensions.DefaultTo(trueText, bool.TrueString);
            falseText = StringExtensions.DefaultTo(falseText, bool.FalseString);
            var names = new string[] { trueText, falseText };
            var values = new string[] { bool.TrueString, bool.FalseString };
            var list = values.ToSelectList(names);

            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            return MakeButtonGroup(htmlHelper, expression, htmlAttributes, metaData, list);
        }

        //todo process htmlAttributes
        private static MvcHtmlString MakeButtonGroup<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, ModelMetadata metaData, SelectList list)
        {
            var propertyName = htmlHelper.NameFor(expression).ToString();
            var properyId = htmlHelper.IdFor(expression).ToString();

            var sb = new StringBuilder(htmlHelper.HiddenFor(expression).ToHtmlString());
            //TODO To support flags uses buttons-checkbox
            sb.AppendLine(@"<div class=""btn-group"" data-toggle=""buttons-radio"">");

            for (var i = 0; i < list.Count(); i++)
            {
                var item = list.ElementAt(i);
                var onclick = "$('#{0}').val($(this).val()).change();".FormatWith(properyId);
                var format = @"<button class=""btn{0}"" name=""{1}"" value=""{2}"" data-toggle=""button"" type=""button"" onclick=""{3}"">{4}</button>";
                sb.AppendFormat(format, metaData.Model != null && metaData.Model.ToString() == item.Value ? " active" : "", propertyName, item.Value, onclick, item.Text);
            }
            sb.AppendLine("</div>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}