using ChilliSource.Core.Extensions;
using Moq;
using System;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace ChilliSource.Cloud.Core.NetStandard.Tests
{
    public class LocalStorageTests
    {

        [Fact]
        public void TestAccessKey()
        {
            var remoteStorage = new LocalStorageProvider(new LocalStorageConfiguration()
            {
                BasePath = "./testlocalstorage"
            });

            IFileStorage storage = FileStorageFactory.Create(remoteStorage);

            var filename = "myfilename.pdf";
            var url = storage.GetPreSignedUrl(filename, TimeSpan.FromMinutes(5));

            Assert.NotNull(url);
            Assert.StartsWith(filename, url);

            var accessKey = url.Substring(url.IndexOf('?') + 1);
            Assert.NotNull(accessKey);
            Assert.StartsWith("accesskey", accessKey);

            accessKey = url.Substring(url.IndexOf('=') + 1);
            Assert.NotNull(accessKey);

            Assert.True(remoteStorage.IsValidPreSignedUrl(filename, accessKey));

            url = storage.GetPreSignedUrl(filename, TimeSpan.FromMinutes(-5));
            accessKey = url.Substring(url.IndexOf('=') + 1);
            Assert.False(remoteStorage.IsValidPreSignedUrl(filename, accessKey));
        }
    }

}
