using System.IO;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Extension methods for System.Web.Mvc.Controller.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Renders specified controller, view and model object to HTML string.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <param name="viewName">The specified view name.</param>
        /// <param name="model">The specified model object.</param>
        /// <returns>The HTML string after rendering.</returns>
        public static string RenderPartialView(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}