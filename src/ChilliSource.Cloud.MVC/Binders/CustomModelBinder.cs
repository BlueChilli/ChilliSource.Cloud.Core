using System;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel;
using System.Collections.Generic;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// MVC doesn't bind a string[] to enum marked with flags attributes. This fixes this.
    /// </summary>
    public class CustomModelBinder : DefaultModelBinder
    {
        public bool TrimAllStrings { get; set; }

        protected override object GetPropertyValue(
           ControllerContext controllerContext,
           ModelBindingContext bindingContext,
           PropertyDescriptor propertyDescriptor,
           IModelBinder propertyBinder)
        {
            var propertyType = propertyDescriptor.PropertyType;

            if (bindingContext.ModelType == typeof(string) && (bindingContext.ModelMetadata.AdditionalValues.ContainsKey("ShouldTrim") || this.TrimAllStrings))
            {
                var providerValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                var sValue = ((string)providerValue.AttemptedValue);
                return (sValue == null ? null : sValue.Trim());
            }

            // Check if the property type is an enum with the flag attribute
            if (propertyType.IsEnum && propertyType.IsDefined(typeof(FlagsAttribute), true))
            {
                var providerValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (providerValue != null)
                {
                    var value = providerValue.RawValue;
                    if (value != null)
                    {
                        // In case it is a checkbox list/dropdownlist/radio 
                        // button list
                        if (value is string[])
                        {
                            // Create flag value from posted values
                            var flagValue = ((string[])value)
                                .Aggregate(0, (acc, i)
                                    => acc | (int)Enum.Parse(propertyType, i));

                            return Enum.ToObject(propertyType, flagValue);
                        }

                        // In case it is a single value
                        if (value.GetType().IsEnum)
                        {
                            return Enum.ToObject(propertyType, value);
                        }
                    }
                }
            }

            return base.GetPropertyValue(controllerContext,
                bindingContext,
                propertyDescriptor,
                propertyBinder);
        }

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var keyName = !String.IsNullOrEmpty(bindingContext.ModelName) ? String.Concat(bindingContext.ModelName, ".", propertyDescriptor.Name) : propertyDescriptor.Name;

            ValueProviderResult valueProviderResult = null;
            if (bindingContext.ValueProvider is ValueProviderCollection)
            {
                var skipValidation = propertyDescriptor.Attributes.OfType<AllowHtmlAttribute>().Any();
                valueProviderResult = ((ValueProviderCollection)bindingContext.ValueProvider).GetValue(keyName, skipValidation);
            }
            else valueProviderResult = bindingContext.ValueProvider.GetValue(keyName);

            var count = 0;
            foreach (var binderProvider in propertyDescriptor.Attributes.OfType<IPropertyBinderProvider>())
            {
                count++;
                binderProvider.CreateBinder().BindProperty(controllerContext, bindingContext, propertyDescriptor, valueProviderResult);
            }

            if (count == 0)
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }
    }
}