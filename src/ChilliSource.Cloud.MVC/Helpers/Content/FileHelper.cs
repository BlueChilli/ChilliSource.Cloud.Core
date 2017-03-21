using ChilliSource.Cloud.Extensions;
using ChilliSource.Cloud.Configuration;
using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.MVC
{
    public static partial class Helpers
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
                url = GetPath(directoryType, filename);
            }
            return urlHelper.Content(url);
        }       
    }
}