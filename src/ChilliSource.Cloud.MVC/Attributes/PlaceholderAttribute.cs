using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using ChilliSource.Cloud.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Adds placeholder value to an input field. Value defaults to what is displayed as the label (for when label is hidden).
    /// Otherwise pass in the value you want displayed.
    /// </summary>
    public class PlaceholderAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Placeholder text description.
        /// </summary>
        public string Value { get; set; }

        public PlaceholderAttribute(string value = "Placeholder")
        {
            Value = value;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["Placeholder"] = Value;
        }

        public static string Resolve(ModelMetadata metadata, FieldOptions fieldOptions, RouteValueDictionary attributes)
        {
            var placeholderText = fieldOptions.LabelText;
            if (fieldOptions.Label == FieldLabel.Placeholder)
            {
                attributes["placeholder"] = placeholderText;
                return placeholderText;
            }
            else if (metadata.AdditionalValues.ContainsKey("Placeholder"))
            {
                var value = metadata.AdditionalValues["Placeholder"] as string;
                placeholderText = value == "Placeholder" ? metadata.GetDisplayName() : value;
                attributes.AddOrSkipIfExists("placeholder", placeholderText);
                return placeholderText;
            }
            return String.Empty;
        }
    }
}
