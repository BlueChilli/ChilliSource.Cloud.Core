using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Specifies that the field should be displayed as radio boxes.
    /// Can be used on Bool or Enum fields.
    /// Can also be used in conjunction with fieldOptions.SelectList.
    /// </summary>
    public class RadioAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Text when false. Only used when applied on Bool properties.
        /// </summary>
        public string FalseText { get; set; }

        /// <summary>
        /// Text when true. Only used when applied on Bool properties.
        /// </summary>
        public string TrueText { get; set; }

        /// <summary>
        /// If true render the radio inline (horizontal), default is vertical
        /// </summary>
        public bool Inline { get; set; }

        public RadioAttribute()
        {
            TrueText = "Yes";
            FalseText = "No";
        }

        public RadioAttribute(string falseText, string trueText)
        {
            FalseText = falseText;
            TrueText = trueText;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["Radio"] = true;
            if (!String.IsNullOrEmpty(FalseText)) metadata.AdditionalValues["RadioFalseText"] = FalseText;
            if (!String.IsNullOrEmpty(FalseText)) metadata.AdditionalValues["RadioTrueText"] = TrueText;
        }
    }
}
