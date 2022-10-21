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

namespace ChilliSource.Cloud.Core.Tests
{
    public class LocalStorageTests
    {        
        static LocalStorageTests()
        {
            GlobalConfiguration.Instance.SetMimeMapping(new MyMimeMapping());
        }

        [Fact]
        public void TestRealStorage()
        {
            var remoteStorage = new LocalStorageProvider(new LocalStorageConfiguration()
            {
                BasePath = "./testlocalstorage"
            });

            IFileStorage storage = FileStorageFactory.Create(remoteStorage);

            string contentType;
            long length;

            var fileName = $"myfolder/{Guid.NewGuid()}.txt";

            Assert.False(storage.Exists(fileName));

            var savedPath = storage.Save(new StorageCommand()
            {
                FileName = fileName,
                ContentType = "text/plain"
            }.SetStreamSource(GetSampleMemoryStream("this is a sample.")));

            Assert.True(!String.IsNullOrEmpty(savedPath));

            Assert.True(storage.Exists(savedPath));

            var file = storage.GetContent(savedPath, null, out length, out contentType);
            byte[] bytes = new byte[length];
            file.Read(bytes, 0, (int)length);
            var downloadedStr = Encoding.UTF8.GetString(bytes);

            Assert.Equal("this is a sample.", downloadedStr);

            Assert.Equal("text/plain", contentType);

            storage.Delete(savedPath);

            Assert.False(storage.Exists(fileName));
        }

        [Fact]
        public void TestRealStorageWithoutMetadata()
        {
            var remoteStorage = new LocalStorageProvider(new LocalStorageConfiguration()
            {
                BasePath = "./testlocalstorage"
            });

            IFileStorage storage = FileStorageFactory.Create(remoteStorage);

            string contentType;
            long length;

            var fileName = $"myfolder/{Guid.NewGuid()}.txt";

            Assert.False(storage.Exists(fileName));

            var savedPath = storage.Save(new StorageCommand()
            {
                FileName = fileName,
                ContentType = "text/plain"
            }.SetStreamSource(GetSampleMemoryStream("this is a sample.")));

            //Deletes local metadata file.
            var metadataFilePath = remoteStorage.GetMetadataJsonFilename(remoteStorage.GetAbsolutePath(fileName));
            File.Delete(metadataFilePath);

            Assert.True(!String.IsNullOrEmpty(savedPath));

            Assert.True(storage.Exists(savedPath));

            var file = storage.GetContent(savedPath, null, out length, out contentType);
            byte[] bytes = new byte[length];
            file.Read(bytes, 0, (int)length);
            var downloadedStr = Encoding.UTF8.GetString(bytes);

            Assert.Equal("this is a sample.", downloadedStr);

            //Content-type should be infered from the file extension because we deleted the metadata file.
            Assert.Equal("text/plain", contentType);

            storage.Delete(savedPath);

            Assert.False(storage.Exists(fileName));
        }

        public MemoryStream GetSampleMemoryStream(string text)
        {
            var source = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(text);
            source.Write(bytes, 0, bytes.Length);

            source.Position = 0;
            return source;
        }
    }


    public class MyMimeMapping : IMimeMapping
    {
        public string GetMimeType(string fileName)
        {
            return MimeMapping.GetMimeMapping(fileName);
        }

        public string GetExtension(string mimeType)
        {
            throw new NotImplementedException();
        }
    }

}
