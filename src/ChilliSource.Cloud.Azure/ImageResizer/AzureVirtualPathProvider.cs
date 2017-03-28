using ImageResizer.Plugins;
using ImageResizer.Util;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Collections.Specialized;
using System.Web.Caching;
using System.Collections;

namespace ChilliSource.Cloud.Azure
{
    internal class AzureVirtualPathProvider : VirtualPathProvider, IVirtualImageProvider
    {
        private string _absolutePrefix = null;
        private CloudBlobClient _cloudBlobClient;

        private class EmptyCacheDependency : CacheDependency
        {
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (this.IsAzurePath(virtualPath))
            {
                return new EmptyCacheDependency();
            }

            return base.Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        public string AbsolutePrefix
        {
            get
            {
                return this._absolutePrefix;
            }
        }

        private void SetRelativePrefix(string value)
        {
            if (!value.EndsWith("/"))
            {
                value += "/";
            }
            this._absolutePrefix = ((value != null) ? PathUtils.ResolveAppRelativeAssumeAppRelative(value) : value);
        }

        public CloudBlobClient CloudBlobClient
        {
            get
            {
                return this._cloudBlobClient;
            }
            private set
            {
                this._cloudBlobClient = value;
            }
        }

        public AzureVirtualPathProvider(string blobStorageConnection, string relativePrefix)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobStorageConnection);
            this.CloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            this.SetRelativePrefix(relativePrefix);
        }

        public bool IsAzurePath(string virtualPath)
        {
            return virtualPath.StartsWith(this.AbsolutePrefix, System.StringComparison.OrdinalIgnoreCase);
        }

        public override bool FileExists(string virtualPath)
        {
            return this.IsAzurePath(virtualPath) || base.Previous.FileExists(virtualPath);
        }

        public bool FileExists(string virtualPath, NameValueCollection queryString)
        {
            return this.IsAzurePath(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var virtualFile = this.GetAzureFile(virtualPath);
            if (virtualFile != null)
            {
                return virtualFile;
            }
            return base.Previous.GetFile(virtualPath);
        }

        public AzureProxyFile GetAzureFile(string virtualPath)
        {
            if (this.IsAzurePath(virtualPath))
            {
                string blobName = virtualPath.Substring(this.AbsolutePrefix.Length).Trim(new char[] { '/', '\\' });

                return AzureProxyFile.Create(blobName, this);
            }
            return null;
        }

        public IVirtualFile GetFile(string virtualPath, NameValueCollection queryString)
        {
            return GetAzureFile(virtualPath);
        }
    }
}
