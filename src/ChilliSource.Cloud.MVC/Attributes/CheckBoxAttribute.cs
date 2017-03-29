using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Marks a field to be generated as CheckBoxes. The field type must be Enum.
    /// Marks a boolean property to be generated as a checkbox template control
    /// </summary>
    public class CheckBoxAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Use the alternative checkbox template
        /// </summary>
        public bool IsAlternative { get; set; }

        /// <summary>
        /// Optionally display a label for this checkbox
        /// </summary>
        public string Label { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["CheckBox"] = true;
        }
    }
}
