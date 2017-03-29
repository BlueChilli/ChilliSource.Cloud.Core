using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Insert an empty item into a dropdown list (either enum or custom based). Use a nullable enum to avoid binding issues
    /// </summary>
    public class EmptyItemAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Text displayed in the empty item
        /// </summary>
        public string Text { get; set; }

        public EmptyItemAttribute(string text)
        {
            Text = text;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["EmptyItem-Text"] = Text;
        }

        public static IList<SelectListItem> Resolve(ModelMetadata metadata, IEnumerable<SelectListItem> items, IEnumerable<SelectListItem> singleEmptyItem)
        {
            if (items == null) items = ArrayExtensions.EmptyArray<SelectListItem>();

            if (metadata.AdditionalValues.ContainsKey("EmptyItem-Text"))
            {
                var emptyItem = new[] { new SelectListItem { Text = metadata.AdditionalValues["EmptyItem-Text"].ToString(), Value = "" } };
                return emptyItem.Concat(items).ToList();
            }
 
            if (metadata.IsNullableValueType)
            {
                if (singleEmptyItem == null)
                    singleEmptyItem = Enumerable.Empty<SelectListItem>();

                return singleEmptyItem.Concat(items).ToList();
            }

            return items.ToList();
        }
    }
}
