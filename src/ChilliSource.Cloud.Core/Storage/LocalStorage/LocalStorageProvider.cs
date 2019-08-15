using ChilliSource.Cloud.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public class LocalStorageProvider : IRemoteStorage
    {
        LocalStorageConfiguration _options;
        public LocalStorageProvider(LocalStorageConfiguration options)
        {
            _options = options.Clone();

            _options.BasePath = Path.GetFullPath(options.BasePath);
        }

        internal string GetAbsolutePath(string file)
        {
            return Path.Combine(_options.BasePath, file.Replace("/", "\\"));
        }

        public Task DeleteAsync(string fileToDelete, CancellationToken cancellationToken)
        {
            var path = GetAbsolutePath(fileToDelete);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken)
        {
            var path = GetAbsolutePath(fileName);
            var exists = File.Exists(path);

            return Task.FromResult(exists);
        }

        public async Task<FileStorageResponse> GetContentAsync(string fileName, CancellationToken cancellationToken)
        {
            var path = GetAbsolutePath(fileName);

            var metadata = await this.GetMetadataAsync(fileName, cancellationToken);
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                Action<Stream> disposingAction = (s) =>
                {
                    s?.Dispose();
                };

                var readonlyStream = ReadOnlyStreamWrapper.Create(fileStream, disposingAction, fileStream.Length);
                var response = FileStorageResponse.Create(metadata, readonlyStream);
                return response;
            }
            catch
            {
                fileStream?.Dispose();

                throw;
            }
        }

        public async Task SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken)
        {
            await SaveAsync(stream, new FileStorageMetadataInfo()
            {
                FileName = fileName,
                ContentType = contentType
            }, cancellationToken);
        }

        public async Task SaveAsync(Stream stream, FileStorageMetadataInfo metadata, CancellationToken cancellationToken)
        {
            var path = GetAbsolutePath(metadata.FileName);
            var directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream, 4096, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);

                fileStream.Close();
            }

            var metadataJsonContent = JsonConvert.SerializeObject(metadata);
            using (var metaFile = new StreamWriter(GetMetadataJsonFilename(path), false, Encoding.UTF8))
            {
                await metaFile.WriteAsync(metadataJsonContent);
                await metaFile.FlushAsync();

                metaFile.Close();
            }
        }

        internal string GetMetadataJsonFilename(string path)
        {
            var ext = Path.GetExtension(path);
            var metadataJsonFilename = Path.ChangeExtension(path, ext + ".meta");
            return metadataJsonFilename;
        }

        public async Task<IFileStorageMetadataResponse> GetMetadataAsync(string fileName, CancellationToken cancellationToken)
        {
            var path = GetAbsolutePath(fileName);
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"File not found", fileName);
            }

            FileStorageMetadataInfo metadataJson = null;
            var metadataFileInfo = new FileInfo(GetMetadataJsonFilename(path));
            if (metadataFileInfo.Exists)
            {                
                using (var reader = new StreamReader(metadataFileInfo.FullName, Encoding.UTF8))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var jsonContent = await reader.ReadToEndAsync(); // cancellationToken not supported
                    metadataJson = JsonConvert.DeserializeObject<FileStorageMetadataInfo>(jsonContent) ?? new FileStorageMetadataInfo();
                }
            }

            //Metadata file not found: populate content-type from fileName.
            metadataJson = metadataJson ?? new FileStorageMetadataInfo();
            metadataJson.FileName = metadataJson.FileName ?? fileName;
            metadataJson.ContentType = metadataJson.ContentType ?? GlobalConfiguration.Instance.GetMimeMapping().GetMimeType(fileName);            

            var metadata = new FileStorageMetadataResponse()
            {
                FileName = fileName,
                CacheControl = metadataJson.CacheControl,
                ContentDisposition = metadataJson.ContentDisposition,
                ContentEncoding = metadataJson.ContentEncoding,
                ContentLength = fileInfo.Length,
                ContentType = metadataJson.ContentType,
                LastModifiedUtc = fileInfo.LastWriteTimeUtc
            };

            return metadata;
        }
    }
}
