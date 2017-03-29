using System;
using System.Globalization;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Contains model binding methods for decimal object.
    /// </summary>
    public class DecimalBinder : IModelBinder
    {
        /// <summary>
        /// Binds the model for decimal object.
        /// </summary>
        /// <param name="controllerContext">The context within which the controller operates.</param>
        /// <param name="bindingContext">The context within which the model is bound.</param>
        /// <returns>The bound decimal object.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = BinderHelper.GetValue(bindingContext);

            if (value == null) return null;

            if (value is decimal) return value;

            var result = 0M;
            if (value is string && Decimal.TryParse((string)value, NumberStyles.Currency, null, out result)) 
                return result;

            return null;
        }
    }
}