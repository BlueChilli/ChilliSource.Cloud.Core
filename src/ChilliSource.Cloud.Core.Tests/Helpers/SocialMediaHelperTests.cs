using ChilliSource.Core.Extensions;
using System;
using System.IO;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class SocialMediaHelperTests
    {

        [Fact]
        public void IsValidUrl_ChecksYouTubeUrl_IsValid()
        {
            var url1 = "http://www.youtube.com/watch?v=-wtIMTCHWuI&param=test";

            var url2 = "https://www.youtube.com/v/-wtIMTCHWuI?version=3&autohide=1";

            var url3 = "http://youtu.be/-wtIMTCHWuI";

            //not supported var url4 = "http://www.youtube.com/oembed?url=http%3A//www.youtube.com/watch?v%3D-wtIMTCHWuI&format=json";

            var url5 = "http://www.google.com";

            var url6 = "http://www.youtube.com/wtIMTCHWuI";

            Assert.True(SocialMediaHelper.IsValidUrl(url1, SocialMediaType.YouTubeVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url2, SocialMediaType.YouTubeVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url3, SocialMediaType.YouTubeVideo));
            //Assert.True(SocialMediaHelper.IsValidUrl(url4, SocialMediaType.YouTubeVideo));
            Assert.False(SocialMediaHelper.IsValidUrl(url5, SocialMediaType.YouTubeVideo));
            Assert.False(SocialMediaHelper.IsValidUrl(url6, SocialMediaType.YouTubeVideo));

            Assert.Equal("-wtIMTCHWuI", SocialMediaHelper.GetId(url1, SocialMediaType.YouTubeVideo));
            Assert.Equal("-wtIMTCHWuI", SocialMediaHelper.GetId(url2, SocialMediaType.YouTubeVideo));
            Assert.Equal("-wtIMTCHWuI", SocialMediaHelper.GetId(url3, SocialMediaType.YouTubeVideo));
        }

        [Fact]
        public void IsValidUrl_ChecksVimeoUrl_IsValid()
        {
            var url1 = "https://vimeo.com/12345678";
            var url2 = "http://vimeo.com/12345678";
            var url3 = "https://www.vimeo.com/12345678";
            var url4 = "http://vimeo.com/channels/12345678";
            var url5 = "https://vimeo.com/groups/name/videos/12345678";
            var url6 = "http://vimeo.com/album/2222222/video/12345678";
            var url7 = "http://vimeo.com/12345678?param=test";
            var url8 = "http://player.vimeo.com/video/12345678";

            var url9 = "http://www.google.com";
            var url10 = "http://www.vimeo.com/ABCDEFGH";

            Assert.True(SocialMediaHelper.IsValidUrl(url1, SocialMediaType.VimeoVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url2, SocialMediaType.VimeoVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url3, SocialMediaType.VimeoVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url4, SocialMediaType.VimeoVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url5, SocialMediaType.VimeoVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url6, SocialMediaType.VimeoVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url7, SocialMediaType.VimeoVideo));
            Assert.True(SocialMediaHelper.IsValidUrl(url8, SocialMediaType.VimeoVideo));

            Assert.False(SocialMediaHelper.IsValidUrl(url9, SocialMediaType.VimeoVideo));
            Assert.False(SocialMediaHelper.IsValidUrl(url10, SocialMediaType.VimeoVideo));

            Assert.Equal("12345678", SocialMediaHelper.GetId(url1, SocialMediaType.VimeoVideo));
            Assert.Equal("12345678", SocialMediaHelper.GetId(url2, SocialMediaType.VimeoVideo));
            Assert.Equal("12345678", SocialMediaHelper.GetId(url3, SocialMediaType.VimeoVideo));
            Assert.Equal("12345678", SocialMediaHelper.GetId(url4, SocialMediaType.VimeoVideo));
            Assert.Equal("12345678", SocialMediaHelper.GetId(url5, SocialMediaType.VimeoVideo));
            Assert.Equal("12345678", SocialMediaHelper.GetId(url6, SocialMediaType.VimeoVideo));
            Assert.Equal("12345678", SocialMediaHelper.GetId(url7, SocialMediaType.VimeoVideo));
            Assert.Equal("12345678", SocialMediaHelper.GetId(url8, SocialMediaType.VimeoVideo));
        }

        [Fact]
        public void IsValidUrl_ChecksTwitterUrl_IsValid()
        {
            var url1 = "http://twitter.com/example";
            var url2 = "https://twitter.com/example";
            var url3 = "http://www.twitter.com/example";
            var url4 = "https://www.twitter.com/example?param=test";

            var url5 = "http://www.google.com";
            var url6 = "http://www.twitter.com";

            Assert.True(SocialMediaHelper.IsValidUrl(url1, SocialMediaType.TwitterProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url2, SocialMediaType.TwitterProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url3, SocialMediaType.TwitterProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url4, SocialMediaType.TwitterProfile));

            Assert.False(SocialMediaHelper.IsValidUrl(url5, SocialMediaType.TwitterProfile));
            Assert.False(SocialMediaHelper.IsValidUrl(url6, SocialMediaType.TwitterProfile));

            Assert.Equal("example", SocialMediaHelper.GetId(url1, SocialMediaType.TwitterProfile));
            Assert.Equal("example", SocialMediaHelper.GetId(url2, SocialMediaType.TwitterProfile));
            Assert.Equal("example", SocialMediaHelper.GetId(url3, SocialMediaType.TwitterProfile));
            Assert.Equal("example", SocialMediaHelper.GetId(url4, SocialMediaType.TwitterProfile));
        }

        [Fact]
        public void IsValidUrl_ChecksLinkedInUrl_IsValid()
        {
            var url1 = "http://uk.linkedin.com/pub/some-name/1/1b3/b45/";
            var url2 = "https://nl.linkedin.com/pub/some-name/11/223/544";
            var url3 = "http://www.linkedin.com/in/some-name/";
            var url4 = "http://linkedin.com/in/some-name?param=test";
            var url5 = "http://nl.linkedin.com/in/some-name";
            var url6 = "http://nl.linkedin.com/in/some-name/";

            var url7 = "http://www.google.com";
            var url8 = "http://www.linkedin.com";

            Assert.True(SocialMediaHelper.IsValidUrl(url1, SocialMediaType.LinkedInProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url2, SocialMediaType.LinkedInProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url3, SocialMediaType.LinkedInProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url4, SocialMediaType.LinkedInProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url5, SocialMediaType.LinkedInProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url6, SocialMediaType.LinkedInProfile));

            Assert.False(SocialMediaHelper.IsValidUrl(url7, SocialMediaType.LinkedInProfile));
            Assert.False(SocialMediaHelper.IsValidUrl(url8, SocialMediaType.LinkedInProfile));
        }


        [Fact]
        public void IsValidUrl_ChecksFacebookUrl_IsValid()
        {
            var url1 = "https://www.facebook.com/example";
            var url2 = "http://facebook.com/example?parm=test";
            var url3 = "http://www.facebook.com/#!/example";
            var url4 = "http://www.facebook.com/pages/Paris-France/Vanity-Url/123456?v=app_555";
            var url5 = "http://www.facebook.com/pages/Vanity-Url/123456";
            var url6 = "http://www.facebook.com/bounce_page#!/pages/Vanity-Url/123456";
            var url7 = "http://www.facebook.com/bounce_page#!/example?v=app_166292090072334";

            var url9 = "http://www.google.com";
            var url10 = "http://www.twitter.com";

            Assert.True(SocialMediaHelper.IsValidUrl(url1, SocialMediaType.FacebookProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url2, SocialMediaType.FacebookProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url3, SocialMediaType.FacebookProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url4, SocialMediaType.FacebookProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url5, SocialMediaType.FacebookProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url6, SocialMediaType.FacebookProfile));
            Assert.True(SocialMediaHelper.IsValidUrl(url7, SocialMediaType.FacebookProfile));

            Assert.False(SocialMediaHelper.IsValidUrl(url9, SocialMediaType.FacebookProfile));
            Assert.False(SocialMediaHelper.IsValidUrl(url10, SocialMediaType.FacebookProfile));

            Assert.Equal("example", SocialMediaHelper.GetId(url1, SocialMediaType.FacebookProfile));
            Assert.Equal("example", SocialMediaHelper.GetId(url2, SocialMediaType.FacebookProfile));
            Assert.Equal("example", SocialMediaHelper.GetId(url3, SocialMediaType.FacebookProfile));
            Assert.Equal("123456", SocialMediaHelper.GetId(url4, SocialMediaType.FacebookProfile));
            Assert.Equal("123456", SocialMediaHelper.GetId(url5, SocialMediaType.FacebookProfile));
            Assert.Equal("123456", SocialMediaHelper.GetId(url6, SocialMediaType.FacebookProfile));
            Assert.Equal("example", SocialMediaHelper.GetId(url7, SocialMediaType.FacebookProfile));
        }

        [Fact]
        public void CategorizeUrl_ReturnsCorrect_SocialMediaType()
        {
            var url1 = "http://www.youtube.com/watch?v=-wtIMTCHWuI";
            var url2 = "http://www.google.com";
            var url3 = "https://vimeo.com/12345678";
            var url4 = "http://twitter.com/example";
            var url5 = "http://www.linkedin.com/in/some-name/";
            var url6 = "https://www.facebook.com/my_page_id";

            Assert.Equal(SocialMediaType.YouTubeVideo, SocialMediaHelper.CategorizeUrl(url1));
            Assert.Equal(SocialMediaType.Unknown, SocialMediaHelper.CategorizeUrl(url2));
            Assert.Equal(SocialMediaType.VimeoVideo, SocialMediaHelper.CategorizeUrl(url3));
            Assert.Equal(SocialMediaType.TwitterProfile, SocialMediaHelper.CategorizeUrl(url4));
            Assert.Equal(SocialMediaType.LinkedInProfile, SocialMediaHelper.CategorizeUrl(url5));
            Assert.Equal(SocialMediaType.FacebookProfile, SocialMediaHelper.CategorizeUrl(url6));
        }


    }


}
