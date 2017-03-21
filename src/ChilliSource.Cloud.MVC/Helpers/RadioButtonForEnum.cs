using System;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using BlueChilli.Lib;

namespace BlueChilli.Web
{
    public static partial class Helpers
    {
        /// <summary>
        /// In almost all cases consume this function via FieldFor and place a RadioAttribute on your property
        /// <param name="inline">Use fieldoptions to specify inline option</param>
        /// </summary>
        public static MvcHtmlString RadioButtonForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null, bool inline = false)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var names = Enum.GetNames(metaData.ModelType);

            return MakeRadio(htmlHelper, expression, htmlAttributes, inline, metaData, names, names);
        }

        public static MvcHtmlString RadioButtonForBool<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string trueText = null, string falseText = null, object htmlAttributes = null, bool inline = false)
        {
            trueText = StringExtend.DefaultTo(trueText, bool.TrueString);
            falseText = StringExtend.DefaultTo(falseText, bool.FalseString);
            var names = new string[] { trueText, falseText };
            var values = new string[] { bool.TrueString, bool.FalseString };

            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            return MakeRadio(htmlHelper, expression, htmlAttributes, inline, metaData, names, values);
        }

        private static MvcHtmlString MakeRadio<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, bool inline, ModelMetadata metaData, string[] names, string[] values)
        {
            int i = 0;
            var sb = new StringBuilder();
            foreach (var name in names)
            {
                var id = string.Format(
                    "{0}_{1}_{2}",
                    htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                    metaData.PropertyName,
                    name
                );

                var radio = htmlHelper.RadioButtonFor(expression, values[i], new { id = id }).ToHtmlString();
                sb.AppendFormat(
                    @"<label class=""radio{0}"">{1}{2}</label>",
                    inline ? " inline" : "",
                    radio,
                    HttpUtility.HtmlEncode(name)
                );
                i++;
            }
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
