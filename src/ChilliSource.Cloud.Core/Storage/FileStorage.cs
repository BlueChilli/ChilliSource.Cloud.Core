using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;
using Humanizer;

namespace ChilliSource.Cloud.Core
{
    internal class FileStorage : IFileStorage
    {
        private IRemoteStorage _storage;

        internal FileStorage(IRemoteStorage storage)
        {
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
                sourceStream = await sourceProvider.GetStreamAsync().IgnoreContext();

                if (String.IsNullOrEmpty(command.FileName))
                {
                    command.FileName = String.Format("{0}{1}", Guid.NewGuid().ToShortGuid(), command.Extension);
                }
                else
                {
                    if (!String.IsNullOrEmpty(command.Extension) && String.IsNullOrEmpty(Path.GetExtension(command.FileName)))
                    {
                        command.FileName = $"{command.FileName}{command.Extension}";
                    }
                }

                if (String.IsNullOrEmpty(command.ContentType))
                {
                    command.ContentType = GlobalConfiguration.Instance.GetMimeMapping().GetMimeType(command.FileName);
                }

                if (!String.IsNullOrEmpty(command.Folder))
                {
                    command.FileName = "{0}/{1}".FormatWith(command.Folder, command.FileName);
                }

                var keys = command.EncryptionOptions?.GetKeys(command.FileName);
                if (keys != null)
                {
                    using (var streamToSave = await EncryptedStream.CreateAsync(sourceStream, keys.Secret, keys.Salt)
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

        public Stream GetContent(string fileName)
        {
            return this.GetContent(fileName, null);
        }

        public Stream GetContent(string fileName, StorageEncryptionKeys encryptionKeys)
        {
            return TaskHelper.GetResultSafeSync(() => this.GetContentAsync(fileName, encryptionKeys)).Stream;
        }

        public Stream GetContent(string fileName, StorageEncryptionKeys encryptionKeys, out string contentType)
        {
            long contentLength;
            return GetContent(fileName, encryptionKeys, out contentLength, out contentType);
        }

        public Stream GetContent(string fileName, StorageEncryptionKeys encryptionKeys, out long contentLength, out string contentType)
        {
            var result = TaskHelper.GetResultSafeSync(() => this.GetContentAsync(fileName, encryptionKeys));
            contentLength = result.ContentLength;
            contentType = result.ContentType;

            return result.Stream;
        }

        /// <summary>
        /// Retrieves a connected file stream from the remote storage.
        /// The Caller should dispose the Stream object.
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">Specifies whether the file needs to be decrypted.</param>
        /// <param name="contentLength">Outputs the content length.</param>
        /// <param name="contentType">Outputs the content type.</param>
        /// <returns>The file content.</returns>
        public Stream GetStreamedContent(string fileName, StorageEncryptionKeys encryptionKeys, out long contentLength, out string contentType)
        {
            var result = TaskHelper.GetResultSafeSync(() => this.GetStreamedContentAsync(fileName, encryptionKeys));
            contentLength = result.ContentLength;
            contentType = result.ContentType;

            return result.Stream;
        }

        public Task<FileStorageResponse> GetContentAsync(string fileName)
        {
            return this.GetContentAsync(fileName, null);
        }

        public async Task<FileStorageResponse> GetContentAsync(string fileName, StorageEncryptionKeys encryptionKeys)
        {
            Stream contentStream = null;
            Stream returnStream = null;
            try
            {
                var response = await _storage.GetContentAsync(fileName)
                                     .IgnoreContext();

                contentStream = response.Stream;
                int contentLength = (int)response.ContentLength;

                if (encryptionKeys != null)
                {
                    response.Stream = await DecryptedStream.CreateAsync(contentStream, encryptionKeys.Secret, encryptionKeys.Salt, contentLength).IgnoreContext();
                }
                else
                {
                    returnStream = contentStream as MemoryStream;

                    if (returnStream == null)
                    {
                        returnStream = new MemoryStream(contentLength);
                        await contentStream.CopyToAsync(returnStream, Math.Min(80 * 1024, contentLength)).IgnoreContext();
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
        /// Retrieves a connected file stream from the remote storage.
        /// The Caller should dispose the response.Stream object.
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="isEncrypted">Option not supported for streamed content yet.</param>
        /// <returns>The file content.</returns>
        public async Task<FileStorageResponse> GetStreamedContentAsync(string fileName, StorageEncryptionKeys encryptionKeys)
        {
            var response = await _storage.GetContentAsync(fileName)
                                    .IgnoreContext();

            if (encryptionKeys != null)
            {
                var stream = response.Stream;

                response.Stream = EncryptionHelper.CreateConnectedStream(EncryptionMode.Decrypt, stream, encryptionKeys.Secret, encryptionKeys.Salt, leaveOpen: false);
            }

            return response;
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
