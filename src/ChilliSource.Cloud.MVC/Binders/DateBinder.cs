using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Contains model binding methods for System.DateTime.
    /// </summary>
    public class DateBinder : IModelBinder
    {
        /// <summary>
        ///  FieldFor version of date control does not emmit hidden fields - see DateFormat. Set AllFieldsPosted to true to work with Inspinia Date template.
        /// </summary>
        public bool AllFieldsPosted { get; set; }
        /// <summary>
        /// Binds the model for System.DateTime object.
        /// </summary>
        /// <param name="controllerContext">The context within which the controller operates.</param>
        /// <param name="bindingContext">The context within which the model is bound.</param>
        /// <returns>The bound System.DateTime object.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            // Ensure there's incoming data
            var key = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(key);
            if (valueProviderResult == null || string.IsNullOrEmpty(valueProviderResult.AttemptedValue))
                return null;

            if (valueProviderResult.RawValue is DateTime) return valueProviderResult.RawValue;

            // Preserve it in case we need to redisplay the form
            bindingContext.ModelState.SetModelValue(key, valueProviderResult);

            // Parse
            var dateParts = valueProviderResult.RawValue as string[];
            if (dateParts == null && valueProviderResult.RawValue is string)
            {
                dateParts = new string[] { (string)valueProviderResult.RawValue };
            }

            if (dateParts.Length == 1)
            {
                var displayFormat = bindingContext.ModelMetadata.DisplayFormatString;
                var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

                if (!String.IsNullOrWhiteSpace(displayFormat) && value != null && value.AttemptedValue != null)
                {
                    DateTime date;

                    if (DateTime.TryParseExact(value.AttemptedValue, displayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        return date;

                    bindingContext.ModelState.AddModelError(
                        bindingContext.ModelName,
                        string.Format("{0} is an invalid date format", value.AttemptedValue)
                        );

                    return null;
                }

                return DateTime.Parse(dateParts[0]);
            }

            string day = "1", month = "-1", year = "-1", hour = "0", minute = "0";

            int partIndex = 0;

            var metadata = bindingContext.ModelMetadata;
            try // if date isn't set this will error, that's fine, we'll exit below and return null
            {
                if (AllFieldsPosted)
                {
                    day = dateParts[0];
                    month = dateParts[1];
                    year = dateParts[2];
                    if (dateParts.Length >= 4) hour = dateParts[3];
                    if (dateParts.Length >= 5) minute = dateParts[4];
                }
                else
                {
                    if (GetMetaData(metadata, "Day", true)) { day = dateParts[partIndex]; partIndex++; }
                    if (GetMetaData(metadata, "Month", true)) { month = dateParts[partIndex]; partIndex++; }
                    if (GetMetaData(metadata, "Year", true)) { year = dateParts[partIndex]; partIndex++; } else { year = DateTime.Now.Year.ToString(); }
                    if (GetMetaData(metadata, "Hour", false)) { hour = dateParts[partIndex]; partIndex++; }
                    if (GetMetaData(metadata, "Minute", false)) { minute = dateParts[partIndex]; partIndex++; }
                }
            }
            catch { }

            if (day == "-1" || month == "-1" || year == "-1")
                return null;
            try
            {
                return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), 0);
            }
            catch // problem parsing date, must return null
            {
                return null;
            }
        }

        private static bool GetMetaData(ModelMetadata metadata, string name, bool defaultAs = false)
        {
            return (metadata.AdditionalValues.SingleOrDefault(m => m.Key == "DateShow" + name).Value as bool?).GetValueOrDefault(defaultAs);
        }
    }
}