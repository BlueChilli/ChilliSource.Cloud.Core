using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data
{
    /// <summary>
    /// Options to support saving objects to a remote storage
    /// The following fields override each other if more than one is populated (first listed wins):
    ///     Source_HttpPostedFileBase, Source_ByteArray, Source_Url, Source_Image, Source_Stream and Source_Path.
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

        public static IFileStorageSourceProvider CreateSourceProvider(Func<Task<Stream>> streamFactory, bool autoDispose)
        {
            return new StreamFileStorageSource(streamFactory, autoDispose);
        }

        private class StreamFileStorageSource : IFileStorageSourceProvider
        {
            private Func<Task<Stream>> _streamFactory;

            public StreamFileStorageSource(Func<Task<Stream>> streamFactory, bool autoDispose)
            {
                this._streamFactory = streamFactory;
                this.AutoDispose = autoDispose;
            }

            public bool AutoDispose { get; private set; }
            public async Task<Stream> GetStreamAsync() { return await _streamFactory(); }
        }
    }

    public class StorageEncryptionOptions
    {
        public string Secret { get; set; }
        public string Salt { get; set; }
    }

    public interface IFileStorageSourceProvider
    {
        bool AutoDispose { get; }
        Task<Stream> GetStreamAsync();
    }
}
