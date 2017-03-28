using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    public interface IMimeMapping
    {
        string GetMimeType(string fileName);
    }
}
