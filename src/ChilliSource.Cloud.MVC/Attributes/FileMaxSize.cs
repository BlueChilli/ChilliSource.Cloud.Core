using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates the Max size for uploaded files. This must be applied on HttpPostedFileBase fields.
    /// </summary>
    public class FileMaxSizeAttribute : ValidationAttribute, IClientValidatable
    {
        public FileMaxSizeAttribute(long maxSizeinBytes)
        {
            MaxSize = maxSizeinBytes;
            ErrorMessage = "The {0} field: The size of the file selected must be less than {1}.";
        }

        /// <summary>
        /// Max size in bytes
        /// </summary>
        public long MaxSize { get; private set; }

        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;
            if (file != null)
            {
                return file.ContentLength < MaxSize;
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(ErrorMessage, name, FormatBytes(MaxSize));
        }

        private static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.DisplayName),
                ValidationType = "filemaxsize"
            };
            rule.ValidationParameters["filemaxsize"] = MaxSize;
            yield return rule;
        }
    }
}
