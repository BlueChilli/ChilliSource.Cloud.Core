using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using ChilliSource.Cloud.Core;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Manages the HTTP response headers to provide information about HTML document.
    /// </summary>
    public static class ResponseHeader
    {
        /// <summary>
        /// Adds an HTTP header to the current response and returns a new instance of the System.Web.Mvc.FileContentResult class
        /// by using the specified file contents and csv content type.
        /// </summary>
        /// <param name="response">The HTTP-response information.</param>
        /// <param name="filename">The file name.</param>
        /// <param name="content">The file content.</param>
        /// <returns>An instance of the System.Web.Mvc.FileContentResult class by using the specified file contents and csv content type.</returns>
        public static FileContentResult WriteCsvResponseHeader(HttpResponseBase response, string filename, string content)
        {
            response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", filename.ToFileName()));
            return new FileContentResult(content.ToByteArray(), "text/comma-separated-values");
        }

        /// <summary>
        /// Adds an HTTP header to the current response and returns a new instance of the System.Web.Mvc.FileContentResult class
        /// by using the specified file contents and pdf content type.
        /// </summary>
        /// <param name="response">The HTTP-response information.</param>
        /// <param name="filename">The file name.</param>
        /// <param name="pdf">The file content in byte array.</param>
        /// <returns>An instance of the System.Web.Mvc.FileContentResult class by using the specified file contents and pdf content type.</returns>
        public static FileContentResult WritePdfResponseHeader(HttpResponseBase response, string filename, byte[] pdf)
        {
            response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", filename.ToFileName()));
            return new FileContentResult(pdf, "application/pdf");
        }

        /// <summary>
        /// Adds an HTTP header to the current response and returns a new instance of the System.Web.Mvc.FileStreamResult class
        /// by using the specified file contents and pdf content type. 
        /// </summary>
        /// <param name="response">The HTTP-response information.</param>
        /// <param name="filename">The file name.</param>
        /// <param name="fileStream">The file content in sequence of bytes.</param>
        /// <returns>An instance of the System.Web.Mvc.FileStreamResult class by using the specified file stream contents and pdf content type.</returns>
        public static FileStreamResult WritePdfResponseHeader(HttpResponseBase response, string filename, Stream fileStream)
        {
            response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", filename.ToFileName()));
            return new FileStreamResult(fileStream, "application/pdf");
        }

        /// <summary>
        /// Add correct mime header for file type (based on passed in file extension) to response stream.
        /// </summary>
        /// <param name="response">The ResponseHeader.</param>
        /// <param name="filename">The filename user will download, generally insert date into this.</param>
        /// <param name="content">The file content in byte array.</param>
        /// <returns>FileContentResult - which should also be the returning value of your action method.</returns>
        public static FileContentResult WriteResponseHeader(HttpResponseBase response, string filename, byte[] content)
        {
            response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", filename.ToFileName()));
            return new FileContentResult(content, MimeMapping.GetMimeMapping(filename));
        }

        /// <summary>
        /// Add correct mime header for file type (based on passed in file extension) to response stream.
        /// </summary>
        /// <param name="response">The ResponseHeader.</param>
        /// <param name="filename">The filename user will download, generally insert date into this.</param>
        /// <param name="stream">The file content in sequence of bytes.</param>
        /// <returns>FileStreamResult - which should also be the returning value of your action method.</returns>
        public static FileStreamResult WriteResponseHeader(HttpResponseBase response, string filename, Stream stream)
        {
            response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", filename.ToFileName()));
            return new FileStreamResult(stream, MimeMapping.GetMimeMapping(filename));
        }
    }
}
