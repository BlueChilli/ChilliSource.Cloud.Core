using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public interface IRemoteStorage
    {
        Task SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken);
        Task DeleteAsync(string fileToDelete, CancellationToken cancellationToken);
        Task<FileStorageResponse> GetContentAsync(string fileName, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken);
    }

    public class FileStorageResponse
    {
        private FileStorageResponse(string fileName, long contentLength, string contentType, Stream stream)
        {
            FileName = fileName;
            ContentLength = contentLength;
            ContentType = contentType;
            Stream = stream;
        }

        public static FileStorageResponse Create(string fileName, long contentLength, string contentType, Stream stream)
        {
            return new FileStorageResponse(fileName, contentLength, contentType, stream);
        }

        public string FileName { get; internal set; }
        public long ContentLength { get; internal set; }
        public string ContentType { get; internal set; }
        public Stream Stream { get; internal set; }
    }
}
