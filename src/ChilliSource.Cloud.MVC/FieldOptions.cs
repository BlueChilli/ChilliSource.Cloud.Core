using ChilliSource.Cloud.Core;
using System;
using System.ComponentModel;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Additional options for input fields
    /// </summary>
    public class FieldOptions
    {
        /// <summary>
        /// Raw html to be prepended before the input field
        /// </summary>
        public string PrependedText { get; set; }
        /// <summary>
        /// Raw html to be appended after the input field
        /// </summary>
        public string AppendedText { get; set; }
        /// <summary>
        /// Additional attributes on a div wrapping the input field (same div used for prepend and append).
        /// Every property in this object will be rendered as a DIV attribute (anonymous object recommended)
        /// </summary>
        public object WrappedAttributes { get; set; }

        public FieldClass Class { get; set; }

        /// <summary>
        /// When rendering radio buttons, specifies whether they should be displayed in-line.
        /// </summary>
        public bool DisplayInline { get; set; }

        public FieldLabel Label { get; set; }
        public string LabelText { get; set; }

        /// <summary>
        /// Use in conjunction with HelpText attribute. This property is to be used to format the help text.
        /// Properties in this object (anonymous object recommended) will be used to replace placeholders(e.g {placeholder1}). See TransformWith.
        /// </summary>
        public object HelpTextTransformData { get; set; }

        /// <summary>
        /// The selectList to be rendered in a drop-down field.
        /// </summary>
        public SelectList SelectList { get; set; }

        /// <summary>
        /// Specifies whether of one the custom controls should be rendered. See SpecializedType Enum.
        /// </summary>
        public SpecializedType SpecialisedType { get; set; }

        /// <summary>
        /// Defines the display format used by SpecialisedType inputs.
        /// </summary>
        public string DisplayFormatString { get; set; }

        /// <summary>
        /// CSS class to be applied to the ControlGroup div in Bootstrap
        /// </summary>
        public string ControlGroupClass { get; set; }

        /// <summary>
        /// CSS class to be applied to the label div in Bootstrap
        /// </summary>
        public string LabelClass { get; set; }

        public bool IsCheckboxLabel()
        {
            return this.Label == FieldLabel.CheckBox;
        }

        public bool IsResponsiveDateOrTimePicker()
        {
            return this.SpecialisedType == SpecializedType.ResponsiveDatePicker
                || this.SpecialisedType == SpecializedType.ResponsiveTimePicker;
        }
    }

    /// <summary>
    /// Enum that defines the width of inputs
    /// </summary>
    public enum FieldClass
    {
        Default,                                    //Take setting from project configuration
        None,                                       //No class set
        [Description("input-mini")]
        mini,
        [Description("input-small")]
        small,
        [Description("input-medium")]
        medium,
        [Description("input-large")]
        large,
        [Description("input-xlarge")]
        xlarge,
        [Description("input-xxlarge")]
        xxlarge,
        [Description("input-fullwidth")]
        fullwidth  //Custom css class depending on usage of wells etc width: 95% or width: 100%
    }

    /// <summary>
    /// Label field types
    /// </summary>
    public enum FieldLabel
    {
        /// <summary>
        /// Normal label
        /// </summary>
        Normal,
        /// <summary>
        /// Doesn't render the label.
        /// </summary>
        None,
        /// <summary>
        /// Doesn't render the label.
        /// </summary>
        Placeholder,
        /// <summary>
        /// Places the label after the input field
        /// </summary>
        CheckBox
    }

    /// <summary>
    /// Specifies whether of one the custom controls should be rendered.
    /// </summary>
    public enum SpecializedType
    {
        None,
        DatePickerCalendar,  //http://www.eyecon.ro/bootstrap-datepicker/
        TimePicker,
        [Data("ContainerCssClass", "datepicker-container")]
        [Data("PopUpCssClass", "datepicker")]
        [Data("PreviewCssClass", "date-preview")]
        [Data("HiddenCssClass", "date-field")]
        ResponsiveDatePicker,
        [Data("ContainerCssClass", "timepicker-container")]
        [Data("PopUpCssClass", "timepicker")]
        [Data("PreviewCssClass", "time-preview")]
        [Data("HiddenCssClass", "time-field")]
        ResponsiveTimePicker,
        ColorPicker
    }

    /// <summary>
    /// Defines which type of field will be rendered.    
    /// </summary>
    public enum FieldInputType
    {
        /// <summary>
        /// Renders input tag.
        /// </summary>
        Text,
        /// Renders input tag.
        Password,
        /// <summary>
        /// Renders styled div
        /// </summary>
        Select,
        /// <summary>
        /// Renders img tag
        /// </summary>
        Image
    }
}
