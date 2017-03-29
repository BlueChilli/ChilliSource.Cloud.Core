using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Contains methods for binding model to view.
    /// </summary>
    public static class BinderHelper
    {
        /// <summary>
        /// Gets the object value for the model in the current binding context.
        /// </summary>
        /// <param name="bindingContext">The model binding context.</param>
        /// <returns>The object for the model in the current binding context.</returns>
        public static object GetValue(ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueResult == null) return null;

            object result = (valueResult.RawValue is Array) ? ((Array)valueResult.RawValue).GetValue(0) :
                valueResult.RawValue;

            return result;
        }

        /// <summary>
        /// Gets the strongly typed object by the key from the current binding context.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="bindingContext">the model binding context.</param>
        /// <param name="key">The key to retrieve object.</param>
        /// <returns>The strongly typed object by the key from the current binding context.</returns>
        public static T GetValue<T>(ModelBindingContext bindingContext, string key)
        {
            if (bindingContext.ValueProvider.ContainsPrefix(key))
            {
                ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(key);
                if (valueResult != null)
                {
                    bindingContext.ModelState.SetModelValue(key, valueResult);
                    return (T)valueResult.ConvertTo(typeof(T));
                }
            }
            return default(T);
        }
    }
}