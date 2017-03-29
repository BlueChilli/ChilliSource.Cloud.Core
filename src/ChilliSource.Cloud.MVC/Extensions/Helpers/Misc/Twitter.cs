//
//
//using System;
//using System.Linq;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Text;
//using System.Web.Mvc;
//using ChilliSource.Cloud.Web;

////Named so to not pollute @Html
//namespace ChilliSource.Cloud.Web.MVC.Misc
//{
//    /// <summary>
//    /// Represents support for rendering Twitter HTML and script element and Twitter cards
//    /// </summary>
//    public static class TwitterHtmlHelper
//    {
//        /// <summary>
//        /// Emits following meta tags for a Twitter product card.
//        /// https://dev.twitter.com/cards/types/product
//        /// https://cards-dev.twitter.com/validator
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="title">Product title.</param>
//        /// <param name="description">Product description.</param>
//        /// <param name="twitterId">The Twitter @username the card should be attributed to</param>
//        /// <param name="S3Image">Image to be loaded from S3</param>
//        /// <param name="data">Up to two label and data pairs can be added to the card</param>
//        /// <param name="alternativeImage">Image to be used if no S3Image passed in</param>
//        /// <returns>Twitter product meta tags to be directed rendered into Html output stream</returns>
//        public static MvcHtmlString TwitterProductCard(this HtmlHelper html, string title, string description, string twitterId, string S3Image, Dictionary<string, string> data = null, string alternativeImage = null)
//        {
//            string format = @"<meta name=""twitter:{0}"" content=""{1}"" />";
//            var config = ProjectConfigurationSection.GetConfig();
//            var sb = new StringBuilder();
//            sb.AppendFormat(format, "card", "product");
//            sb.AppendFormat(format, "site", twitterId);
//            //sb.AppendFormat(format, "creator", twitterId);
//            sb.AppendFormat(format, "title", html.Encode(title));
//            sb.AppendFormat(format, "description", html.Encode(description));
//            sb.AppendFormat(format, "image", html.ImgS3Url(S3Image, new ImageResizerCommand { Height = 160, Width = 160, Format = ImageResizerFormat.JPG, Quality = 90 }, protocol: "http", alternativeImage: alternativeImage));
//            if (data != null)
//            {
//                for (var i = 0; i < data.Count; i++ )
//                {
//                    var key = data.Keys.ToList()[i];
//                    sb.AppendFormat(format, "label{0}".FormatWith(i + 1), html.Encode(key));
//                    sb.AppendFormat(format, "data{0}".FormatWith(i + 1), html.Encode(data[key]));
//                };
//            }
//            return MvcHtmlString.Create(sb.ToString());
//        }

//        /// <summary>
//        /// Loads twitter controls on ajaxed content.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <returns>An HTML string for the Twitter JavaScript scripts.</returns>
//        public static MvcHtmlString TwitterAjax(this HtmlHelper html)
//        {
//            return MvcHtmlString.Create("twttr.widgets.load();");
//        }

//        /// <summary>
//        /// Emits Twitter SDK script which is needed for twitter controls like share link, and place in end of script tags.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="show">Optionally return empty result. For example @Html.TwitterSdk(ViewBag.LoadTwitter as bool?)</param>
//        /// <returns>An HTML string for the Twitter SDK scripts.</returns>
//        public static MvcHtmlString TwitterSdk(this HtmlHelper html, bool? show = true)
//        {
//            string sdk = @"<script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0];if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src='//platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document,'script','twitter-wjs');</script>";
//            var result = MvcHtmlString.Empty;
//            if (show.GetValueOrDefault(false)) result = MvcHtmlString.Create(sdk);
//            return result;
//        }

//        /// <summary>
//        /// Returns HTML element for Twitter share like button.
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="text">The link text.</param>
//        /// <param name="url">The link URL.</param>
//        /// <param name="via">The via property for the share link button.</param>
//        /// <param name="showCount">True to display count of the link clicked, otherwise not.</param>
//        /// <returns>An HTML element for Twitter share like button.</returns>
//        /// <remarks>https://twitter.com/about/resources/buttons</remarks>
//        public static MvcHtmlString TwitterShareLink(this HtmlHelper html, string text, string url = "", string via = "", bool showCount = true)
//        {
//            string format = @"<div class=""twitter-control""><a href=""https://twitter.com/share"" class=""twitter-share-button"" data-url=""{1}"" data-text=""{0}"" data-via=""{2}""{3}>Tweet</a></div>";
//            string countString = showCount ? "" : @"data-count=""none""";
//            return MvcHtmlString.Empty.Format(format, html.Encode(text), ResolveUrl(html, url), via, countString);
//        }

//        private static string ResolveUrl(HtmlHelper html, string url)
//        {
//            if (String.IsNullOrEmpty(url)) return html.ViewContext.RequestContext.HttpContext.Request.Url.AbsoluteUri;
//            return UriExtensions.Parse(url).AbsoluteUri;
//        }


//        #region one base method with 3 overload twittercard

//        //this is the base method
//        public static MvcHtmlString TwitterCard(this HtmlHelper html, string cardType, string title, string description, string image = null, object listAttributes = null)
//        {

//            var format = @"<meta name=""twitter:{0}"" content=""{1}"" />";
//            var sb = new StringBuilder();
//            var config = ProjectConfigurationSection.GetConfig();
//            sb.AppendFormat(format, "card", html.Encode(cardType));
//            sb.AppendFormat(format, "site", config.ProjectEnvironment == ProjectEnvironment.Production ? config.ProjectDisplayName : String.Format("{0} ({1})", config.ProjectDisplayName, config.ProjectEnvironment.GetDescription()));
//            sb.AppendFormat(format, "title", html.Encode(title));
//            sb.AppendFormat(format, "description", html.Encode(description));
//            if (cardType == "summary" || cardType == "summary_large_image")
//            {
//                sb.AppendFormat(format, "image", UriExtensions.Parse(image).AbsoluteUri);
//            }

//            if (listAttributes != null)
//            {

//                sb.Append(GetHtmlAttributes(listAttributes));
//            }

//            return MvcHtmlString.Create(sb.ToString());
//        }

//        /// <summary>
//        /// Emits following meta tags for a Twitter Summary card.
//        /// https://dev.twitter.com/cards/types/summary
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="title">Title should be concise and will be truncated at 70 characters.</param>
//        /// <param name="description">Description text will be truncated at the word to 200 characters.</param>
//        /// <param name="image">Image path</param>
//        /// <returns>Twitter summary meta tags to be directed rendered into Html output stream</returns>
//        public static MvcHtmlString TwitterSummaryCard(this HtmlHelper html, string title, string description, string image = null)
//        {
//            return TwitterCard(html, "summary", title, description, image);
//        }


//        /// <summary>
//        /// Emits following meta tags for a Twitter Summary card with large image.
//        /// https://dev.twitter.com/cards/types/summary
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="title">Title should be concise and will be truncated at 70 characters.</param>
//        /// <param name="description">Description text will be truncated at the word to 200 characters.</param>
//        /// <param name="image">Image path</param>
//        /// <returns>Twitter summary with large image meta tags to be directed rendered into Html output stream</returns>
//        public static MvcHtmlString TwitterSummaryCardWithLargeImage(this HtmlHelper html, string title, string description, string image = null)
//        {

//            return TwitterCard(html, "summary_large_image", title, description, image);
//        }


//        /// <summary>
//        /// Emits following meta tags for a Twitter App card.
//        /// https://dev.twitter.com/cards/types/summary
//        /// </summary>
//        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
//        /// <param name="title">Title should be concise and will be truncated at 70 characters.</param>
//        /// <param name="description">Description text will be truncated at the word to 200 characters.</param>
//        /// <param name="listAttributes">this is pass it as a sample below</param>
//        /// <returns>Twitter summary with large image meta tags to be directed rendered into Html output stream</returns>
//        ///listAttributes sample:
//        ///    new{
//        ///         iphoneName ="iphoneName",
//        ///         iphoneId="iphoneId",
//        ///         iphoneUrl="iphoneUrl",
//        ///         ipadName = "ipadName",
//        ///             ipadId="ipadId",
//        ///         ipadUrl="ipadUrl",
//        ///         googleplayName="googleplayName",
//        ///         googleplayId="googleplayId",
//        ///         googleplayUrl="googleplayUrl"
//        ///      }
//        public static MvcHtmlString TwitterAppCard(this HtmlHelper html, string title, string description, object listAttribute)
//        {
//            return TwitterCard(html, "app", title, description, null, listAttribute);
//        }

//        private static string GetHtmlAttributes(object listAttributes)
//        {

//            var format = @"<meta name=""twitter:app:{0}:{1}"" content=""{2}"" />";
//            var sb = new StringBuilder();
//            sb.Append(@"<meta name=""twitter:app:country"" content=""AU"" />");

//            if (listAttributes != null)
//            {
//                var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(listAttributes);
//                foreach (var item in attributes)
//                {

//                    if (item.Key == "iphoneName") sb.AppendFormat(format, "name", "iphone", item.Value);
//                    if (item.Key == "iphoneId") sb.AppendFormat(format, "id", "iphone", item.Value);
//                    if (item.Key == "iphoneUrl") sb.AppendFormat(format, "url", "iphone", item.Value);

//                    if (item.Key == "ipadName") sb.AppendFormat(format, "name", "ipad", item.Value);
//                    if (item.Key == "ipadId") sb.AppendFormat(format, "id", "ipad", item.Value);
//                    if (item.Key == "ipadUrl") sb.AppendFormat(format, "url", "ipad", item.Value);

//                    if (item.Key == "googleplayName") sb.AppendFormat(format, "name", "googleplay", item.Value);
//                    if (item.Key == "googleplayId") sb.AppendFormat(format, "id", "googleplay", item.Value);
//                    if (item.Key == "googleplayUrl") sb.AppendFormat(format, "url", "googleplay", item.Value);

//                }

//            }

//            return sb.ToString();
//        }


//        #endregion


//    }
//}