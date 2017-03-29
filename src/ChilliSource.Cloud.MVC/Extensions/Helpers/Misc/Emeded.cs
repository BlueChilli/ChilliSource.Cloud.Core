
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.ComponentModel;
using System.Web.Mvc;

//Named so to not pollute @Html
namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents support for rendering Emdeded (IFRAME) players for YouTube and Vimeo
    /// </summary>
    public static class EmbededHtmlHelper
    {
        /// <summary>
        /// Returns HTML and script element for YouTube.
        /// </summary>
        /// <param name="videoId">YouTube video id.</param>
        /// <param name="height">The height of the control in pixels.</param>
        /// <param name="width">The width of the control, defaults to 16:9 ratio + 25px for controls.</param>
        /// <param name="allowFullScreen">True to allow full screen mode, otherwise not.</param>
        /// <param name="showRelatedVideos">True to display related videos, otherwise not.</param>
        /// <param name="showYouTubeBranding">True to display YouTube branding, otherwise not.</param>
        /// <param name="windowMode">The window mode defined by BlueChilli.Web.Misc.YouTubeWindowMode.</param>
        /// <param name="theme">The theme defined by BlueChilli.Web.Misc.YouTubeTheme.</param>
        /// <returns>An HTML and script element for YoutTube.</returns>
        /// <remarks>https://developers.google.com/youtube/player_parameters</remarks>
        public static MvcHtmlString YouTubeEmbed(this HtmlHelper helper, string videoId, int width, int? height = null, object htmlAttributes = null, bool allowFullScreen = true, bool showRelatedVideos = false, bool showYouTubeBranding = true, YouTubeWindowMode windowMode = YouTubeWindowMode.None, YouTubeTheme theme = YouTubeTheme.Dark, bool autoPlay = false)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            string format = @"<iframe width=""{0}px"" height=""{1}px"" frameborder=""0"" src=""//www.youtube.com/embed/{2}?fs={3}&rel={4}&modestbranding={5}&wMode={6}&theme={7}&autoplay={8}"" {9}></iframe>";
            return MvcHtmlString.Empty.Format(format, width, height.GetValueOrDefault(width * 9 / 16 + 25), videoId, allowFullScreen.ToInt(), showRelatedVideos.ToInt(), showYouTubeBranding.Toggle().ToInt(), windowMode.GetDescription(), theme.GetDescription(), autoPlay.ToInt(), attributes.ToAttributeString());
        }

        public static MvcHtmlString VimeoEmbed(this HtmlHelper helper, string videoId, int width, int? height = null, object htmlAttributes = null, string color = "", bool autoPlay = false)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            string format = @"<iframe src=""https://player.vimeo.com/video/{0}?color={1}&title=0&byline=0&portrait=0&badge=0&autoplay={2}"" width=""{3}"" height=""{4}"" frameborder=""0"" webkitallowfullscreen mozallowfullscreen allowfullscreen {5}></iframe>";
            return MvcHtmlString.Empty.Format(format, videoId, color, autoPlay.ToInt(), width, height.GetValueOrDefault(width * 9 / 16 + 25), attributes.ToAttributeString());
        }
    }

    /// <summary>
    /// Enumeration values for BlueChilli.Web.Misc.YouTubeWindowMode.
    /// </summary>
    public enum YouTubeWindowMode
    {
        /// <summary>
        /// YouTube window mode has not been set.
        /// </summary>
        [Description("")]None,
        /// <summary>
        /// Opaque mode.
        /// </summary>
        [Description("opaque")]Opaque,
        /// <summary>
        /// Transparent mode.
        /// </summary>
        [Description("transparent")]Transparent
    }

    /// <summary>
    /// Enumeration values for BlueChilli.Web.Misc.YouTubeTheme.
    /// </summary>
    public enum YouTubeTheme
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
}