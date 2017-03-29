using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Removes value from dropdown if present. 
    /// One of the following properties can be used (priority list): EnumValue, EnumValues, Value, Values, Text, Texts
    /// </summary>
    public class RemoveItemAttribute : Attribute, IMetadataAware
    {
        public object EnumValue { get; set; }
        public object[] EnumValues { get; set; }
        public string Value { get; set; }
        public string[] Values { get; set; }
        public string Text { get; set; }
        public string[] Texts { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            if (EnumValue != null || EnumValues != null)
            {
                metadata.AdditionalValues["RemoveItem-Value"] = EnumValue != null ? new string[] { EnumValue.ToString() } : EnumValues.Select(e => e.ToString()).ToArray(); 
            }
            if (!String.IsNullOrEmpty(Value) || Values != null)
            {
                metadata.AdditionalValues["RemoveItem-Value"] = !String.IsNullOrEmpty(Value) ? new string[] { Value } : Values;
            }
            if (!String.IsNullOrEmpty(Text) || Texts != null)
            {
                metadata.AdditionalValues["RemoveItem-Text"] = !String.IsNullOrEmpty(Text) ? new string[] { Text } : Texts;
            }
        }

        public static IList<SelectListItem> Resolve(ModelMetadata metadata, IEnumerable<SelectListItem> items)
        {
            var list = items.ToList();
            if (metadata.AdditionalValues.ContainsKey("RemoveItem-Value"))
            {
                var toRemove = metadata.AdditionalValues["RemoveItem-Value"] as string[];
                list.RemoveAll(l => toRemove.Contains(l.Value));
            }
            if (metadata.AdditionalValues.ContainsKey("RemoveItem-Text"))
            {
                var toRemove = metadata.AdditionalValues["RemoveItem-Text"] as string[];
                list.RemoveAll(l => toRemove.Contains(l.Text));
            }
            return list;
        }
    }
}
