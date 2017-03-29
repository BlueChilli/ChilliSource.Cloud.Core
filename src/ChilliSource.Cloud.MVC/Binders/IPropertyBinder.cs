using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Abstract interface for property binders.
    /// </summary>
    public interface IPropertyBinder
    {
        /// <summary>
        /// Implement a custom property binding here.
        /// Use propertyDescriptor.SetValue(bindingContext.Model, value) to set a new value.
        /// </summary>
        /// <param name="value">Original value</param>
        void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, ValueProviderResult valueProviderResult);
    }
}
