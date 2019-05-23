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
        Task SaveAsync(Stream stream, FileStorageMetadataInfo metadata, CancellationToken cancellationToken);
        Task DeleteAsync(string fileToDelete, CancellationToken cancellationToken);
        Task<FileStorageResponse> GetContentAsync(string fileName, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken);
        Task<IFileStorageMetadataResponse> GetMetadataAsync(string fileName, CancellationToken cancellationToken);
    }

    public class FileStorageMetadataInfo
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string CacheControl { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentDisposition { get; set; }
    }

    public interface IFileStorageMetadataResponse
    {
        string FileName { get; }
        DateTime LastModifiedUtc { get; }
        long ContentLength { get; }
        string ContentType { get; }
        string CacheControl { get; }
        string ContentEncoding { get; }
        string ContentDisposition { get; }
    }

    public class FileStorageMetadataResponse : IFileStorageMetadataResponse
    {
        public FileStorageMetadataResponse() { }

        public string FileName { get; set; }
        public DateTime LastModifiedUtc { get; set; }
        public long ContentLength { get; set; }
        public string ContentType { get; set; }
        public string CacheControl { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentDisposition { get; set; }
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
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            return new FileStorageResponse(fileName, contentLength, contentType, stream);
        }

        public static FileStorageResponse Create(IFileStorageMetadataResponse metadata, Stream stream)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            var response = FileStorageResponse.Create(metadata.FileName, metadata.ContentLength, metadata.ContentType, stream);
            response.LastModifiedUtc = metadata.LastModifiedUtc;
            response.CacheControl = metadata.CacheControl;
            response.ContentEncoding = metadata.ContentEncoding;
            response.ContentDisposition = metadata.ContentDisposition;

            return response;
        }

        public string FileName { get; internal set; }
        public long ContentLength { get; internal set; }
        public string ContentType { get; internal set; }
        public Stream Stream { get; internal set; }
        public DateTime LastModifiedUtc { get; internal set; }
        public string CacheControl { get; internal set; }
        public string ContentEncoding { get; internal set; }

        public string ContentDisposition { get; internal set; }
    }
}
