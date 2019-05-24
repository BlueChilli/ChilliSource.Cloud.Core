using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliSource.Cloud.Core
{
    public class LocalStorageConfiguration
    {
        public string BasePath { get; set; }

        internal LocalStorageConfiguration Clone()
        {
            var clone = (LocalStorageConfiguration)this.MemberwiseClone();
            return clone;
        }
    }
}
