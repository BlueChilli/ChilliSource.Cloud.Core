using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Core;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for an input field inside div tags with specified options and CSS classes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="fieldOptions">An object that contains additional options for input field.</param>
        /// <param name="labelText">The label text of the input field.</param>
        /// <returns>An HTML string for an input field.</returns>
        public static MvcHtmlString FieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes = null, FieldOptions fieldOptions = null, string labelText = null)
        {
            var result = html.FieldOuterForBegin(expression, labelText, fieldOptions).ToHtmlString() +
                html.FieldInnerFor(expression, htmlAttributes, fieldOptions).ToHtmlString() +
                html.FieldOuterForEnd(fieldOptions).ToHtmlString();

            return MvcHtmlString.Create(result);
        }

        /// <summary>
        /// Returns HTML string for an input field.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="fieldOptions">An object that contains additional options for input field.</param>
        /// <returns>An HTML string for an input field.</returns>
        public static MvcHtmlString FieldInnerFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes = null, FieldOptions fieldOptions = null)
        {
            if (fieldOptions == null) fieldOptions = new FieldOptions();
            return new MvcHtmlString(string.Format(@"{0}{1}{2}{3}",
               ResolveFieldOptions(fieldOptions, GetEditorFor(html, expression, htmlAttributes, fieldOptions)),
               GetStringLengthFor(html, expression),
               HelpTextAttribute.GetHelpTextFor(html, expression, fieldOptions.HelpTextTransformData),
               "" /*html.ValidationMessageFor(expression))*/
               ));
        }

        private static MvcHtmlString FieldOuterForBegin<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> labelExpression, string labelText = null, FieldOptions fieldOptions = null)
        {
            var expressionText = ExpressionHelper.GetExpressionText(labelExpression);
            var format = "<div " + GetControlOptionsString(expressionText, fieldOptions) + @">{0}<div class=""controls"">";
            if (fieldOptions != null)
            {
                if (fieldOptions.IsCheckboxLabel())
                {
                    format = "<div " + GetControlOptionsString(expressionText, fieldOptions) + "><div>";
                }
            }
            return new MvcHtmlString(string.Format(format, html.BootStrapLabelFor(labelExpression, labelText, fieldOptions)));
        }

        /// <summary>
        /// Returns HTML string for an input field.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="name">The name of the input field.</param>
        /// <param name="inputType">The type of the input field defined by BlueChilli.Models.FieldInputType.</param>
        /// <param name="label">The label text of the input field.</param>
        /// <param name="value">The value of the input field.</param>
        /// <param name="helpText">The value of the input field. (not used)</param>
        /// <param name="options">The System.Web.Mvc.SelectList for drop down list field.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML string for an input field.</returns>
        public static MvcHtmlString Field(this HtmlHelper html, string name, FieldInputType inputType, string label = "", object value = null, string helpText = "", SelectList options = null, object htmlAttributes = null)
        {
            string template = @"<div class=""control-group"">{0}<div class=""controls"">{1}</div></div>";

            if (!String.IsNullOrEmpty(label)) label = @"<label class=""control-label"" for=""{0}"">{1}</label>".FormatWith(name, label);

            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            attributes.Add("id", name);
            attributes.Add("name", name);
            TagBuilder tag = null;
            var tagMode = TagRenderMode.Normal;
            switch (inputType)
            {
                case FieldInputType.Text:
                case FieldInputType.Password:
                    tag = new TagBuilder("input");
                    tagMode = TagRenderMode.SelfClosing;
                    attributes.Add("type", inputType.ToString().ToLower());
                    if (value != null) attributes.Add("Value", value);
                    tag.MergeAttributes(attributes);
                    break;
                case FieldInputType.Select:
                    tag = new TagBuilder("div");
                    tag.AddCssClass("styled-select");
                    var select = new TagBuilder("select");
                    if (options != null)
                    {
                        var optionSB = new StringBuilder();
                        foreach (var option in options)
                        {
                            optionSB.AppendFormat(@"<option value=""{0}"">{1}</option>", option.Value, option.Text);
                        }
                        select.InnerHtml = optionSB.ToString();
                    }
                    select.MergeAttributes(attributes);
                    tag.InnerHtml = select.ToString(TagRenderMode.Normal) + @"<div class=""arrow""></div>";
                    break;
                case FieldInputType.Image:
                    return html.ImgEmbedded(value as byte[], label, htmlAttributes);
            }

            return MvcHtmlString.Create(template.FormatWith(label, tag.ToString(tagMode)));
        }

        private static MvcHtmlString FieldOuterForBegin(this HtmlHelper html)
        {
            return new MvcHtmlString(@"<div class=""control-group""><div class=""controls"">");
        }

        private static MvcHtmlString FieldOuterForEnd(this HtmlHelper html, FieldOptions fieldOptions)
        {
            return new MvcHtmlString("</div></div>");
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items and HTML attributes inside div tags with CSS classes "control-group" and "controls".
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="selectList">A collection of System.Web.Mvc.SelectListItem for drop down list field.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="fieldOptions">An object that contains additional options for input field.</param>
        /// <returns>An HTML select element for each property in the object that is represented by the expression.</returns>
        public static MvcHtmlString DropDownFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes = null, FieldOptions fieldOptions = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            var items = EmptyItemAttribute.Resolve(metadata, selectList, SingleEmptyItem);
            items = RemoveItemAttribute.Resolve(metadata, items);

            return new MvcHtmlString(string.Format(@"<div class=""control-group"">{0}<div class=""controls"">{1}{2}{3}</div></div>",
               html.BootStrapLabelFor(expression, fieldOptions: fieldOptions),
               ResolveFieldOptions(fieldOptions, html.DropDownListFor(expression, items, htmlAttributes)),
               HelpTextAttribute.GetHelpTextFor(html, expression),
               "")/*html.ValidationMessageFor(expression))*/
               );
        }

        /// <summary>
        /// Returns HTML string for date picker modal window.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <returns>An HTML string for date picker modal window.</returns>
        public static MvcHtmlString DatePickerModalFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return html.DateTimePickerModalFor(expression, SpecializedType.ResponsiveDatePicker);
        }

        /// <summary>
        /// Returns HTML string for time picker modal window.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <returns>An HTML string for time picker modal window.</returns>
        public static MvcHtmlString TimePickerModalFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return html.DateTimePickerModalFor(expression, SpecializedType.ResponsiveTimePicker);
        }

        private static MvcHtmlString DateTimePickerModalFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, SpecializedType type)
        {
            var fieldName = ExpressionHelper.GetExpressionText(expression);
            var fieldClass = GetDateTimePickerModalClass(fieldName, type);
            var result = string.Format("<div class='{0} {1}'></div>", fieldClass, type.GetData<string>("PopUpCssClass"));

            return MvcHtmlString.Create(result);
        }

        private static string GetDateTimePickerModalClass(string fieldName, SpecializedType type)
        {
            return string.Format("{0}-{1}", fieldName.Replace(".", "-"), type.GetData<string>("PopUpCssClass"));
        }

        private static MvcHtmlString GetEditorFor<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes, FieldOptions fieldOptions)
        {
            var member = expression.Body as MemberExpression;
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);

            if (!fieldOptions.Class.IsIn(FieldClass.Default, FieldClass.None))
            {
                attributes["class"] = fieldOptions.Class.GetDescription();
            }
            if (!attributes.Keys.Contains("class") && fieldOptions.Class != FieldClass.None)
            {
                attributes["class"] = GlobalMVCConfiguration.Instance.DefaultFieldCSS;
            }

            fieldOptions.LabelText = fieldOptions.LabelText.DefaultTo(GetLabelTextFor(html, expression));
            PlaceholderAttribute.Resolve(metadata, fieldOptions, attributes);

            // add conditional display attributes
            if (metadata.AdditionalValues.ContainsKey("ConditionalDisplayPropertyName"))
            {
                attributes["data-conditional-on"] = metadata.AdditionalValues["ConditionalDisplayPropertyName"];
                attributes["data-conditional-values"] = string.Join(",", (object[])metadata.AdditionalValues["ConditionalDisplayPropertyValues"]);
            }

            attributes = FieldAttributesFor(html, expression, attributes);

            ColorPickerAttribute.Resolve(metadata, fieldOptions, attributes);
            fieldOptions.DisplayFormatString = metadata.DisplayFormatString;


            MvcHtmlString result = null;
            string typeName = typeof(TValue).Name;
            Type baseType = typeof(TValue).BaseType;
            if (metadata.IsNullableValueType)
            {
                typeName = typeof(TValue).GetProperty("Value").PropertyType.Name;
                baseType = typeof(TValue).GetProperty("Value").PropertyType.BaseType;
            }
            if (fieldOptions.SelectList != null)
            {
                typeName = "DropDownList";
            }

            switch (typeName)
            {
                case "DateTime":
                    if (fieldOptions.SpecialisedType == SpecializedType.DatePickerCalendar)
                    {
                        DateTime? metadataDateTime = metadata.Model as DateTime?;

                        attributes["class"] += " ignore";
                        attributes["readonly"] = "readonly";

                        attributes["Value"] = metadataDateTime.HasValue ? metadataDateTime.Value.ToString(metadata.DisplayFormatString) : string.Empty;
                        result = html.TextBoxFor(expression, attributes);
                    }
                    else if (fieldOptions.IsResponsiveDateOrTimePicker())
                    {
                        return ResponsivePicker(fieldOptions, attributes, html, expression, metadata);
                    }
                    else result = (metadata.IsReadOnly) ? html.TextBoxFor(expression, metadata.DisplayFormatString, attributes) : html.DatePickerFor(expression, htmlAttributes);
                    break;
                case "Int32":
                    var minMax = IntegerDropDownAttribute.ResolveAttribute(metadata, member);
                    if (minMax != null)
                    {
                        var selectedValue = metadata.Model.ToNullable<int>();
                        int min = minMax.Min;
                        int max = minMax.Max;
                        var intList = Enumerable.Range(min, max - min + 1).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = selectedValue.HasValue && selectedValue.Value == i }).ToList();
                        if (minMax.IsReverse) intList.Reverse();
                        intList = EmptyItemAttribute.Resolve(metadata, intList, SingleEmptyItem).ToList();
                        result = html.CustomDropDownListFor(expression, intList, attributes);
                    }
                    else
                    {
                        if (metadata.DataTypeName == null)
                        {
                            attributes.AddOrSkipIfExists("Type", "number");
                        }
                        result = html.TextBoxFor(expression, metadata.EditFormatString, attributes);
                    }
                    break;
                case "Decimal":
                    if (metadata.DataTypeName == null)
                    {
                        attributes.AddOrSkipIfExists("Type", "number");
                        attributes.Add("Step", "any");
                    }
                    result = html.TextBoxFor(expression, attributes);
                    break;
                case "Boolean":
                    if (metadata.AdditionalValues.ContainsKey("Radio"))
                    {
                        result = html.RadioButtonForBool(expression, metadata.AdditionalValues["RadioTrueText"] as string, metadata.AdditionalValues["RadioFalseText"] as string, htmlAttributes, fieldOptions.DisplayInline);
                    }
                    else if (metadata.AdditionalValues.ContainsKey("ButtonGroup"))
                    {
                        result = html.ButtonGroupForBool(expression, metadata.AdditionalValues["ButtonGroupTrueText"] as string, metadata.AdditionalValues["ButtonGroupFalseText"] as string, htmlAttributes);
                    }
                    else
                    {
                        Expression<Func<TModel, bool>> boolExpression = expression as Expression<Func<TModel, bool>>;
                        result = html.CheckBoxFor(boolExpression, htmlAttributes);
                    }
                    break;
                case "DropDownList":
                    var items = EmptyItemAttribute.Resolve(metadata, fieldOptions.SelectList, SingleEmptyItem);
                    items = RemoveItemAttribute.Resolve(metadata, items);
                    if (metadata.AdditionalValues.ContainsKey("Radio"))
                    {
                        result = html.RadioButtonForList(expression, items.ToSelectList(), attributes, fieldOptions.DisplayInline);
                    }
                    else if (metadata.AdditionalValues.ContainsKey("CheckBox"))
                    {
                        result = html.CheckBoxForFlagEnum(expression, attributes);
                    }
                    else if (metadata.AdditionalValues.ContainsKey("ButtonGroup"))
                    {
                        result = html.ButtonGroupForEnum(expression, htmlAttributes, items);
                    }
                    else
                    {
                        result = html.CustomDropDownListFor(expression, items, attributes);
                    }

                    break;
                case "HttpPostedFileBase":
                    attributes.Add("type", "file");
                    result = html.TextBoxFor(expression, attributes);
                    break;
                case "TimeSpan":
                    attributes["class"] += " timepicker";
                    attributes["Value"] = metadata.Model == null ? "" : new DateTime((metadata.Model as TimeSpan?).Value.Ticks).ToString("HH:mm");
                    result = html.TextBoxFor(expression, attributes);
                    break;
                default:
                    if (baseType == typeof(Enum))
                    {
                        if (metadata.AdditionalValues.ContainsKey("Radio"))
                            result = html.RadioButtonForEnum(expression, attributes, fieldOptions.DisplayInline);
                        else if (metadata.AdditionalValues.ContainsKey("CheckBox"))
                            result = html.CheckBoxForFlagEnum(expression, attributes);
                        else if (metadata.AdditionalValues.ContainsKey("ButtonGroup"))
                            result = html.ButtonGroupForEnum(expression, htmlAttributes);

                        else
                            result = html.EnumDropDownListFor(expression, attributes);
                    }
                    else
                        switch (metadata.DataTypeName)
                        {
                            case "Password":
                                result = html.PasswordFor(expression, attributes);
                                break;
                            case "MultilineText":
                                result = html.TextAreaFor(expression, attributes);
                                break;
                            case "Html":
                                result = html.TextAreaFor(expression, attributes);
                                break;
                            default:
                                result = html.TextBoxFor(expression, metadata.EditFormatString, attributes);
                                break;
                        }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Use StringLength in combination with CharactersLeft to show the characters left countdown
        /// </summary>
        private static MvcHtmlString GetStringLengthFor<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var member = expression.Body as MemberExpression;
            var charactersLeft = member.Member.GetAttribute<CharactersLeftAttribute>(false);
            if (charactersLeft == null) return MvcHtmlString.Empty;
            var stringLength = member.Member.GetAttribute<StringLengthAttribute>(false);
            if (stringLength == null) return MvcHtmlString.Empty;
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var s = metadata.Model as string;
            int used = s == null ? 0 : s.Length;
            return MvcHtmlString.Empty.Format(@"<p class=""pull-right maxlength label"">{0} characters left</p>", stringLength.MaximumLength - used);
        }

        private static MvcHtmlString ResolveFieldOptions(FieldOptions fieldOptions, MvcHtmlString inputControl)
        {
            if (fieldOptions == null)
                return inputControl;

            if (fieldOptions.SpecialisedType == SpecializedType.DatePickerCalendar)
            {
                fieldOptions.AppendedText = fieldOptions.AppendedText.DefaultTo("<i class=\"icon-th\"></i>");
                if (String.IsNullOrEmpty(fieldOptions.DisplayFormatString)) throw new Exception("DatePickerCalendar property is missing [DisplayFormat(DataFormatString = ?)] attribute");
                fieldOptions.WrappedAttributes = new { @class = "date", data_date_format = fieldOptions.DisplayFormatString.ToLower() };
            }
            if (fieldOptions.SpecialisedType == SpecializedType.TimePicker)
            {
                fieldOptions.AppendedText = fieldOptions.AppendedText.DefaultTo("<i class=\"icon-time\"></i>");
                fieldOptions.WrappedAttributes = new { @class = "bootstrap-timepicker-component" };
            }
            if (fieldOptions.IsResponsiveDateOrTimePicker())
            {
                if (fieldOptions.SpecialisedType == SpecializedType.ResponsiveDatePicker)
                {
                    fieldOptions.AppendedText = fieldOptions.AppendedText.DefaultTo("<i class=\"icon-calendar\"></i>"); ;
                }
                else
                {
                    fieldOptions.AppendedText = fieldOptions.AppendedText.DefaultTo("<i class=\"icon-clock\"></i>"); ;
                }
            }
            if (fieldOptions.Label == FieldLabel.CheckBox)
            {
                string format = @"<label class=""checkbox"">{0}&nbsp;{1}</label>";
                return MvcHtmlString.Empty.Format(format, inputControl, fieldOptions.LabelText);
            }
            else if (String.IsNullOrEmpty(fieldOptions.PrependedText) && String.IsNullOrEmpty(fieldOptions.AppendedText) && fieldOptions.WrappedAttributes == null)
            {
                return inputControl;
            }
            else
            {
                RouteValueDictionary attributes = fieldOptions.WrappedAttributes as RouteValueDictionary;
                if (attributes == null) attributes = (fieldOptions.WrappedAttributes != null) ? HtmlHelper.AnonymousObjectToHtmlAttributes(fieldOptions.WrappedAttributes) : new RouteValueDictionary();

                var tagBuilder = new TagBuilder("div");
                tagBuilder.MergeAttributes(attributes);
                if (!String.IsNullOrEmpty(fieldOptions.PrependedText)) tagBuilder.AddCssClass("input-prepend");
                if (!String.IsNullOrEmpty(fieldOptions.AppendedText)) tagBuilder.AddCssClass("input-append");

                string format = @"{0}{1}{2}";

                tagBuilder.InnerHtml = String.Format(format,
                    StringExtensions.FormatIfNotNull(@"<span class=""add-on"">{0}</span>", fieldOptions.PrependedText),
                    inputControl,
                    StringExtensions.FormatIfNotNull(@"<span class=""add-on"">{0}</span>", fieldOptions.AppendedText)
                );

                return MvcHtmlString.Create(tagBuilder.ToString());
            }
        }

        private static string GetControlOptionsString(string expressionText, FieldOptions fieldOptions)
        {
            if (fieldOptions != null)
            {
                if (fieldOptions.IsResponsiveDateOrTimePicker())
                {
                    var modalClass = GetDateTimePickerModalClass(expressionText, fieldOptions.SpecialisedType);
                    var uniqueClass = string.Format("{0}-{1}", expressionText, fieldOptions.SpecialisedType.ToString().ToLower());
                    return string.Format("class=\"control-group {0} {1} {2}\" data-modal=\"{3}\"",
                        fieldOptions.ControlGroupClass,
                        uniqueClass,
                        fieldOptions.SpecialisedType.GetData<string>("ContainerCssClass"),
                        modalClass);
                }
                if (!string.IsNullOrWhiteSpace(fieldOptions.ControlGroupClass))
                {
                    return string.Format("class=\"control-group {0}\"", fieldOptions.ControlGroupClass);
                }
            }
            return "class=\"control-group\"";
        }

        private static MvcHtmlString ResponsivePicker<TModel, TValue>(FieldOptions fieldOptions, RouteValueDictionary attributes, HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, ModelMetadata metadata)
        {
            var previewCssClass = fieldOptions.SpecialisedType.GetData<string>("PreviewCssClass");
            var hiddenCssClass = fieldOptions.SpecialisedType.GetData<string>("HiddenCssClass");
            if (attributes == null)
            {
                attributes = new RouteValueDictionary();
            }
            attributes["class"] = string.Format("{0} {1}", attributes["class"], previewCssClass);
            attributes["readonly"] = "readonly";
            var result = html.TextBox(Guid.NewGuid().ToString(), string.Empty, attributes);
            var options = DateYearRangeAttribute.GetResponsivePickerOptions(metadata);

            if (String.IsNullOrEmpty(fieldOptions.DisplayFormatString)) fieldOptions.DisplayFormatString = "yyyy/MM/dd HH:mm:ss";
            result = MvcHtmlString.Create(string.Format("{0}{1}", result, html.HiddenFor(expression, new { data_options = options.ToJson(), @class = hiddenCssClass, value = String.Format("{0:" + fieldOptions.DisplayFormatString + "}", metadata.Model) })));

            return result;
        }
    }
}