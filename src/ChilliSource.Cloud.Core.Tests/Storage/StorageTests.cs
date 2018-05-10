using ChilliSource.Core.Extensions;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class StorageTests
    {
        private readonly Mock<IRemoteStorage> _remoteStorage;
        private readonly FileStorage _fixture;

        public StorageTests()
        {
            _remoteStorage = new Mock<IRemoteStorage>();
            _fixture = new FileStorage(_remoteStorage.Object);
        }

        [Fact]
        public async Task SaveAsync_ShouldSaveFile()
        {
            var stream = new MemoryStream();
            var result = "test.txt";
            StorageCommand command = new StorageCommand()
            {
                ContentType = "text/plain",
                FileName = "test.txt"
            };

            command.SetStreamSource(stream, "txt");
            _remoteStorage.Setup(x => x.SaveAsync(It.IsAny<Stream>(), command.FileName, command.ContentType))
            .Returns(Task<string>.FromResult<string>(result))
            .Verifiable();

            var res = await _fixture.SaveAsync(command);
            _remoteStorage.Verify();
            Assert.Equal(result, res);
        }

        [Fact]
        public async Task SaveAsync_ShouldAppendFolderToFileNameIfProvided()
        {
            var stream = new MemoryStream();
            var result = "Test/test.txt";
            StorageCommand command = new StorageCommand()
            {
                ContentType = "text/plain",
                FileName = "test.txt",
                Folder = "Test"
            };

            command.SetStreamSource(stream, "txt");
            _remoteStorage.Setup(x => x.SaveAsync(It.IsAny<Stream>(), result, command.ContentType))
            .Returns(Task<string>.FromResult<string>(result))
            .Verifiable();

            var res = await _fixture.SaveAsync(command);
            _remoteStorage.Verify();
            Assert.Equal(result, res);
        }

        [Fact]
        public async Task SaveAsync_ShouldDetermineContentTypeFromExtensionIfNotProvided()
        {
            var _mimeMapping = new Mock<IMimeMapping>();
            _mimeMapping.Setup(x => x.GetMimeType(It.IsAny<string>())).Returns("text/plain");

            GlobalConfiguration.Instance.SetMimeMapping(_mimeMapping.Object);

            var stream = new MemoryStream();
            var result = "Test/test.txt";
            StorageCommand command = new StorageCommand()
            {
                FileName = "test.txt",
                Folder = "Test"
            };

            command.SetStreamSource(stream, "txt");
            _remoteStorage.Setup(x => x.SaveAsync(It.IsAny<Stream>(), result, "text/plain"))
            .Returns(Task<string>.FromResult<string>(result))
            .Verifiable();

            var res = await _fixture.SaveAsync(command);
            _remoteStorage.Verify();
            Assert.Equal(result, res);
        }

    }


}
