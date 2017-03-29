using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Extension methods for BlueChilli.Model.ServiceResult&lt;T&gt;.
    /// </summary>
    public static class ServiceResultExtensions
    {
        /// <summary>
        /// Adds the specified BlueChilli.Model.ServiceResult&lt;T&gt; to controller.
        /// </summary>
        /// <typeparam name="T">The type of object in BlueChilli.Model.ServiceResult.</typeparam>
        /// <param name="svcResult">The specified BlueChilli.Model.ServiceResult&lt;T&gt;.</param>
        /// <param name="controller">The specified controller.</param>
        /// <returns>A BlueChilli.Model.ServiceResult&lt;T&gt;.</returns>
        public static ServiceResult<T> AddToModelState<T>(this ServiceResult<T> svcResult, Controller controller)
        {
            if (!svcResult.Success)
            {
                controller.ModelState.AddResult(svcResult);
            }

            return svcResult;
        }
    }
}