using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Encapsulates functions for streaming any type of binary data as the ActionResult.
    /// </summary>
    public class BinaryResult : ActionResult
    {
        private byte[] _fileBinary;
        private string _contentType;
        private string _fileName;

        /// <summary>
        /// Initializes a new instance of the BinaryResult class by using the specified file name, content and content type.
        /// </summary>
        /// <param name="fileBinary">The file content in byte array.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="fileName">The file name.</param>
        public BinaryResult(byte[] fileBinary, string contentType, string fileName = "")
        {
            _fileBinary = fileBinary;
            _contentType = contentType;
            _fileName = fileName;
        }

        /// <summary>
        /// Enables processing of the result of an action method by a custom type that inherits from the ActionResult class.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes the controller, HTTP content, request context, and route data.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.ContentType = _contentType;
            if (_fileName != "")
                context.HttpContext.Response.AddHeader("Content-Disposition", "filename=\"" + _fileName+ "\"");

            if (_fileBinary != null)
            {
                context.HttpContext.Response.BinaryWrite(_fileBinary);
            }
        }
    }
}