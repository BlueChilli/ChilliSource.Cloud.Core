using System;
using System.Globalization;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Contains model binding methods for System.Int32 object.
    /// </summary>
    public class Int32Binder : IModelBinder
    {
        /// <summary>
        /// Binds the model for System.Int32 object.
        /// </summary>
        /// <param name="controllerContext">The context within which the controller operates.</param>
        /// <param name="bindingContext">The context within which the model is bound.</param>
        /// <returns>The bound System.Int32 object.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = BinderHelper.GetValue(bindingContext);

            if (value == null) return null;

            if (value is Int32) return value;

            var result = 0;
            if (value is string && Int32.TryParse((string)value, NumberStyles.Currency, null, out result))
                return result;

            return null;
        }
    }
}