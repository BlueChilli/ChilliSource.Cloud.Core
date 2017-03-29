using ImageResizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Resizes an image file server-side when posted.
    /// </summary>
    public class HttpPostedImageResizeAttribute : Attribute, IPropertyBinderProvider
    {
        /// <summary>
        /// Default contructor.
        /// </summary>
        public HttpPostedImageResizeAttribute() { }

        /// <summary>
        /// <para>Image resizer instructions. It's a good idea to specify "quality=100" when uploading an image to avoid reducing the quality twice when displaying it.</para>
        /// <para>See http://imageresizing.net/docs/reference and http://imageresizing.net/docs/basics.</para>
        /// <para>e.g autorotate=true&amp;maxwidth=1024&amp;maxheight=768&amp;quality=100</para>
        /// </summary>
        public string Instructions { get; set; }

        IPropertyBinder IPropertyBinderProvider.CreateBinder()
        {
            return new HttpPostedImageResizeAttribute.Binder() { Instructions = this.Instructions };
        }

        private class Binder : IPropertyBinder
        {
            public string Instructions { get; set; }
            public void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, ValueProviderResult valueProviderResult)
            {
                if (valueProviderResult == null)
                    return;

                if (propertyDescriptor.PropertyType != typeof(HttpPostedFileBase))
                {
                    throw new ApplicationException("PostedImageResizeAttribute - The property type must be HttpPostedFileBase.");
                }

                var collection = valueProviderResult.RawValue as IEnumerable<HttpPostedFileBase>;
                if (collection == null)
                    return;

                var value = collection.FirstOrDefault();
                if (value == null)
                    return;

                if (!String.IsNullOrEmpty(this.Instructions))
                {
                    var resizedImg = new MemoryStream();
                    ImageJob i = new ImageJob(value.InputStream, resizedImg, new Instructions(this.Instructions));
                    i.Build();
                    resizedImg.Position = 0;
                    value = new ResizedImageFileWrapper(value.FileName, resizedImg, value.ContentType);
                }

                propertyDescriptor.SetValue(bindingContext.Model, value);
            }
        }

        private class ResizedImageFileWrapper : HttpPostedFileBase
        {
            public ResizedImageFileWrapper(string filename, Stream inputStream, string contentType)
            {
                _FileName = filename;
                _ContentType = contentType;
                _InputStream = inputStream;
                _ContentLength = (int)inputStream.Length;
            }

            int _ContentLength;
            string _ContentType;
            string _FileName;
            Stream _InputStream;

            public override int ContentLength { get { return _ContentLength; } }
            public override string ContentType { get { return _ContentType; } }
            public override string FileName { get { return _FileName; } }
            public override Stream InputStream { get { return _InputStream; } }

            public override void SaveAs(string filename)
            {
                using (var file = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    _InputStream.CopyTo(file);

                    file.Flush();
                    file.Close();
                }
            }
        }
    }
}
