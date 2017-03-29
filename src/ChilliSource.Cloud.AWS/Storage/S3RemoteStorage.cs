using Amazon.S3;
using Amazon.S3.Model;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.AWS
{
    public class S3RemoteStorage : IRemoteStorage
    {
        S3Element _s3Config;
        private string _host;

        public S3RemoteStorage(S3Element s3Config)
        {
            _s3Config = s3Config ?? ProjectConfigurationSection.GetConfig().FileStorage?.S3;
            if (_s3Config == null)
            {
                throw new ApplicationException("S3 storage element not found in the configuration file");
            }

            _host = _s3Config.Host ?? "https://s3.amazonaws.com";
            if (!_host.StartsWith("http"))
                _host = $"https://{_host}";
        }

        private AmazonS3Client GetClient()
        {
            return new AmazonS3Client(_s3Config.AccessKeyId, _s3Config.SecretAccessKey, new AmazonS3Config()
            {
                ServiceURL = _host
            });
        }

        private static string EncodeKey(string key)
        {
            return key?.Replace('\\', '/');
        }

        private PutObjectRequest CreatePutRequest(Stream stream, string fileName, string contentType)
        {
            return new PutObjectRequest()
            {
                BucketName = _s3Config.Bucket,
                Key = EncodeKey(fileName),
                ContentType = contentType,
                InputStream = stream,
                AutoCloseStream = false
            };
        }

        public async Task DeleteAsync(string fileToDelete)
        {
            using (var s3Client = GetClient())
            {
                try
                {
                    await s3Client.DeleteObjectAsync(_s3Config.Bucket, EncodeKey(fileToDelete))
                          .IgnoreContext();
                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.StatusCode != HttpStatusCode.NotFound)
                        throw;
                }
            }
        }

        public async Task<FileStorageResponse> GetContentAsync(string fileName)
        {
            using (var s3Client = GetClient())
            using (var response = await s3Client.GetObjectAsync(_s3Config.Bucket, EncodeKey(fileName))
                                        .IgnoreContext())
            {
                var contentLength = response.Headers.ContentLength;
                var contentType = response.Headers.ContentType;

                var memStream = new MemoryStream((int)contentLength);
                await response.ResponseStream.CopyToAsync(memStream, Math.Min(80 * 1024, (int)contentLength))
                       .IgnoreContext();

                memStream.Position = 0;

                return FileStorageResponse.Create(fileName, contentLength, contentType, memStream);
            }
        }

        public async Task SaveAsync(Stream stream, string fileName, string contentType)
        {
            using (var s3Client = GetClient())
            {
                var putRequest = CreatePutRequest(stream, fileName, contentType);
                var response = await s3Client.PutObjectAsync(putRequest)
                                     .IgnoreContext();
            }
        }

        public async Task<bool> ExistsAsync(string fileName)
        {
            using (var s3Client = GetClient())
            {
                try
                {
                    var request = new GetObjectMetadataRequest()
                    {
                        BucketName = _s3Config.Bucket,
                        Key = EncodeKey(fileName)
                    };

                    var response = await s3Client.GetObjectMetadataAsync(request)
                                         .IgnoreContext();
                    return true;
                }
                catch (AmazonS3Exception ex)
                {
                    if (!string.Equals(ex.ErrorCode, "NoSuchBucket") && (ex.StatusCode == HttpStatusCode.NotFound))
                    {
                        return false;
                    }
                    throw;
                }
            }
        }
    }
}
