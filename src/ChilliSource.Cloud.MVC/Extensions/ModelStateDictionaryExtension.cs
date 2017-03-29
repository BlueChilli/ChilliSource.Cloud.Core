using ChilliSource.Cloud.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Extension methods for System.Web.Mvc.ModelStateDictionary.
    /// </summary>
    public static class ModelStateDictionaryExtension
    {
        /// <summary>
        /// Gets a collection of errors from the specified model state.
        /// </summary>
        /// <param name="modelState">The specified model state.</param>
        /// <returns>A collection of errors.</returns>
        public static IEnumerable<KeyValuePair<string, string[]>> Errors(this ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                return modelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()).Where(m => m.Value.Count() > 0);
            }
            return null;
        }

        /// <summary>
        /// Adds error in BlueChilli.Model.ServiceResult&lt;T&gt; to the specified model state.
        /// </summary>
        /// <typeparam name="T">The type of object in BlueChilli.Model.ServiceResult.</typeparam>
        /// <param name="modelState">The specified model state.</param>
        /// <param name="serviceResult">The specified BlueChilli.Model.ServiceResult&lt;T&gt;.</param>
        public static void AddResult<T>(this ModelStateDictionary modelState, ServiceResult<T> serviceResult)
        {
            if (serviceResult != null && !String.IsNullOrEmpty(serviceResult.Error))
                modelState.AddModelError(serviceResult.Key ?? "", serviceResult.Error);
        }

        /// <summary>
        /// Removes the expression model name from specified model state.
        /// </summary>
        /// <typeparam name="TModel">The type of the model to remove.</typeparam>
        /// <param name="modelState">The specified model state.</param>
        /// <param name="expression">A lambda expression to get model name.</param>
        public static void RemoveFor<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression)
        {
            string expressionText = ExpressionHelper.GetExpressionText(expression);

            foreach (var msCopy in modelState.ToArray())
            {
                if (msCopy.Key.Equals(expressionText) || msCopy.Key.StartsWith(expressionText + "."))
                {
                    modelState.Remove(msCopy);
                }
            }
        }
    }
}