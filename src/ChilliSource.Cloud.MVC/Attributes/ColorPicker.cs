
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Marks a field as a ColorPicker
    /// </summary>
    public class ColorPickerAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Color picker format
        /// </summary>
        public ColorPickerFormat Format { get; set; }
        /// <summary>
        /// Default Color (in the ColorPickerFormat - including # symbol).
        /// </summary>
        public string DefaultColor { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["ColorPicker-Format"] = Format;
            metadata.AdditionalValues["ColorPicker-Color"] = DefaultColor;
        }

        public static void Resolve(ModelMetadata metadata, FieldOptions fieldOptions, RouteValueDictionary attributes)
        {
            if ( metadata.AdditionalValues.ContainsKey("ColorPicker-Format"))
            {
                fieldOptions.SpecialisedType = SpecializedType.ColorPicker;

                fieldOptions.AppendedText = "<i></i>";
                string color = (metadata.Model as string).DefaultTo(metadata.AdditionalValues["ColorPicker-Color"] as string, "#000000");
                fieldOptions.WrappedAttributes = new { id = metadata.PropertyName + "-color", data_color_format = metadata.AdditionalValues["ColorPicker-Format"], data_color = color, @class = "color" };
            }
        }
    }

    /// <summary>
    /// Color picker formats
    /// </summary>
    public enum ColorPickerFormat
    {
        hex,
        rgb,
        rgba,
        hsl,
        hsla
    }
}
