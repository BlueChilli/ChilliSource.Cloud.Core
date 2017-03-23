using ChilliSource.Cloud.Configuration;
using ChilliSource.Cloud.Data;
using ChilliSource.Cloud.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Azure
{
    internal class AzureRemoteStorage : IRemoteStorage
    {
        private AzureStorageElement _azureConfig;
        private CloudBlobContainer _storageContainer;

        public AzureRemoteStorage(AzureStorageElement azureConfig)
        {
            _azureConfig = azureConfig ?? ProjectConfigurationSection.GetConfig().FileStorage?.Azure;

            //var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=;AccountKey=");
            if (_azureConfig == null)
                throw new ApplicationException("Azure storage element not found in the configuration file");

            var storageAccount = new CloudStorageAccount(new StorageCredentials(_azureConfig.AccountName, _azureConfig.AccountKey), useHttps: true);
            var blobClient = storageAccount.CreateCloudBlobClient();

            _storageContainer = String.IsNullOrWhiteSpace(_azureConfig.Container) ?
                                    blobClient.GetRootContainerReference() :
                                    blobClient.GetContainerReference(_azureConfig.Container);
        }

        public async Task SaveAsync(Stream stream, string fileName, string contentType)
        {
            var fileRef = _storageContainer.GetBlockBlobReference(fileName);
            if (!String.IsNullOrEmpty(contentType))
            {
                fileRef.Properties.ContentType = contentType;
            }

            await fileRef.UploadFromStreamAsync(stream)
                  .IgnoreContext();
        }

        public async Task DeleteAsync(string fileToDelete)
        {
            var fileRef = _storageContainer.GetBlobReference(fileToDelete);
            await fileRef.DeleteIfExistsAsync()
                  .IgnoreContext();
        }

        public async Task<FileStorageResponse> GetContentAsync(string fileName)
        {
            var fileRef = _storageContainer.GetBlobReference(fileName);

            using (var stream = await fileRef.OpenReadAsync()
                               .IgnoreContext())
            {
                var contentLength = fileRef.Properties.Length;
                var contentType = fileRef.Properties.ContentType;

                var memStream = new MemoryStream((int)contentLength);
                await stream.CopyToAsync(memStream, Math.Min(80 * 1024, (int)contentLength))
                       .IgnoreContext();

                memStream.Position = 0;

                //lastModified = fileRef.Properties.LastModified?.UtcDateTime ?? new DateTime(0, DateTimeKind.Utc);            

                return FileStorageResponse.Create(fileName, contentLength, contentType, memStream);
            }
        }

        public async Task<bool> ExistsAsync(string fileName)
        {
            try
            {
                var fileRef = await _storageContainer.GetBlobReferenceFromServerAsync(fileName)
                              .IgnoreContext();
                return true;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw;
            }
        }
    }
}
