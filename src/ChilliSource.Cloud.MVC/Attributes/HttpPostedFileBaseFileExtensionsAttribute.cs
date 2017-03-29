using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates the extension of uploaded files.
    /// </summary>
    public class HttpPostedFileBaseFileExtensionsAttribute : ValidationAttribute, IMetadataAware
    {
        /// <summary>
        /// Validates the extension of uploaded files. Defaults allowedExtensions to jpg, jpeg, png, gif.
        /// </summary>
        public HttpPostedFileBaseFileExtensionsAttribute()
        {
            Extensions = "jpg, jpeg, png, gif";
            ErrorMessage = "Field {0} is not a valid image type ({1})";
        }

        /// <summary>
        /// Validates the extension of uploaded files.
        /// </summary>
        /// <param name="allowedExtensions">Allowed extensions separated by comma</param>
        public HttpPostedFileBaseFileExtensionsAttribute(string allowedExtensions)
        {
            Extensions = allowedExtensions.ToLower();
            ErrorMessage = "Field {0} is not one of following valid extensions ({1})";
        }

        public string Extensions { get; set; }

        private List<string> GetExtensions()
        {
            return Extensions.Split(',').Select(s => "." + s.Trim()).ToList();
        }



        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;
            if (file != null)
            {
                return GetExtensions().Contains(Path.GetExtension(file.FileName).ToLower());
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            string errorMessage = String.Join(", ", GetExtensions());
            return String.Format(ErrorMessage, name, errorMessage.TrimEnd(' ', ','));
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["FileExtensions"] = Extensions.Replace(" ", "");
        }


        public static void Resolve(ModelMetadata metadata, RouteValueDictionary attributes)
        {
            if (metadata.AdditionalValues.ContainsKey("FileExtensions"))
            {
                Resolve(metadata.AdditionalValues["FileExtensions"].ToString(), attributes);
            }
        }

        public static void Resolve(string extensionsData, RouteValueDictionary attributes)
        {
            attributes["extensions"] = extensionsData.Replace(" ", "");
            var extensions = attributes["extensions"].ToString().Split(',').Select(s => "." + s).ToList();
            var mimeTypes = new List<string>();
            foreach (var extension in extensions)
            {
                var mimeType = MimeMapping.GetMimeMapping("dummy" + extension);
                if (mimeType.Equals("application/octet-stream", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!mimeTypes.Contains(extension))
                    {
                        mimeTypes.Add(extension);
                    }
                }
                else if (!mimeTypes.Contains(mimeType))
                {
                    mimeTypes.Add(mimeType);
                }
            }
            attributes["accept"] = String.Join(", ", mimeTypes);
        }
    }
}
