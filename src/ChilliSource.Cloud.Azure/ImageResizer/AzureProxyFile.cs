using ImageResizer.Plugins;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace ChilliSource.Cloud.Azure.ImageResizer
{ 
    internal class AzureProxyFile : VirtualFile, IVirtualFile, IVirtualFileWithModifiedDate, IVirtualFileSourceCacheKey
    {
        private ICloudBlob _blobRef;

        private AzureProxyFile(string blobName, ICloudBlob blobRef) : base(blobName)
        {
            _blobRef = blobRef;
        }

        public static AzureProxyFile Create(string blobName, AzureVirtualPathProvider pathProvider)
        {
            var blobUri = new System.Uri(string.Format("{0}/{1}", pathProvider.CloudBlobClient.BaseUri.OriginalString.TrimEnd(new char[] { '/', '\\' }), blobName));

            try
            {
                var blobRef = pathProvider.CloudBlobClient.GetBlobReferenceFromServer(blobUri);

                return new AzureProxyFile(blobName, blobRef);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public BlobProperties Properties { get { return _blobRef.Properties; } }

        public override System.IO.Stream Open()
        {
            var stream = new System.IO.MemoryStream(4096);

            _blobRef.DownloadToStream(stream);

            stream.Position = 0;
            return stream;
        }

        public string GetCacheKey(bool includeModifiedDate)
        {
            var includeStr = includeModifiedDate ? $"_{this.ModifiedDateUTC.Ticks}" : "";
            return $"{base.VirtualPath}{includeStr}";
        }

        string IVirtualFile.VirtualPath
        {
            get
            {
                return base.VirtualPath;
            }
        }

        public DateTime ModifiedDateUTC
        {
            get
            {
                return this.Properties.LastModified?.UtcDateTime ?? new DateTime(0, DateTimeKind.Utc);
            }
        }
    }
}
