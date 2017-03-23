using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using ChilliSource.Cloud.Infrastructure;
using ChilliSource.Cloud.Security;
using ChilliSource.Cloud.Extensions;
using ChilliSource.Cloud.DataStructures;
using ChilliSource.Cloud.Configuration;

namespace ChilliSource.Cloud.Data
{
    public interface IFileStorage
    {
        /// <summary>
        /// Save a file from various sources to the remote storage
        /// </summary>
        /// <param name="command">Options for the saving the file</param>
        /// <returns>name of file as stored in the remote storage</returns>
        string Save(StorageCommand command);

        /// <summary>
        /// Save a file from various sources to the remote storage
        /// </summary>
        /// <param name="command">Options for the saving the file</param>
        /// <returns>name of file as stored in the remote storage</returns>
        Task<string> SaveAsync(StorageCommand command);

        /// <summary>
        ///     Deletes a file from the remote storage.
        /// </summary>
        /// <param name="fileToDelete">The remote file path to be deleted</param>
        void Delete(string fileToDelete);

        /// <summary>
        ///     Deletes a file from the remote storage.
        /// </summary>
        /// <param name="fileToDelete">The remote file path to be deleted</param>
        Task DeleteAsync(string fileToDelete);

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">(Optional) Specifies whether the file needs to be decrypted.</param>
        Stream GetContent(string fileName, bool isEncrypted = false);

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">Specifies whether the file needs to be decrypted.</param>
        /// <param name="contentType">Outputs the content type.</param>
        /// <returns>The file content.</returns>
        Stream GetContent(string fileName, bool isEncrypted, out string contentType);

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">Specifies whether the file needs to be decrypted.</param>
        /// <returns>The file content.</returns>
        Task<FileStorageResponse> GetContentAsync(string fileName, bool isEncrypted = false);

        /// <summary>
        ///     Checks whether a file exists in the remote storage
        /// </summary>
        /// <param name="fileName">A remote file path</param>
        /// <returns>Returns whether the file exists or not.</returns>
        bool Exists(string fileName);

        /// <summary>
        ///     Checks whether a file exists in the remote storage
        /// </summary>
        /// <param name="fileName">A remote file path</param>
        /// <returns>Returns whether the file exists or not.</returns>
        Task<bool> ExistsAsync(string fileName);
    }

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
        /// Option to encrypt the file before storing. When downloading the file this setting must be passed as well.
        /// </summary>
        public bool Encrypt { get; set; }

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

    public interface IFileStorageSourceProvider
    {
        bool AutoDispose { get; }
        Task<Stream> GetStreamAsync();
    }

    public interface IStorageEncryptionKeys
    {
        string GetSecret(string fileName);
        string GetSalt(string fileName);
    }

    internal class FileStorage : IFileStorage
    {
        private IRemoteStorage _storage;
        private IStorageEncryptionKeys _encryptionKeys;

        internal FileStorage(IRemoteStorage storage, IStorageEncryptionKeys encryptionKeys = null)
        {
            _encryptionKeys = encryptionKeys;
            _storage = storage;
        }

        /// <summary>
        /// Save a file from various sources to the remote storage
        /// </summary>
        /// <param name="command">Options for the saving the file</param>
        /// <returns>name of file as stored in the remote storage</returns>
        public string Save(StorageCommand command)
        {
            return TaskHelper.GetResultSafeSync(() => SaveAsync(command));
        }

        /// <summary>
        /// Save a file from various sources to the remote storage
        /// </summary>
        /// <param name="command">Options for the saving the file</param>
        /// <returns>name of file as stored in the remote storage</returns>
        public async Task<string> SaveAsync(StorageCommand command)
        {
            var sourceProvider = command.SourceProvider;
            Stream sourceStream = null;

            try
            {
                sourceStream = await sourceProvider.GetStreamAsync();

                if (String.IsNullOrEmpty(command.ContentType))
                {
                    command.ContentType = GlobalConfiguration.Instance.GetMimeMapping().GetMimeType(command.FileName);
                }

                if (String.IsNullOrEmpty(command.FileName))
                {
                    command.FileName = String.Format("{0}{1}", Guid.NewGuid().ToShortGuid(), command.Extension);
                }

                if (!String.IsNullOrEmpty(command.Folder))
                {
                    command.FileName = "{0}/{1}".FormatWith(command.Folder, command.FileName);
                }

                if (command.Encrypt)
                {
                    ValidateKeysProvider();
                    using (var streamToSave = await EncryptedStream.CreateAsync(sourceStream, _encryptionKeys.GetSecret(command.FileName), _encryptionKeys.GetSalt(command.FileName))
                                                                 .IgnoreContext())
                    {
                        await _storage.SaveAsync(streamToSave, command.FileName, command.ContentType)
                              .IgnoreContext();
                    }
                }
                else
                {
                    await _storage.SaveAsync(sourceStream, command.FileName, command.ContentType)
                          .IgnoreContext();
                }
            }
            finally
            {
                if (sourceStream != null && sourceProvider.AutoDispose)
                {
                    sourceStream.Dispose();
                }
            }

            return command.FileName;
        }

        private void ValidateKeysProvider()
        {
            if (_encryptionKeys == null)
            {
                throw new ApplicationException("Encryption cannot be used because there's no Key Provider set. See interface IStorageEncryptionKeys.");
            }
        }

        /// <summary>
        ///     Deletes a file from the remote storage.
        /// </summary>
        /// <param name="fileToDelete">The remote file path to be deleted</param>
        public void Delete(string fileToDelete)
        {
            TaskHelper.WaitSafeSync(() => DeleteAsync(fileToDelete));
        }

        /// <summary>
        ///     Deletes a file from the remote storage.
        /// </summary>
        /// <param name="fileToDelete">The remote file path to be deleted</param>
        public async Task DeleteAsync(string fileToDelete)
        {
            if (String.IsNullOrEmpty(fileToDelete)) return;

            await _storage.DeleteAsync(fileToDelete)
                 .IgnoreContext();
        }

        ///// <summary>
        ///// Retrieves a file from the remote storage and writes it to output stream determining mime type from source filename
        ///// <param name="filename">File name or key for a file in the storage</param>
        ///// <param name="attachmentFilename">File name end user will see. This is made file name safe if not already</param>
        ///// <param name="isEncrypted">(Optional) Specifies whether the file needs to be decrypted.</param>
        ///// <returns>File stream result</returns>
        ///// </summary>
        //public FileStreamResult WriteAttachmentContent(HttpResponse response, string filename, string attachmentFilename = "", bool isEncrypted = false)
        //{
        //    return TaskHelper.GetResultSafeSync(() => WriteAttachmentContentAsync(response, filename, attachmentFilename, isEncrypted));
        //}

        ///// <summary>
        ///// Retrieves a file from the remote storage and writes it to output stream determining mime type from source filename
        ///// <param name="filename">File name or key for a file in the storage</param>
        ///// <param name="attachmentFilename">File name end user will see. This is made file name safe if not already</param>
        ///// <param name="isEncrypted">(Optional) Specifies whether the file needs to be decrypted.</param>
        ///// <returns>File stream result</returns>
        ///// </summary>
        //public async Task<FileStreamResult> WriteAttachmentContentAsync(HttpResponse response, string filename, string attachmentFilename = "", bool isEncrypted = false)
        //{
        //    attachmentFilename = attachmentFilename.DefaultTo(filename).ToFileName();
        //    if (!Path.HasExtension(attachmentFilename)) attachmentFilename = attachmentFilename + Path.GetExtension(filename);
        //    response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", attachmentFilename));

        //    var result = await this.GetContentAsync(filename, isEncrypted)
        //                          .IgnoreContext();
        //    Stream stream = result.Stream;

        //    var contentType = String.IsNullOrEmpty(result.ContentType) ? MimeMapping.GetMimeMapping(filename) : result.ContentType;
        //    return new FileStreamResult(stream, contentType);
        //}


        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">(Optional) Specifies whether the file needs to be decrypted.</param>
        public Stream GetContent(string fileName, bool isEncrypted = false)
        {
            return TaskHelper.GetResultSafeSync(() => this.GetContentAsync(fileName, isEncrypted)).Stream;
        }

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">Specifies whether the file needs to be decrypted.</param>
        /// <param name="contentType">Outputs the content type.</param>
        /// <returns>The file content.</returns>
        public Stream GetContent(string fileName, bool isEncrypted, out string contentType)
        {
            long contentLength;
            return GetContent(fileName, isEncrypted, out contentLength, out contentType);
        }

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">Specifies whether the file needs to be decrypted.</param>
        /// <param name="contentLength">Outputs the content length.</param>
        /// <param name="contentType">Outputs the content type.</param>
        /// <returns>The file content.</returns>
        public Stream GetContent(string fileName, bool isEncrypted, out long contentLength, out string contentType)
        {
            var result = TaskHelper.GetResultSafeSync(() => this.GetContentAsync(fileName, isEncrypted));
            contentLength = result.ContentLength;
            contentType = result.ContentType;

            return result.Stream;
        }

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">Specifies whether the file needs to be decrypted.</param>
        /// <returns>The file content.</returns>
        public async Task<FileStorageResponse> GetContentAsync(string fileName, bool isEncrypted = false)
        {
            Stream contentStream = null;
            MemoryStream returnStream = null;
            try
            {
                var response = await _storage.GetContentAsync(fileName)
                                     .IgnoreContext();

                contentStream = response.Stream;
                returnStream = contentStream as MemoryStream;
                int contentLength = (int)response.ContentLength;

                if (isEncrypted)
                {
                    ValidateKeysProvider();
                    response.Stream = await DecryptedStream.CreateAsync(contentStream, _encryptionKeys.GetSecret(fileName), _encryptionKeys.GetSalt(fileName), contentLength);
                }
                else
                {
                    if (returnStream == null)
                    {
                        returnStream = new MemoryStream(contentLength);
                        await contentStream.CopyToAsync(returnStream, Math.Min(80 * 1024, contentLength));
                    }

                    returnStream.Position = 0;
                    response.Stream = returnStream;
                }

                return response;
            }
            finally
            {
                //closes the remote stream
                if (contentStream != null && !object.ReferenceEquals(contentStream, returnStream))
                    contentStream.Dispose();
            }
        }

        /// <summary>
        ///     Checks whether a file exists in the remote storage
        /// </summary>
        /// <param name="fileName">A remote file path</param>
        /// <returns>Returns whether the file exists or not.</returns>
        public bool Exists(string fileName)
        {
            return TaskHelper.GetResultSafeSync(() => ExistsAsync(fileName));
        }

        /// <summary>
        ///     Checks whether a file exists in the remote storage
        /// </summary>
        /// <param name="fileName">A remote file path</param>
        /// <returns>Returns whether the file exists or not.</returns>
        public Task<bool> ExistsAsync(string fileName)
        {
            return _storage.ExistsAsync(fileName);
        }
    }
}
