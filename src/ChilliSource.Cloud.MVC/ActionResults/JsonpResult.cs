using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Represents a class that is used to send JSON-formatted content to the response.
    /// </summary>
    public class JsonpResult : JsonResult
    {
        /// <summary>
        /// Enables processing of the result of an action method by a custom type that inherits from the ActionResult class.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes the controller, HTTP content, request context, and route data.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            string jsoncallback = (context.RouteData.Values["jsoncallback"] as string) ?? request["jsoncallback"];
            if (!string.IsNullOrEmpty(jsoncallback))
            {
                if (string.IsNullOrEmpty(base.ContentType))
                {
                    base.ContentType = "application/x-javascript";
                }
                response.Write(string.Format("{0}(", jsoncallback));
            }
            base.ExecuteResult(context);
            if (!string.IsNullOrEmpty(jsoncallback))
            {
                response.Write(")");
            }
        }

        /// <summary>
        /// Creates JSON-formatted content from controller and view result.
        /// </summary>
        /// <param name="controller">The controller name.</param>
        /// <param name="view">The view result.</param>
        /// <returns>JSON-formatted content.</returns>
        public static JsonpResult CreateFromView(Controller controller, ViewResult view)
        {
            string viewString = ControllerExtensions.RenderPartialView(controller, view.ViewName, view.Model);
            return new JsonpResult
            {
                Data = new JsonMessage() { html = viewString },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        /// <summary>
        /// Creates JSON-formatted content from key/value collection of errors.
        /// </summary>
        /// <param name="errors">The collection of errors.</param>
        /// <returns>Returns JSON-formatted content.</returns>
        public static JsonpResult CreateFromErrors(IEnumerable<KeyValuePair<string, string[]>> errors)
        {
            return new JsonpResult
            {
                Data = new JsonMessage() { errors = errors },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }

    /// <summary>
    /// Encapsulates information about the JSON message result.
    /// </summary>
    public class JsonMessage
    {
        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        public string redirect { get; set; }
        /// <summary>
        /// Gets or sets HTNL string.
        /// </summary>
        public string html { get; set; }
        /// <summary>
        /// Gets or sets error object.
        /// </summary>
        public object errors { get; set; }
    }

}