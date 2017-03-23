using ChilliSource.Cloud.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChilliSource.Cloud.Web.Infrastructure
{
    public class WebMimeMapping : IMimeMapping
    {
        public string GetMimeType(string fileName)
        {
            return MimeMapping.GetMimeMapping(fileName);
        }
    }
}
