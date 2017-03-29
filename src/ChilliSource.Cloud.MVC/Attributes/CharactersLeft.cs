using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Use StringLength in combination with CharactersLeft to show the characters left countdown close to a string input field.
    /// </summary>
    public class CharactersLeftAttribute : Attribute, IMetadataAware
    {
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["CharactersLeft"] = true;
        }

        public static string Resolve(ModelMetadata metadata, FieldOptions fieldOptions, RouteValueDictionary attributes)
        {
            attributes["charactersleft"] = true;
            return String.Empty;
        }
    }
}
