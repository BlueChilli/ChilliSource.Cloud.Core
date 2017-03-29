using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Sets a custom label for a field.
    /// </summary>
    public class LabelAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Label text
        /// </summary>
        public string Value { get; set; }

        public LabelAttribute(string value)
        {
            Value = value;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["Label"] = Value;
        }
    }
}
