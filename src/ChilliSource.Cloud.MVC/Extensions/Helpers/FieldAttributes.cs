
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;


namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns System.Web.Routing.RouteValueDictionary which contains attributes defined on the model, HTML attributes and unobtrusive JavaScript validation attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="includeUnobtrusiveValidationAttributes">True to return unobtrusive JavaScript validation attributes, otherwise not.</param>
        /// <returns>The System.Web.Routing.RouteValueDictionary which contains attributes defined on the model, HTML attributes and unobtrusive JavaScript validation attributes.</returns>
        public static RouteValueDictionary FieldAttributesFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes = null, bool includeUnobtrusiveValidationAttributes = false)
        {
            var member = expression.Body as MemberExpression;
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);
            if (includeUnobtrusiveValidationAttributes)
            {
                var propertyName = html.NameFor(expression).ToString();
                var validationAttributes = new RouteValueDictionary(html.GetUnobtrusiveValidationAttributes(propertyName, metadata));
                attributes.Merge(validationAttributes);
            }

            ResolveReadOnly(metadata, attributes);
            ResolveStringLength(member, attributes);

            HttpPostedFileBaseFileExtensionsAttribute.Resolve(metadata, attributes);
            PhoneNumberAttribute.Resolve(member, metadata, attributes);

            switch (metadata.DataTypeName)
            {
                case "EmailAddress":
                    attributes.Add("Type", "email");
                    break;
                case "Url": attributes.Add("Type", "url"); break;
                case "creditcard":
                    attributes.Add("Pattern", "[0-9]*");
                    break;
                case "numeric":
                case "Currency":
                    attributes.AddOrSkipIfExists("Type", "number");
                    if (metadata.DataTypeName == "Currency")
                        attributes.Add("Step", "any");
                    break;
                case "Html":
                    attributes["class"] += " richText";
                    break;
            }

            return attributes;
        }

        private static void ResolveReadOnly(ModelMetadata metadata, IDictionary<string, object> attributes)
        {
            if (metadata.IsReadOnly)
            {
                attributes["disabled"] = "disabled";
                attributes["class"] += "disabled";

                if (!String.IsNullOrEmpty(metadata.DisplayFormatString))
                {
                    attributes["Value"] = String.Format("{" + metadata.DisplayFormatString + "}", metadata.Model);
                }
            }
        }

        public static void ResolveStringLength(MemberExpression member, IDictionary<string, object> attributes)
        {
            var maxLength = member.Member.GetAttribute<MaxLengthAttribute>(false);
            if (maxLength != null && maxLength.Length > 0)
            {
                attributes["maxlength"] = maxLength.Length;
            }

            var minLength = member.Member.GetAttribute<MinLengthAttribute>(false);
            if (minLength != null && minLength.Length > 0)
            {
                attributes["minlength"] = minLength.Length;
            }

            // StringLength overrides MinLength and MaxLength (you are not supposed to use both anyway):
            var stringLength = member.Member.GetAttribute<StringLengthAttribute>(false);
            if (stringLength != null)
            {
                if (stringLength.MaximumLength > 0) attributes["maxlength"] = stringLength.MaximumLength;
                if (stringLength.MinimumLength > 0) attributes["minlength"] = stringLength.MinimumLength;
            }

            //CharactersLeft is always used in combination with StringLength
            var charactersLeft = member.Member.GetAttribute<CharactersLeftAttribute>(false);
            if (charactersLeft != null)
            {
                attributes["charactersleft"] = true;
            }
        }
    }
}