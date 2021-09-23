using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Options to support saving objects to a remote storage
    /// </summary>
    public class StorageCommand
    {
        internal IFileStorageSourceProvider SourceProvider { get; private set; }

        public StorageCommand SetSourceProvider(IFileStorageSourceProvider sourceProvider)
        {
            this.SourceProvider = sourceProvider;
            return this;
        }

        /// <summary>
        /// Destination folder in storage for file to be stored into. If no folder specified, stored in the root of the bucket/container.
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// File must be stored with an extension. If source doesn't contain an extension, this must be set. 
        /// Extension should include seperator eg ".jpg"
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// If no filename specified, a new Guid will be used as the filename. Due to potential clashes when storing this is prefered behaviour.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Options to encrypt the file before storing. When downloading the file this setting must be passed as well. Leave it null to disable encryption
        /// </summary>
        public StorageEncryptionOptions EncryptionOptions { get; set; }

        public string ContentType { get; set; }

        public static IFileStorageSourceProvider CreateSourceProvider(Func<CancellationToken, Task<Stream>> streamFactory, bool autoDispose)
        {
            return new StreamFileStorageSource(streamFactory, autoDispose);
        }

        private class StreamFileStorageSource : IFileStorageSourceProvider
        {
            private Func<CancellationToken, Task<Stream>> _streamFactory;

            public StreamFileStorageSource(Func<CancellationToken, Task<Stream>> streamFactory, bool autoDispose)
            {
                this._streamFactory = streamFactory;
                this.AutoDispose = autoDispose;
            }

            public bool AutoDispose { get; private set; }
            public Task<Stream> GetStreamAsync(CancellationToken cancellationToken) { return _streamFactory(cancellationToken); }
        }
    }

    public class StorageEncryptionOptions
    {
        private StorageEncryptionKeys _defaultKeys;

        public StorageEncryptionOptions() { }
        public StorageEncryptionOptions(string secret, string salt)
        {
            _defaultKeys = new StorageEncryptionKeys(secret, salt);
        }

        public virtual StorageEncryptionKeys GetKeys()
        {
            return _defaultKeys;
        }
    }

    public class StorageEncryptionKeys
    {
        public StorageEncryptionKeys(string secret, string salt)
        {
            if (String.IsNullOrEmpty(secret) || String.IsNullOrEmpty(salt))
                throw new ArgumentException("secret/salt are invalid");

            Secret = secret;
            Salt = salt;
        }

        public string Secret { get; private set; }
        public string Salt { get; private set; }
    }

    public interface IFileStorageSourceProvider
    {
        bool AutoDispose { get; }
        Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
