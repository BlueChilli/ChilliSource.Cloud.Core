using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        public static MvcHtmlString ImgEmbedded(this HtmlHelper html, byte[] data, string altText = null, object htmlAttributes = null)
        {
            //TODO:
            throw new NotImplementedException("ImgEmbedded not implemented");
        }
    }
}

//
//
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Configuration;
//using System.IO;
//using System.Web;
//using System.Web.Mvc;
//using System.Linq;
//using System.Text;

//namespace ChilliSource.Cloud.Web.MVC
//{
//    public static partial class HtmlHelperExtensions
//    {
//        /// <summary>
//        /// Returns HTML string for the image element.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="width">The width of the image.</param>
//        /// <param name="height">The height of the image.</param>
//        /// <param name="altText">The alternate text of the image.</param>
//        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>An HTML-encoded string for the image element.</returns>
//        public static MvcHtmlString Img(this HtmlHelper html, string filename, int? width = null, int? height = null, string altText = null, object htmlAttributes = null, string alternativeImage = "")
//        {
//            return Img(html, filename, new ImageResizerCommand { Width = width, Height = height, AutoRotate = false }, altText, htmlAttributes, alternativeImage);
//        }

//        /// <summary>
//        /// Returns HTML string for the image element.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="altText">The alternate text of the image.</param>
//        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>An HTML-encoded string for the image element.</returns>
//        public static MvcHtmlString Img(this HtmlHelper html, string filename, ImageResizerCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "")
//        {
//            TagBuilder builder = new TagBuilder("img");

//            builder.Attributes.Add("src", S3Query(ResolveFilenameToUrl(html, DirectoryType.Images, filename, alternativeImage), cmd));
//            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);
//            if (cmd.Width.HasValue) builder.Attributes.Add("width", cmd.Width.Value.ToString());
//            if (cmd.Height.HasValue) builder.Attributes.Add("height", cmd.Height.Value.ToString());
//            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
//            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
//        }

//        /// <summary>
//        /// Returns the fully qualified URL for the specified image file.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The file name of the image.</param>
//        /// <returns>A fully qualified URL for the specified image file.</returns>
//        public static string ImgUrl(this HtmlHelper html, string filename)
//        {
//            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);
//            return ResolveFilenameToUrl(urlHelper, DirectoryType.Images, filename);
//        }

//        /// <summary>
//        /// Returns the fully qualified URL for the specified image file.
//        /// </summary>
//        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper instance that this method extends.</param>
//        /// <param name="filename">The file name of the image.</param>
//        /// <returns>A fully qualified URL for the specified image file.</returns>
//        public static string ImgUrl(this UrlHelper urlHelper, string filename)
//        {
//            return ResolveFilenameToUrl(urlHelper, DirectoryType.Images, filename);
//        }

//        /// <summary>
//        /// Embeds image into page using src:data with base64 encoded image data.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="data">Raw image data.</param>
//        /// <param name="altText">Optional alt text.</param>
//        /// <param name="htmlAttributes">Optional attribute to include in the img tag.</param>
//        /// <returns>An image tag with image encoded as base64.</returns>
//        public static MvcHtmlString ImgEmbedded(this HtmlHelper html, byte[] data, string altText = null, object htmlAttributes = null)
//        {
//            TagBuilder builder = new TagBuilder("img");

//            var srcFormat = "data:{mimeType};base64,{data}";
//            var mimeType = data.ToImage().GetMimeType();
//            var base64Data = Convert.ToBase64String(data);
//            builder.Attributes.Add("src", srcFormat.TransformWith(new { mimeType = mimeType, data = base64Data }));
//            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);
//            //if (cmd.Width.HasValue) builder.Attributes.Add("width", cmd.Width.Value.ToString());
//            //if (cmd.Height.HasValue) builder.Attributes.Add("height", cmd.Height.Value.ToString());
//            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
//            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
//        }

//        private static string ResolveProtocol(string url, string protocol)
//        {
//            return String.IsNullOrEmpty(protocol) ? url : UrlHelperExtensions.Create().GenerateExternalUrl(url, protocol);
//        }

//        #region S3

//        /// <summary>
//        ///     Returns the url the S3 image (Image stored in Amazon S3 storage).
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>The S3 image url</returns>
//        public static IHtmlString ImgS3Url(this HtmlHelper html, string filename, ImageResizerCommand cmd, string protocol = "", string alternativeImage = "")
//        {
//            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);
//            var s3Config = ProjectConfigurationSection.GetConfig().FileStorage?.S3;
//            return html.Raw(html.ImgUrl(S3Path(filename, cmd, s3Config, protocol, alternativeImage)));
//        }

//        /// <summary>
//        /// Returns HTML string for the S3 image element (Image stored in Amazon S3 storage).
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="width">The width of the image.</param>
//        /// <param name="height">The height of the image.</param>
//        /// <param name="altText">The alternate text of the image.</param>
//        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>An HTML-encoded string for the S3 image element.</returns>
//        public static MvcHtmlString ImgS3(this HtmlHelper html, string filename, int? width, int? height, string altText = null, object htmlAttributes = null, string alternativeImage = "")
//        {
//            return ImgS3(html, filename, new ImageResizerCommand { Width = width, Height = height }, altText, htmlAttributes, alternativeImage);
//        }

//        /// <summary>
//        /// Returns HTML string for the S3 image element (Image stored in Amazon S3 storage).
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="altText">The alternate text of the image.</param>
//        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <param name="ensureSize">Specifies whether width and height attributes should be generated in the 'img' tag. Defaults to true (compatibility).</param>
//        /// <returns>An HTML-encoded string for the S3 image element.</returns>
//        public static MvcHtmlString ImgS3(this HtmlHelper html, string filename, ImageResizerCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "", bool ensureSize = true)
//        {
//            if (cmd == null) cmd = new ImageResizerCommand();
//            if (String.IsNullOrEmpty(filename)) return html.Img(S3Query(alternativeImage, cmd), cmd.Width, cmd.Height, altText, htmlAttributes);
//            var s3Config = ProjectConfigurationSection.GetConfig().FileStorage?.S3;

//            TagBuilder builder = new TagBuilder("img");

//            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);
//            var path = S3Path(filename, cmd, s3Config);
//            var url = urlHelper.Content(path);

//            builder.Attributes.Add("src", url);
//            if (ensureSize)
//            {
//                if (cmd.Width.HasValue) builder.Attributes.Add("width", cmd.Width.Value.ToString());
//                if (cmd.Height.HasValue) builder.Attributes.Add("height", cmd.Height.Value.ToString());
//            }

//            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);
//            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
//            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
//        }

//        /// <summary>
//        /// Returns CSS background property with S3 image stored in Amazon S3 storage.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="width">The width of the image.</param>
//        /// <param name="height">The height of the image.</param>
//        /// <param name="norepeat">True to set "no-repeat" value in CSS property, otherwise not.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>An HTML-encoded string for CSS background property.</returns>
//        public static MvcHtmlString BackgroundImageS3(this HtmlHelper html, string filename, int? width, int? height, bool norepeat = true, string alternativeImage = null)
//        {
//            return BackgroundImageS3(html, filename, new ImageResizerCommand { Width = width, Height = height }, norepeat, alternativeImage);
//        }

//        /// <summary>
//        /// Returns CSS background property with S3 image stored in Amazon S3 storage.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="norepeat">True to set "no-repeat" value in CSS property, otherwise not.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>An HTML-encoded string for CSS background property.</returns>
//        public static MvcHtmlString BackgroundImageS3(this HtmlHelper html, string filename, ImageResizerCommand cmd, bool norepeat = true, string alternativeImage = null)
//        {
//            var s3Config = ProjectConfigurationSection.GetConfig().FileStorage?.S3;
//            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);
//            var path = String.IsNullOrEmpty(filename) ? html.ImgUrl(S3Query(alternativeImage, cmd)) : S3Path(filename, cmd, s3Config);
//            var url = urlHelper.Content(path);
//            return MvcHtmlString.Empty.Format("background: url('{0}'){1}; height: {2}px; width: {3}px;", url, norepeat ? " no-repeat" : "", cmd.Height, cmd.Width);
//        }

//        /// <summary>
//        /// Returns a fully qualified URL without query parameters for the file stored in Amazon S3 storage.
//        /// </summary>
//        /// <param name="fileName">The name of the file.</param>
//        /// <param name="s3Config">The Amazon S3 configuration.</param>
//        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
//        /// <returns>A fully qualified URL for the file stored in Amazon S3 storage.</returns>
//        public static string S3PathWithoutQuery(string fileName, S3Element s3Config = null, string protocol = "")
//        {
//            if (s3Config == null) s3Config = ProjectConfigurationSection.GetConfig().FileStorage?.S3;

//            var url = ResolveProtocol(String.Format("~/S3/{0}/{1}", s3Config.Bucket, fileName), protocol);
//            return url;
//        }

//        /// <summary>
//        /// Returns HTML string for the cloud image element (Image stored in the Cloud - s3 or azure).
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="altText">The alternate text of the image.</param>
//        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <param name="ensureSize">Specifies whether width and height attributes should be generated in the 'img' tag. Defaults to true (compatibility).</param>
//        /// <returns>An HTML-encoded string for image element.</returns>
//        public static MvcHtmlString ImgStorage(this HtmlHelper html, string filename, ImageResizerCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "", bool ensureSize = true)
//        {
//            var fileStorage = ProjectConfigurationSection.GetConfig().FileStorage;
//            if (fileStorage.DefaultProvider == 0)
//                throw new ArgumentNullException("FileStorage element is not setup");

//            return fileStorage.DefaultProvider == FileStorageProvider.S3 ?
//                        ImgS3(html, filename, cmd, altText, htmlAttributes, alternativeImage, ensureSize)
//                        : ImgAzure(html, filename, cmd, altText, htmlAttributes, alternativeImage, ensureSize);
//        }

//        /// <summary>
//        ///     Returns the url the Cloud image (s3 or azure file).
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>The image url</returns>
//        public static IHtmlString ImgStorageUrl(this HtmlHelper html, string filename, ImageResizerCommand cmd, string protocol = "", string alternativeImage = "")
//        {
//            var fileStorage = ProjectConfigurationSection.GetConfig().FileStorage;
//            if (fileStorage.DefaultProvider == 0)
//                throw new ArgumentNullException("FileStorage element is not setup");

//            return fileStorage.DefaultProvider == FileStorageProvider.S3 ?
//                        ImgS3Url(html, filename, cmd, protocol, alternativeImage)
//                        : ImgAzureUrl(html, filename, cmd, protocol, alternativeImage);
//        }

//        /// <summary>
//        /// Returns a fully qualified URL with image resize query parameters for the image file stored in the cloud (s3 or azure).
//        /// </summary>
//        /// <param name="filename">The name of the image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand.</param>
//        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>A fully qualified URL with image resize query parameters for the image file stored in Amazon S3 storage.</returns>
//        public static string StoragePath(string filename, ImageResizerCommand cmd = null, string protocol = "", string alternativeImage = null)
//        {
//            var fileStorage = ProjectConfigurationSection.GetConfig().FileStorage;
//            if (fileStorage.DefaultProvider == 0)
//                throw new ArgumentNullException("FileStorage element is not setup");

//            return fileStorage.DefaultProvider == FileStorageProvider.S3 ?
//                        S3Path(filename, cmd, protocol: protocol, alternativeImage: alternativeImage)
//                        : AzurePath(filename, cmd, protocol: protocol, alternativeImage: alternativeImage);
//        }

//        static Lazy<NameValueCollection> _azureResizerConfig = new Lazy<NameValueCollection>(() =>
//        {
//            try
//            {
//                var section = (ImageResizer.ResizerSection)ConfigurationManager.GetSection("resizer");

//                var azurePlugin = section.getCopyOfNode("plugins").Children.Where(n => n.Name == "add" && n.Attrs["name"] == "BlueChilli.ImageResizer.Plugins.AzureReader").FirstOrDefault();
//                return azurePlugin.Attrs ?? new NameValueCollection();
//            }
//            catch
//            {
//                return new NameValueCollection();
//            }
//        });

//        static Lazy<AzureStorageElement> _azureConfig = new Lazy<AzureStorageElement>(() =>
//        {
//            try
//            {
//                var section = ProjectConfigurationSection.GetConfig();
//                return section.FileStorage.Azure;
//            }
//            catch
//            {
//                return new AzureStorageElement();
//            }
//        });

//        /// <summary>
//        /// Returns HTML string for the Azure image element (Image stored in Azure storage).
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="altText">The alternate text of the image.</param>
//        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <param name="ensureSize">Specifies whether width and height attributes should be generated in the 'img' tag. Defaults to true (compatibility).</param>
//        /// <returns>An HTML-encoded string for the Azure image element.</returns>
//        public static MvcHtmlString ImgAzure(this HtmlHelper html, string filename, ImageResizerCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "", bool ensureSize = true)
//        {
//            if (cmd == null) cmd = new ImageResizerCommand();
//            if (String.IsNullOrEmpty(filename)) return html.Img(S3Query(alternativeImage, cmd), cmd.Width, cmd.Height, altText, htmlAttributes);

//            TagBuilder builder = new TagBuilder("img");

//            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);
//            var path = AzurePath(filename, cmd, null, "", alternativeImage);
//            var url = urlHelper.Content(path);

//            builder.Attributes.Add("src", url);
//            if (ensureSize)
//            {
//                if (cmd.Width.HasValue) builder.Attributes.Add("width", cmd.Width.Value.ToString());
//                if (cmd.Height.HasValue) builder.Attributes.Add("height", cmd.Height.Value.ToString());
//            }

//            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);
//            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
//            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
//        }

//        /// <summary>
//        ///     Returns the url the Azure image (Image stored in Azure storage).
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="filename">The name of image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand for the width and height of the image.</param>
//        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>The Azure image url</returns>
//        public static IHtmlString ImgAzureUrl(this HtmlHelper html, string filename, ImageResizerCommand cmd, string protocol = "", string alternativeImage = "")
//        {
//            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);
//            return html.Raw(html.ImgUrl(AzurePath(filename, cmd, null, protocol, alternativeImage)));
//        }

//        /// <summary>
//        /// Returns a fully qualified URL with image resize query parameters for the image file stored in Azure.
//        /// </summary>
//        /// <param name="filename">The name of the image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand.</param>
//        /// <param name="config">The Azure configuration.</param>
//        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>A fully qualified URL with image resize query parameters for the image file stored in Amazon S3 storage.</returns>
//        public static string AzurePath(string filename, ImageResizerCommand cmd = null, NameValueCollection config = null, string protocol = "", string alternativeImage = null)
//        {
//            if (String.IsNullOrEmpty(filename) && String.IsNullOrEmpty(alternativeImage))
//            {
//                return "";
//            }
//            var url = String.IsNullOrEmpty(filename)
//                ? ResolveProtocol(alternativeImage, protocol)
//                : AzurePathWithoutQuery(filename, config, protocol);

//            if (cmd == null) cmd = new ImageResizerCommand();

//            return S3Query(url, cmd);
//        }

//        private static string AzurePathWithoutQuery(string fileName, NameValueCollection config = null, string protocol = "")
//        {
//            var prefix = (config != null) ? config["prefix"] : _azureResizerConfig.Value["prefix"];
//            if (String.IsNullOrEmpty(prefix))
//            {
//                throw new ApplicationException("AzureReader prefix not found");
//            }

//            prefix = prefix.TrimEnd("/");

//            var container = (config != null) ? config["container"] : _azureConfig.Value?.Container;
//            var path = String.IsNullOrEmpty(container) ? $"{prefix}/{fileName}" : $"{prefix}/{container}/{fileName}";

//            var url = ResolveProtocol(path, protocol);
//            return url;
//        }

//        /// <summary>
//        /// Returns a fully qualified URL with image resize query parameters for the image file stored in Amazon S3 storage.
//        /// </summary>
//        /// <param name="filename">The name of the image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand.</param>
//        /// <param name="s3Config">The Amazon S3 configuration.</param>
//        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
//        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
//        /// <returns>A fully qualified URL with image resize query parameters for the image file stored in Amazon S3 storage.</returns>
//        public static string S3Path(string filename, ImageResizerCommand cmd = null, S3Element s3Config = null, string protocol = "", string alternativeImage = null)
//        {
//            if (String.IsNullOrEmpty(filename) && String.IsNullOrEmpty(alternativeImage))
//            {
//                return "";
//            }
//            if (s3Config == null) s3Config = ProjectConfigurationSection.GetConfig().FileStorage?.S3;
//            var url = String.IsNullOrEmpty(filename)
//                ? ResolveProtocol(alternativeImage, protocol)
//                : S3PathWithoutQuery(filename, s3Config, protocol);

//            if (cmd == null) cmd = new ImageResizerCommand();

//            return S3Query(url, cmd);
//        }

//        /// <summary>
//        /// Appends image resize query parameters to the image file name.
//        /// </summary>
//        /// <param name="filename">The name of the image file.</param>
//        /// <param name="cmd">The BlueChilli.Web.ImageResizerCommand.</param>
//        /// <returns>An image file name with image resize query parameters appended.</returns>
//        public static string S3Query(string filename, ImageResizerCommand cmd)
//        {
//            var query = HttpUtility.ParseQueryString(string.Empty);
//            if (cmd.Height.HasValue) query.Add("h", cmd.RetinaHeight().Value.ToString());
//            if (cmd.Width.HasValue) query.Add("w", cmd.RetinaWidth().Value.ToString());
//            if (cmd.Mode != ImageResizerMode.Pad) query.Add("mode", cmd.Mode.ToString().ToLower());
//            if (cmd.Anchor != ImageResizerAnchor.None) query.Add("anchor", cmd.Anchor.ToString().ToLower());
//            if (cmd.Scale != ImageResizerScale.None) query.Add("scale", cmd.Scale.ToString().ToLower());
//            if (cmd.Format != ImageResizerFormat.Original) query.Add("format", cmd.Format.ToString().ToLower());
//            if (cmd.Quality.HasValue && cmd.Quality.Value != 90 && (cmd.Format == ImageResizerFormat.JPG || Path.GetExtension(filename).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(filename).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
//                query.Add("quality", cmd.Quality.Value.ToString());
//            if (cmd.Rotate.HasValue && cmd.Rotate.Value != 0) query.Add("rotate", cmd.Rotate.Value.ToString());
//            if (cmd.AutoRotate) query.Add("autorotate", "true");
//            if (cmd.Blur != 0) query.Add("blur", cmd.Blur.ToString());
//            if (!String.IsNullOrEmpty(cmd.BgColor)) query.Add("bgcolor", cmd.BgColor);

//            return String.Format("{0}?{1}", filename, query.ToString());
//        }
//        #endregion

//        /// <summary>
//        /// Returns HTML string for the static Google map image element.
//        /// </summary>
//        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="googleAddress">The Google address for Google map.</param>
//        /// <param name="width">The width of the image.</param>
//        /// <param name="height">The height of the image.</param>
//        /// <param name="zoom">The zoom for the Google map.</param>
//        /// <param name="markers">A list of markers on the Google map.</param>
//        /// <param name="additionalParams">Additional query string parameters for the Google map.</param>
//        /// <returns>An HTML string for the static Google map image element.</returns>
//        public static MvcHtmlString ImageGoogleStaticMap(this HtmlHelper htmlHelper, GoogleAddress googleAddress, int width, int height, int zoom = 15, List<GoogleMapMarker> markers = null, string[] additionalParams = null)
//        {
//            return MvcHtmlString.Create(GoogleMap.StaticMapFromGoogleAddress(googleAddress, width, height, zoom, markers, additionalParams));
//        }
//    }

//    #region Image Resizer
//    /// <summary>
//    /// Represents the commands used by image resize.
//    /// </summary>
//    /// <remarks>http://imageresizing.net/docs/reference</remarks>
//    public class ImageResizerCommand
//    {
//        /// <summary>
//        /// Initialize a new instance of BlueChilli.Web.ImageResizerCommand with "AutoRotate" property set to True.
//        /// </summary>
//        public ImageResizerCommand()
//        {
//            AutoRotate = true;
//        }

//        /// <summary>
//        /// Gets or set the image width.
//        /// </summary>
//        public int? Width { get; set; }
//        /// <summary>
//        /// Gets or sets the image height.
//        /// </summary>
//        public int? Height { get; set; }
//        /// <summary>
//        /// Gets or sets the image resize mode.
//        /// </summary>
//        public ImageResizerMode Mode { get; set; }
//        /// <summary>
//        /// Gets or sets how to anchor the image for padding or cropping mode.
//        /// </summary>
//        public ImageResizerAnchor Anchor { get; set; }
//        /// <summary>
//        /// Gets or sets the scale options when image resizing.
//        /// </summary>
//        public ImageResizerScale Scale { get; set; }
//        /// <summary>
//        /// Gets or sets the format of the image.
//        /// </summary>
//        public ImageResizerFormat Format { get; set; }
//        /// <summary>
//        /// Gets or sets the scale options for retina screen. 
//        /// </summary>
//        public ImageRetinaScale RetinaScale { get; set; }
//        /// <summary>
//        /// Gets or sets the image quality, only if format is JPG or has been forced to JPG, default is 90.
//        /// </summary>
//        public int? Quality { get; set; }
//        /// <summary>
//        /// Gets or sets degrees to rotate the image.
//        /// </summary>
//        public double? Rotate { get; set; }
//        /// <summary>
//        /// Gets or sets the "AutoRotate" property which automatically rotates the image based on the EXIF info from the camera (Requires the AutoRotate plugin).
//        /// </summary>
//        /// <remarks>http://imageresizing.net/plugins/autorotate</remarks>
//        public bool AutoRotate { get; set; }

//        /// <summary>
//        /// Gets or sets the radius for Gaussian blur.
//        /// </summary>
//        public int Blur { get; set; }

//        /// <summary>
//        /// Gets or sets the background color to use when resizing (e.g 000000 or black)
//        /// </summary>
//        public string BgColor { get; set; }

//        /// <summary>
//        /// Gets the image width for retina screen.
//        /// </summary>
//        /// <returns>The width of the image.</returns>
//        public int? RetinaWidth()
//        {
//            if (Width.HasValue)
//            {
//                switch (RetinaScale)
//                {
//                    case ImageRetinaScale.Double: return Width.Value * 2;
//                    default: return Width.Value;
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// Gets the image height for the retina screen.
//        /// </summary>
//        /// <returns>The height of the image.</returns>
//        public int? RetinaHeight()
//        {
//            if (Height.HasValue)
//            {
//                switch (RetinaScale)
//                {
//                    case ImageRetinaScale.Double: return Height.Value * 2;
//                    default: return Height.Value;
//                }
//            }
//            return null;
//        }
//    }

//    /// <summary>
//    /// The enumeration values for BlueChilli.Web.ImageResizerMode. How to handle aspect-ratio conflicts between the image and width+height.
//    /// </summary>
//    public enum ImageResizerMode
//    {
//        /// <summary>
//        /// Adds whitespace.
//        /// </summary>
//        Pad,
//        /// <summary>
//        /// Behaves like max width/max height.
//        /// </summary>
//        Max,
//        /// <summary>
//        /// Crops minimally.
//        /// </summary>
//        Crop,
//        /// <summary>
//        /// Loses aspect-ratio, stretching the image
//        /// </summary>
//        Stretch,
//        /// <summary>
//        /// Uses seam carving, requires SeamCarving plugin.
//        /// </summary>
//        /// <remarks>http://imageresizing.net/plugins/seamcarving</remarks>
//        Carve
//    }

//    /// <summary>
//    /// Enumeration values for BlueChilli.Web.ImageResizerAnchor. How to anchor the image when padding or cropping.
//    /// </summary>
//    public enum ImageResizerAnchor
//    {
//        /// <summary>
//        /// Do not specify options for BlueChilli.Web.ImageResizerAnchor.
//        /// </summary>
//        None,
//        /// <summary>
//        /// Image at top left.
//        /// </summary>
//        TopLeft,
//        /// <summary>
//        /// Image at top center.
//        /// </summary>
//        TopCenter,
//        /// <summary>
//        /// Image at top right.
//        /// </summary>
//        TopRight,
//        /// <summary>
//        /// Image at middle left.
//        /// </summary>
//        MiddleLeft,
//        /// <summary>
//        /// Image at middle center.
//        /// </summary>
//        MiddleCenter,
//        /// <summary>
//        /// Image at middle right.
//        /// </summary>
//        MiddleRight,
//        /// <summary>
//        /// Image at bottom left.
//        /// </summary>
//        BottomLeft,
//        /// <summary>
//        /// Image at bottom center.
//        /// </summary>
//        BottomCenter,
//        /// <summary>
//        /// Image ate bottom right.
//        /// </summary>
//        BottomRight
//    }

//    /// <summary>
//    /// Enumeration values for BlueChilli.Web.ImageResizerScale. By default, images are not enlarged - the image stays its original size if you request a larger size.
//    /// </summary>
//    public enum ImageResizerScale
//    {
//        /// <summary>
//        /// Do not specify options for BlueChilli.Web.ImageResizerScale.
//        /// </summary>
//        None,
//        /// <summary>
//        /// Reduce the size of the image.
//        /// </summary>
//        Down,
//        /// <summary>
//        /// Allow both reduction and enlargement.
//        /// </summary>
//        Both,
//        /// <summary>
//        /// Expands image to fill the desired area.
//        /// </summary>
//        Canvas
//    }

//    /// <summary>
//    /// Enumeration values for BlueChilli.Web.ImageResizerFormat. The output format to use.
//    /// </summary>
//    public enum ImageResizerFormat
//    {
//        /// <summary>
//        /// Do not specify options for BlueChilli.Web.ImageResizerFormat, keep the original format.
//        /// </summary>
//        Original,
//        /// <summary>
//        /// Outputs image in JPG format.
//        /// </summary>
//        JPG,
//        /// <summary>
//        /// Outputs image in GIF format.
//        /// </summary>
//        GIF,
//        /// <summary>
//        /// Outputs image in PNG format.
//        /// </summary>
//        PNG,
//    }

//    /// <summary>
//    /// Enumeration values for BlueChilli.Web.ImageRetinaScale. Returns larger image and use CSS to constrain image to actual size. Retina screens will make use of the extra pixels.
//    /// </summary>
//    public enum ImageRetinaScale
//    {
//        /// <summary>
//        /// Do not specify options for BlueChilli.Web.ImageRetinaScale
//        /// </summary>
//        None,
//        /// <summary>
//        /// Doubles the image height and width.
//        /// </summary>
//        Double
//    }
//    #endregion
//}