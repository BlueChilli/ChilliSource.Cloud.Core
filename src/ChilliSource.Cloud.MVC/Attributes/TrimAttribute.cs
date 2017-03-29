using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Properties marked with this attribute will be trimmed.
    /// </summary>
    public class TrimAttribute : Attribute, IMetadataAware
    {
        public TrimAttribute() { }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["ShouldTrim"] = true;
        }
    }
}
