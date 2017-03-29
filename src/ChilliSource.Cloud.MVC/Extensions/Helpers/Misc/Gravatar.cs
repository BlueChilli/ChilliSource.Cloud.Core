using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

//todo this googled code could do with a refactor 
//Named so to not pollute @Html
namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Globally Recognised Avatar - http://gravatar.com
    /// </summary>
    /// <remarks>
    /// This implementation by Andrew Freemantle - http://www.fatlemon.co.uk/
    /// <para>Source, Wiki and Issues: https://github.com/AndrewFreemantle/Gravatar-HtmlHelper </para>
    /// </remarks>
    public static class GravatarHtmlHelper
    {
        /// <summary>
        /// In addition to allowing you to use your own image, Gravatar has a number of built in options which you can also use as defaults. Most of these work by taking the requested email hash and using it to generate a themed image that is unique to that email address
        /// </summary>
        public enum DefaultImage
        {
            /// <summary>Default Gravatar logo</summary>
            [DescriptionAttribute("")]
            Default,
            /// <summary>404 - do not load any image if none is associated with the email hash, instead return an HTTP 404 (File Not Found) response</summary>
            [DescriptionAttribute("404")]
            Http404,
            /// <summary>Mystery-Man - a simple, cartoon-style silhouetted outline of a person (does not vary by email hash)</summary>
            [DescriptionAttribute("mm")]
            MysteryMan,
            /// <summary>Identicon - a geometric pattern based on an email hash</summary>
            [DescriptionAttribute("identicon")]
            Identicon,
            /// <summary>MonsterId - a generated 'monster' with different colors, faces, etc</summary>
            [DescriptionAttribute("monsterid")]
            MonsterId,
            /// <summary>Wavatar - generated faces with differing features and backgrounds</summary>
            [DescriptionAttribute("wavatar")]
            Wavatar,
            /// <summary>Retro - awesome generated, 8-bit arcade-style pixelated faces</summary>
            [DescriptionAttribute("retro")]
            Retro
        }
        
        /// <summary>
        /// Gravatar allows users to self-rate their images so that they can indicate if an image is appropriate for a certain audience. By default, only 'G' rated images are displayed unless you indicate that you would like to see higher ratings
        /// </summary>
        public enum Rating
        {
            /// <summary>Suitable for display on all websites with any audience type</summary>
            [DescriptionAttribute("g")]
            G,
            /// <summary>May contain rude gestures, provocatively dressed individuals, the lesser swear words, or mild violence</summary>
            [DescriptionAttribute("pg")]
            PG,
            /// <summary>May contain such things as harsh profanity, intense violence, nudity, or hard drug use</summary>
            [DescriptionAttribute("r")]
            R,
            /// <summary>May contain hardcore sexual imagery or extremely disturbing violence</summary>
            [DescriptionAttribute("x")]
            X
        }
        
        /// <summary>
        /// Returns a Globally Recognised Avatar as an 80 pixel &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress)
        {
            return GravatarImage(htmlHelper, emailAddress, 80, DefaultImage.Default, string.Empty, false, Rating.G, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size)
        {
            return GravatarImage(htmlHelper, emailAddress, size, DefaultImage.Default, string.Empty, false, Rating.G, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImage">Default image if user hasn't created a Gravatar</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, DefaultImage defaultImage)
        {
            return GravatarImage(htmlHelper, emailAddress, size, defaultImage, string.Empty, false, Rating.G, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImageUrl">URL to a custom default image (e.g: 'Url.Content("~/images/no-grvatar.png")' )</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, string defaultImageUrl)
        {
            return GravatarImage(htmlHelper, emailAddress, size, DefaultImage.Default, defaultImageUrl, false, Rating.G, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImage">Default image if user hasn't created a Gravatar</param>
        /// <param name="forceDefaultImage">Prefer the default image over the users own Gravatar</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, DefaultImage defaultImage, bool forceDefaultImage)
        {
            return GravatarImage(htmlHelper, emailAddress, size, defaultImage, string.Empty, forceDefaultImage, Rating.G, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImageUrl">URL to a custom default image (e.g: 'Url.Content("~/images/no-grvatar.png")' )</param>
        /// <param name="forceDefaultImage">Prefer the default image over the users own Gravatar</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, string defaultImageUrl, bool forceDefaultImage)
        {
            return GravatarImage(htmlHelper, emailAddress, size, DefaultImage.Default, defaultImageUrl, forceDefaultImage, Rating.G, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImage">Default image if user hasn't created a Gravatar</param>
        /// <param name="rating">Gravatar content rating (note that Gravatars are self-rated)</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, DefaultImage defaultImage, Rating rating)
        {
            return GravatarImage(htmlHelper, emailAddress, size, defaultImage, string.Empty, false, rating, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImageUrl">URL to a custom default image (e.g: 'Url.Content("~/images/no-grvatar.png")' )</param>
        /// <param name="rating">Gravatar content rating (note that Gravatars are self-rated)</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, string defaultImageUrl, Rating rating)
        {
            return GravatarImage(htmlHelper, emailAddress, size, DefaultImage.Default, defaultImageUrl, false, rating, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImage">Default image if user hasn't created a Gravatar</param>
        /// <param name="forceDefaultImage">Prefer the default image over the users own Gravatar</param>
        /// <param name="rating">Gravatar content rating (note that Gravatars are self-rated)</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, DefaultImage defaultImage, bool forceDefaultImage, Rating rating)
        {
            return GravatarImage(htmlHelper, emailAddress, size, defaultImage, string.Empty, forceDefaultImage, rating, false);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImageUrl">URL to a custom default image (e.g: 'Url.Content("~/images/no-grvatar.png")' )</param>
        /// <param name="forceDefaultImage">Prefer the default image over the users own Gravatar</param>
        /// <param name="rating">Gravatar content rating (note that Gravatars are self-rated)</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, string defaultImageUrl, bool forceDefaultImage, Rating rating)
        {
            return GravatarImage(htmlHelper, emailAddress, size, DefaultImage.Default, defaultImageUrl, forceDefaultImage, rating, false);
        }
        
        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImage">Default image if user hasn't created a Gravatar</param>
        /// <param name="forceDefaultImage">Prefer the default image over the users own Gravatar</param>
        /// <param name="rating">Gravatar content rating (note that Gravatars are self-rated)</param>
        /// <param name="forceSecureRequest">Always do secure (https) requests</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, DefaultImage defaultImage, bool forceDefaultImage, Rating rating, bool forceSecureRequest)
        {
            return GravatarImage(htmlHelper, emailAddress, size, defaultImage, string.Empty, forceDefaultImage, rating, forceSecureRequest);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="emailAddress">Email Address for the Gravatar</param>
        /// <param name="size">Size in pixels (default: 80)</param>
        /// <param name="defaultImageUrl">URL to a custom default image (e.g: 'Url.Content("~/images/no-grvatar.png")' )</param>
        /// <param name="forceDefaultImage">Prefer the default image over the users own Gravatar</param>
        /// <param name="rating">Gravatar content rating (note that Gravatars are self-rated)</param>
        /// <param name="forceSecureRequest">Always do secure (https) requests</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, string defaultImageUrl, bool forceDefaultImage, Rating rating, bool forceSecureRequest)
        {
            return GravatarImage(htmlHelper, emailAddress, size, DefaultImage.Default, defaultImageUrl, forceDefaultImage, rating, forceSecureRequest);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="options">Image options defined by BlueChilli.Web.Misc.GravatarOptions.</param>
        /// <returns>An HTML string for the Avatar image element.</returns>
        public static HtmlString GravatarImage(this HtmlHelper htmlHelper, GravatarOptions options)
        {
            return GravatarImage(htmlHelper, options.EmailAddress, options.Size, options.DefaultImage, options.DefaultImageUrl, options.ForceDefaultImage, options.Rating, options.ForceSecureRequest, options.HtmlAttributes);
        }

        /// <summary>
        /// Returns a Globally Recognised Avatar as an &lt;img /&gt; - http://gravatar.com
        /// </summary>
        private static HtmlString GravatarImage(this HtmlHelper htmlHelper, string emailAddress, int size, DefaultImage defaultImage, string defaultImageUrl, bool forceDefaultImage, Rating rating, bool forceSecureRequest, object htmlAttributes = null)
        {
            var imgTag = new TagBuilder("img");

            imgTag.Attributes.Add("src",
                string.Format("{0}://{1}.gravatar.com/avatar/{2}?s={3}{4}{5}{6}",
                    htmlHelper.ViewContext.HttpContext.Request.IsSecureConnection || forceSecureRequest ? "https" : "http",
                    htmlHelper.ViewContext.HttpContext.Request.IsSecureConnection || forceSecureRequest ? "secure" : "www",
                    EncryptionHelper.GetMd5Hash(emailAddress.Trim().ToLower()),
                    size.ToString(),
                    "&d=" + (!string.IsNullOrEmpty(defaultImageUrl) ? HttpUtility.UrlEncode(defaultImageUrl) : defaultImage.GetDescription()),
                    forceDefaultImage ? "&f=y" : "",
                    "&r=" + rating.GetDescription()
                    )
                );

            imgTag.Attributes.Add("alt", "Gravatar image");
            imgTag.MergeAttributes(RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes), true);
            imgTag.AddCssClass("gravatar");

            return new HtmlString(imgTag.ToString());
        }

        /// <summary>
        /// Returns the value of a DescriptionAttribute for a given Enum value
        /// </summary>
        /// <remarks>Source: http://blogs.msdn.com/b/abhinaba/archive/2005/10/21/483337.aspx </remarks>
        /// <param name="en"></param>
        /// <returns></returns>
        private static string GetDescription(this Enum en)
        {

            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return en.ToString();
        }
    }

    /// <summary>
    /// Represents image options used by generate Avatar image. 
    /// </summary>
    public class GravatarOptions
    {
        /// <summary>
        /// Gets or sets email address.
        /// </summary>
        public string EmailAddress { get; set; }
        /// <summary>
        /// Gets or set image size.
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Gets or sets default image.
        /// </summary>
        public GravatarHtmlHelper.DefaultImage DefaultImage { get; set; }
        /// <summary>
        /// Gets or sets default image URL.
        /// </summary>
        public string DefaultImageUrl { get; set; }
        /// <summary>
        /// Gets or sets whether to use default image or not.
        /// </summary>
        public bool ForceDefaultImage { get; set; }
        /// <summary>
        /// Gets or sets image rating.
        /// </summary>
        public GravatarHtmlHelper.Rating Rating { get; set; }
        /// <summary>
        /// Gets or sets whether to use "HTTPS" protocol or not.
        /// </summary>
        public bool ForceSecureRequest { get; set; }
        /// <summary>
        /// Gets or sets the object that contains the HTML attributes.
        /// </summary>
        public object HtmlAttributes { get; set; }
    }
}