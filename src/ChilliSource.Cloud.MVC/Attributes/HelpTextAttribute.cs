using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Displays a help text after the input field.
    /// </summary>
    public class HelpTextAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Help text to be displayed
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Specifies whether the help text should be displayed in-line or as a block (under the input field).
        /// </summary>
        public bool DisplayAsBlock { get; set; }

        public HelpTextAttribute(string value, bool displayAsBlock = true)
        {
            Value = value;
            DisplayAsBlock = displayAsBlock;
            
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["HelpText"] = Value;
            metadata.AdditionalValues["HelpText-Display"] = (DisplayAsBlock) ? "help-block" : "help-inline";
        }

        public static MvcHtmlString GetHelpTextFor<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object transformData = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string helpText = (metadata.AdditionalValues.SingleOrDefault(m => m.Key == "HelpText").Value as string);
            if (string.IsNullOrEmpty(helpText)) return new MvcHtmlString("");

            string helpTextDisplay = (metadata.AdditionalValues.SingleOrDefault(m => m.Key == "HelpText-Display").Value as string);
            if (transformData != null) helpText = helpText.TransformWith(transformData);

            return new MvcHtmlString(string.Format(@"<p class=""{0}"">{1}</p>", helpTextDisplay, helpText));
        }
    }
}
