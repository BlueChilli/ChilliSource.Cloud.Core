
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Contains methods for social medias (Facebook, Twitter, LinkIn, YouTube)
    /// </summary>
    public static class SocialMediaHelper
    {
        /// <summary>
        /// Regular expression to validate facebook profile url
        /// </summary>
        public const string FaceBookRegex = @"(((https?:\/\/)?(www\.)?facebook\.com\/))(.*\/)?([a-zA-Z0-9.]*)($|\?.*)$";

        #region Youtube
        /// <summary>
        /// Regular expression to validate Youtube video link. VideoId is in group 9.
        /// </summary>
        /// <example>new Regex(SocialHelper.YouTubeRegex).Match(url).Groups[9].Value;.</example>
        public const string YouTubeRegex = "(https?://)?(www\\.)?(youtu\\.be/|youtube\\.com/)?((.+/)?(watch(\\?v=|.+&v=))?(v=)?)([\\w_-]{11})(&.+)?";

        /// <summary>
        /// Validates a link is a valid reference to a Youtube video
        /// </summary>
        /// <param name="link">The Youtube link.</param>
        /// <returns></returns>
        public static bool IsValidYouTubeUrl(string link)
        {
            if (String.IsNullOrEmpty(link)) return false;
            var match = new Regex(SocialMediaHelper.YouTubeRegex).Match(link);
            return (match.Success && match.Groups.Count > 10 && match.Groups[3].Success && match.Groups[9].Success);
        }

        /// <summary>
        /// Gets the Youtube video ID from the youtube link.
        /// Can be used in conjunction with ChilliSource.Cloud.Core.Web.Misc Html.YoutubeEmbed
        /// </summary>
        /// <param name="link">The Youtube link.</param>
        /// <returns>The Youtube video ID.</returns>
        public static string GetYouTubeId(string link)
        {
            return new Regex(SocialMediaHelper.YouTubeRegex).Match(link).Groups[9].Value;
        }
        #endregion

        /// <summary>
        /// Regular expression to validate twitter profile url or username. Username is in group 6
        /// </summary>
        public const string TwitterRegex = @"(@([a-zA-Z0-9_]{1,15}))|((https?:\/\/)?(www\.)?twitter.com\/([a-zA-Z0-9_]{1,15}))";

        /// <summary>
        /// Gets Twitter ID from the Twitter link.
        /// </summary>
        /// <param name="link">Teh Twitter link.</param>
        /// <returns>The Twitter ID.</returns>
        public static string GetTwitterId(string link)
        {
            var groups = new Regex(SocialMediaHelper.TwitterRegex).Match(link).Groups;
            return groups[1].Value.DefaultTo(groups[6].Value);
        }

        /// <summary>
        /// Regular expression to validate linkedin profile url
        /// </summary>
        public const string LinkInRegex = @"^https?:\/\/?((www|\w\w)\.)?linkedin.com(\w+:{0,1}\w*@)?(\S+)(:([0-9])+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?";

        #region Vimeo
        public const string VimeoRegex = @"vimeo\.com/(\w*/)*(\d+)";

        public static bool IsValidVimeoLink(string link)
        {
            if (String.IsNullOrEmpty(link)) return false;
            var match = new Regex(VimeoRegex).Match(link);
            return (match.Success && match.Groups.Count > 2 && match.Groups[2].Success);
        }

        public static string GetVimeoId(string link)
        {
            return new Regex(VimeoRegex).Match(link).Groups[2].Value;
        }
        #endregion

    }
}
