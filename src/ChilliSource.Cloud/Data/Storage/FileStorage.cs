using ChilliSource.Cloud.Configuration;
using ChilliSource.Cloud.DataStructures;
using ChilliSource.Cloud.Extensions;
using ChilliSource.Cloud.Infrastructure;
using ChilliSource.Cloud.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data
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

                if (command.EncryptionOptions != null)
                {
                    ValidateKeysProvider();
                    using (var streamToSave = await EncryptedStream.CreateAsync(sourceStream, command.EncryptionOptions.Secret, command.EncryptionOptions.Salt)
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
            if (_encryptionProvider == null)
            {
                throw new ApplicationException("Encryption cannot be used because there's no Key Provider set. See interface IStorageEncryptionProvider.");
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
                    response.Stream = await DecryptedStream.CreateAsync(contentStream, _encryptionProvider.GetSecret(fileName), _encryptionProvider.GetSalt(fileName), contentLength);
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
