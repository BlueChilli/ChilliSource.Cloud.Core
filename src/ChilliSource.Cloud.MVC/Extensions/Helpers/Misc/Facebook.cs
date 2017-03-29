using System;
using System.ComponentModel;
using System.Text;
using System.Web.Mvc;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Core;

//Named so to not pollute @Html
namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents support for rendering Facebook HTML and script element.
    /// </summary>
    public static class FacebookHtmlHelper
    {
        /// <summary>
        /// Calls FB.XFBML.parse on every selector marked with '.facebook-control'.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML string for the Facebook Ajax scripts.</returns>
        public static MvcHtmlString FacebookAjax(this HtmlHelper html)
        {
            return MvcHtmlString.Create("$('.facebook-control').each(function () { FB.XFBML.parse(this); });");
        }

        /// <summary>
        /// Emits meta tag containing facebook app id and places in layout page in head section with your other meta tags.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="show">Optionally returns empty result. For example @Html.FacebookMeta(ViewBag.LoadFacebook as bool?) where ViewBag.LoadFaceBook is set by the View</param>
        /// <returns>An HTML string for the Facebook meta tag.</returns>
        public static MvcHtmlString FacebookMeta(this HtmlHelper html, bool? show = true)
        {
            string format = @"<meta property=""fb:app_id"" content=""{0}""/>";
            var result = MvcHtmlString.Empty;
            if (show.GetValueOrDefault(false)) result = result.Format(format, ProjectConfigurationSection.GetConfig().Facebook.FacebookAppId);
            return result;
        }

        /// <summary>
        /// Emits meta tags describing the page for link back to your view. Resolve this using a meta section.
        /// For ajaxed content, you need to forward load this into the main view using instructions embedded in the url passed to facebook.
        /// FacebookMeta must also be called if FacebookLinkback is used.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="title">The title content for Facebook open graph property.</param>
        /// <param name="description">The description content for Facebook open graph property.</param>
        /// <param name="image">The image URL for Facebook open graph property.</param>
        /// <param name="type">The type content for Facebook open graph property.</param>
        /// <param name="url">If passed in null the url meta tag is omitted, if passed in empty defaults to current url.</param>
        /// <returns>An HTML string for the Facebook meta tag with open graph properties.</returns>
        /// <remarks>http://developers.facebook.com/docs/opengraphprotocol/#types</remarks>
        public static MvcHtmlString FacebookLinkback(this HtmlHelper html, string title, string description, string image = null, string type = "article", string url = "")
        {
            var format = @"<meta property=""og:{0}"" content=""{1}""/>";
            var sb = new StringBuilder();
            sb.AppendFormat(format, "title", html.Encode(title));
            sb.AppendFormat(format, "type", type);
            sb.AppendFormat(format, "description", html.Encode(description));
            if (!String.IsNullOrEmpty(image))
            {
                sb.AppendFormat(format, "image", UriExtensions.Parse(image).AbsoluteUri);
            }
            if (url != null) sb.AppendFormat(format, "url", ResolveUrl(html, url));

            var config = ProjectConfigurationSection.GetConfig();
            sb.AppendFormat(format, "site_name", config.ProjectEnvironment == ProjectEnvironment.Production ? config.ProjectDisplayName : String.Format("{0} ({1})", config.ProjectDisplayName, config.ProjectEnvironment.GetDescription()));

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// Emits Facebook SDK script which is needed for facebook controls like face book comments, and places in layout page immediately after the body tag.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="show">Optionally returns empty result. For example @Html.FacebookSdk(ViewBag.LoadFacebook as bool?)</param>
        /// <returns>An HTML string for the Facebook SDK scripts.</returns>
        public static MvcHtmlString FacebookSdk(this HtmlHelper html, bool? show = true)
        {
            string format = @"<div id=""fb-root""></div><script>(function (d, s, id) {{ var js, fjs = d.getElementsByTagName(s)[0];if (d.getElementById(id)) return; js = d.createElement(s); js.id = id; js.src = '//connect.facebook.net/en_GB/all.js#xfbml=1&appId={0}'; fjs.parentNode.insertBefore(js, fjs); }}(document, 'script', 'facebook-jssdk'));</script>";
            var result = MvcHtmlString.Empty;
            if (show.GetValueOrDefault(false)) result = result.Format(format, ProjectConfigurationSection.GetConfig().Facebook.FacebookAppId);
            return result;
        }

        /// <summary>
        /// Returns HTML element for Facebook comments box.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="url">The URL for Facebook comments box.</param>
        /// <param name="numberPostsToShow">Number of posts to display.</param>
        /// <param name="controlWidth">The width of the control.</param>
        /// <param name="theme">The Facebook color theme defined by BlueChilli.Web.Misc.FacebookTheme.</param>
        /// <param name="responsive">True to render for mobile, otherwise not.</param>
        /// <returns>An HTML element for Facebook comments box.</returns>
        /// <remarks>http://developers.facebook.com/docs/reference/plugins/comments/</remarks>
        public static MvcHtmlString FacebookComments(this HtmlHelper html, string url = "", int numberPostsToShow = 10, int controlWidth = 470, FacebookTheme theme = FacebookTheme.Light, bool responsive = false)
        {
            string format = @"<div class=""facebook-control""><div class=""fb-comments"" data-href=""{0}"" data-num-posts=""{1}"" data-width=""{2}"" data-colorscheme=""{3}"" data-mobile=""{4}""></div></div>";
            url = url.DefaultTo(html.ViewContext.RequestContext.HttpContext.Request.Url.AbsoluteUri);
            return MvcHtmlString.Empty.Format(format, ResolveUrl(html, url), numberPostsToShow, controlWidth, theme.GetDescription(), responsive.ToString().ToLower());
        }

        /// <summary>
        /// Returns HTML element for Facebook like button.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="url">The URL for Facebook like button.</param>
        /// <param name="facebookLikeStyle">The style defined by BlueChilli.Web.Misc.FacebookLikeStyle.</param>
        /// <param name="includeSend">True to include send button, otherwise not.</param>
        /// <param name="text">The text defined by BlueChilli.Web.Misc.FacebookLikeText.</param>
        /// <param name="theme">The Facebook color theme defined by BlueChilli.Web.Misc.FacebookTheme.</param>
        /// <returns>An HTML element for Facebook comments like button.</returns>
        /// <remarks>http://developers.facebook.com/docs/reference/plugins/like/</remarks>
        public static MvcHtmlString FacebookLike(this HtmlHelper html, string url = "", FacebookLikeStyle facebookLikeStyle = FacebookLikeStyle.ButtonCount, bool includeSend = false, FacebookLikeText text = FacebookLikeText.Like, FacebookTheme theme = FacebookTheme.Light)
        {
            string format = @"<div class=""facebook-control""><div class=""fb-like"" data-href=""{0}"" data-layout=""{1}"" data-send=""{2}"" action=""{3}"" colorscheme=""{4}"" ></div></div>";
            return MvcHtmlString.Empty.Format(format, ResolveUrl(html, url), facebookLikeStyle.GetDescription(), includeSend.ToString().ToLower(), text.GetDescription(), theme.GetDescription());
        }

        private static string ResolveUrl(HtmlHelper html, string url)
        {
            if (String.IsNullOrEmpty(url)) return html.ViewContext.RequestContext.HttpContext.Request.Url.AbsoluteUri;
            return UriExtensions.Parse(url).AbsoluteUri;
        }
    }

    /// <summary>
    /// Enumeration values for Facebook color theme.
    /// </summary>
    public enum FacebookTheme
    {
        /// <summary>
        /// Dark theme.
        /// </summary>
        [Description("dark")]Dark,
        /// <summary>
        /// Light theme.
        /// </summary>
        [Description("light")]Light
    }

    /// <summary>
    /// Enumeration values for the style of Facebook like button.
    /// </summary>
    public enum FacebookLikeStyle
    {
        /// <summary>
        /// Minimum width: 225 pixels. Minimum increases by 40px if action is 'recommend' by and increases by 60px if send is 'true'.
        /// Default width: 450 pixels.
        /// Height: 35 pixels (without photos) or 80 pixels (with photos).
        /// </summary>
        [Description("standard")]Standard,
        /// <summary>
        /// Minimum width: 90 pixels.
        /// Default width: 90 pixels.
        /// Height: 20 pixels.
        /// </summary>
        [Description("button_count")]ButtonCount,
        /// <summary>
        /// Minimum width: 55 pixels.
        /// Default width: 55 pixels.
        /// Height: 65 pixels.
        /// </summary>
        [Description("box_count")]BoxCount
    }

    /// <summary>
    /// Enumeration values for Facebook like button text.
    /// </summary>
    public enum FacebookLikeText
    {
        /// <summary>
        /// Displays "like" on the button. 
        /// </summary>
        [Description("like")]Like,
        /// <summary>
        /// Displays "recommend" on the button.
        /// </summary>
        [Description("recommend")]Recommend
    }
}