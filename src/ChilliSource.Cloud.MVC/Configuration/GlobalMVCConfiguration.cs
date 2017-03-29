
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC
{
    public class GlobalMVCConfiguration
    {
        private static readonly GlobalMVCConfiguration _instance = new GlobalMVCConfiguration();
        public static GlobalMVCConfiguration Instance { get { return _instance; } }

        private GlobalMVCConfiguration() { }

        public string StylesDirectory { get; set; } = "~/Styles/";
        public string ImagesDirectory { get; set; } = "~/Images/";
        public string ScriptsDirectory { get; set; } = "~/Scripts/";
        public string DefaultFieldCSS { get; internal set; }

        internal string GetPath(DirectoryType type, string filename)
        {
            return GetDirectory(type) + filename;
        }

        /// <summary>
        /// Get directory path from configuration file based on the BlueChilli.Lib.Configuration.DirectoryType.
        /// </summary>
        /// <param name="type">A BlueChilli.Lib.Configuration.DirectoryType enum that represents directory type.</param>
        /// <returns>The directory path from configuration file.</returns>
        internal string GetDirectory(DirectoryType type)
        {
            switch (type)
            {
                case DirectoryType.Images: return ImagesDirectory;
                case DirectoryType.Scripts: return ScriptsDirectory;
                case DirectoryType.Styles: return StylesDirectory;
            }

            throw new ApplicationException($"DirectoryType '{type.ToString()}' not supported.");
        }
    }
}
