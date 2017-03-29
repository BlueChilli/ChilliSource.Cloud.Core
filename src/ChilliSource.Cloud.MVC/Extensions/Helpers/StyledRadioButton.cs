using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for radio button with CSS style "styled".
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the System.Web.Mvc.ViewDataDictionary or the System.Web.Mvc.ModelStateDictionary object matches this value, this radio button is selected.</param>
        /// <returns>An HTML string for radio button with CSS style "styled".</returns>
        public static MvcHtmlString StyledRadioButtonFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object value)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var attributes = new Dictionary<string, object>();
            attributes["class"] = "styled";

            if (metadata.AdditionalValues.ContainsKey("ConditionalDisplayPropertyName"))
            {
                attributes["data-conditional-on"] = metadata.AdditionalValues["ConditionalDisplayPropertyName"];
                attributes["data-conditional-values"] = string.Join(",", (object[])metadata.AdditionalValues["ConditionalDisplayPropertyValues"]);
            }

            return htmlHelper.RadioButtonFor(expression, value, attributes);
        }
    }
}