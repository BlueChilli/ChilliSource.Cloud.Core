
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Contains properties related to an image returned for a request to the Image SourceType.
    /// </summary>
    public class ImageResult : ActionResult
    {
        private Image _image;
        private ImageFormat _format;
        private string _fileName;

        /// <summary>
        /// Initializes a new instance of the ImageResult class by using the specified Image, Image format and file name.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="format">The image format.</param>
        /// <param name="fileName">The image file name.</param>
        public ImageResult(Image image, ImageFormat format = null, string fileName = "")
        {
            _image = image;
            _format = format == null ? image.RawFormat : format;
            _fileName = fileName;
        }

        /// <summary>
        /// Enables processing of the result of an action method by a custom type that inherits from the ActionResult class.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes the controller, HTTP content, request context, and route data.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();

            if (!String.IsNullOrEmpty(_fileName))
            {
                context.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", _fileName.ToFileName()));
            }
            context.HttpContext.Response.ContentType = MimeMapping.GetMimeMapping(_format.FileExtension());
            _image.Save(context.HttpContext.Response.OutputStream, _format);
        }
    }
}
