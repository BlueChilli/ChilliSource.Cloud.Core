using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Contains methods for social medias (Facebook, Twitter, LinkIn, YouTube, Vimeo)
    /// </summary>
    public static class SocialMediaHelper
    {
        /// <summary>
        /// Check that a social media url is valid
        /// </summary>
        /// <param name="url">Url to check is valid</param>
        /// <param name="type">Social media type to validate against. By default will check that url is valid for any social media type.</param>
        /// <returns>Returns true when valid otherwise false</returns>
        public static bool IsValidUrl(string url, SocialMediaType type = SocialMediaType.Unknown)
        {
            if (String.IsNullOrEmpty(url)) return false;

            if (type == SocialMediaType.Unknown)
            {
                return CategorizeUrl(url) != SocialMediaType.Unknown;
            }

            switch (type)
            {
                case SocialMediaType.YouTubeVideo: return IsValidYouTubeUrl(url);
                case SocialMediaType.VimeoVideo: return IsValidVimeoUrl(url);
                case SocialMediaType.TwitterProfile: return IsValidTwitterUrl(url);
                case SocialMediaType.LinkedInProfile: return IsValidLinkedInUrl(url);
                case SocialMediaType.FacebookProfile: return IsValidFacebookUrl(url);
            }
            throw new NotSupportedException($"Social media type {type} not supported");
        }

        public static SocialMediaType CategorizeUrl(string url)
        {
            if (IsValidYouTubeUrl(url)) return SocialMediaType.YouTubeVideo;
            if (IsValidVimeoUrl(url)) return SocialMediaType.VimeoVideo;
            if (IsValidFacebookUrl(url)) return SocialMediaType.FacebookProfile;
            if (IsValidTwitterUrl(url)) return SocialMediaType.TwitterProfile;
            if (IsValidLinkedInUrl(url)) return SocialMediaType.LinkedInProfile;

            return SocialMediaType.Unknown;
        }

        /// <summary>
        /// Returns social media id from url
        /// </summary>
        /// <param name="url">Url to parse for id</param>
        /// <param name="type">Social media type to target</param>
        /// <returns>If found returns social media id otherwise returns empty string</returns>
        public static string GetId(string url, SocialMediaType type)
        {
            if (String.IsNullOrEmpty(url)) return String.Empty;

            switch (type)
            {
                case SocialMediaType.YouTubeVideo: return IsValidYouTubeUrl(url) ? GetYouTubeId(url) : String.Empty;
                case SocialMediaType.VimeoVideo: return IsValidVimeoUrl(url) ? GetVimeoId(url) : String.Empty;
                case SocialMediaType.TwitterProfile: return IsValidTwitterUrl(url) ? GetTwitterId(url) : String.Empty;
                case SocialMediaType.FacebookProfile: return IsValidFacebookUrl(url) ? GetFacebookId(url) : String.Empty;
            }
            throw new NotSupportedException($"Social media type {type} not supported");
        }

        #region Facebook

        /// <summary>
        /// Regular expression to validate facebook profile url
        /// </summary>
        public const string FacebookRegex = @"(?:https?:\/\/)?(?:www\.)?facebook\.com\/(?:(?:\w)*#!\/)?(?:pages\/)?(?:[\w\-]*\/)*([\w\-\.]*)";

        private static bool IsValidFacebookUrl(string url)
        {
            var match = new Regex(SocialMediaHelper.FacebookRegex).Match(url);
            return (match.Success && match.Groups.Count == 2 && match.Groups[1].Success);
        }

        private static string GetFacebookId(string url)
        {
            var groups = new Regex(SocialMediaHelper.FacebookRegex).Match(url).Groups;
            return groups[1].Value;
        }

        #endregion

        #region Youtube
        /// <summary>
        /// Regular expression to validate Youtube video link. VideoId is in group 9.
        /// </summary>
        /// <example>new Regex(SocialHelper.YouTubeRegex).Match(url).Groups[9].Value;.</example>
        public const string YouTubeRegex = "(https?://)?(www\\.)?(youtu\\.be/|youtube\\.com/)?((.+/)?(watch(\\?v=|.+&v=))?(v=)?)([\\w_-]{11})(&.+)?";

        private static bool IsValidYouTubeUrl(string url)
        {
            var match = new Regex(SocialMediaHelper.YouTubeRegex).Match(url);
            return (match.Success && match.Groups.Count > 10 && match.Groups[3].Success && match.Groups[9].Success);
        }
        private static string GetYouTubeId(string link)
        {
            return new Regex(SocialMediaHelper.YouTubeRegex).Match(link).Groups[9].Value;
        }
        #endregion

        #region Twitter
        /// <summary>
        /// Regular expression to validate twitter profile url
        /// </summary>
        public const string TwitterRegex = @"(http(?:s)?:\/\/(?:www\.)?twitter\.com\/([a-zA-Z0-9_]+))";

        private static bool IsValidTwitterUrl(string url)
        {
            var match = new Regex(SocialMediaHelper.TwitterRegex).Match(url);
            return (match.Success && match.Groups.Count == 3 && match.Groups[2].Success);
        }

        private static string GetTwitterId(string url)
        {
            var groups = new Regex(SocialMediaHelper.TwitterRegex).Match(url).Groups;
            return groups[2].Value;
        }
        #endregion

        #region LinkedIn
        /// <summary>
        /// Regular expression to validate linkedin profile url
        /// </summary>
        public const string LinkedInRegex = @"^https?:\/\/?((www|\w\w)\.)?linkedin.com(\w+:{0,1}\w*@)?(\S+)(:([0-9])+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?";

        private static bool IsValidLinkedInUrl(string url)
        {
            var match = new Regex(SocialMediaHelper.LinkedInRegex).Match(url);
            return (match.Success && match.Groups.Count == 9 && match.Groups[0].Success && match.Groups[4].Success);
        }

        #endregion

        #region Vimeo
        public const string VimeoRegex = @"vimeo\.com/(\w*/)*(\d+)";

        private static bool IsValidVimeoUrl(string url)
        {
            var match = new Regex(VimeoRegex).Match(url);
            return (match.Success && match.Groups.Count > 2 && match.Groups[2].Success);
        }

        private static string GetVimeoId(string link)
        {
            return new Regex(VimeoRegex).Match(link).Groups[2].Value;
        }
        #endregion

    }

    public enum SocialMediaType
    {
        Unknown,
        YouTubeVideo,
        VimeoVideo,
        TwitterProfile,
        FacebookProfile,
        LinkedInProfile
    }
}
