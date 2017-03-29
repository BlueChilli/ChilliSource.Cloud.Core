using ChilliSource.Cloud.Core;
using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        private static string ResolveFilenameToUrl(HtmlHelper html, DirectoryType directoryType, string filename, string alternativeImage = "")
        {
            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);
            return ResolveFilenameToUrl(urlHelper, directoryType, filename, alternativeImage);
        }

        private static string ResolveFilenameToUrl(UrlHelper urlHelper, DirectoryType directoryType, string filename, string alternativeImage = "")
        {
            string url = "";
            filename = StringExtensions.DefaultTo(filename, alternativeImage);
            if (String.IsNullOrEmpty(filename)) return "";

            if (filename.StartsWith("~"))
            {
                url = urlHelper.Content(filename);
            }
            else if (filename.StartsWith("http://") || filename.StartsWith("https://") || filename.StartsWith("//"))
            {
                url = filename;
            }
            else
            {                
                url = GlobalMVCConfiguration.Instance.GetPath(directoryType, filename);
            }

            return urlHelper.Content(url);
        }       
    }
}