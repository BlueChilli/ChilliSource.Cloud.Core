using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
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
        Task<string> SaveAsync(StorageCommand command, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Deletes a file from the remote storage.
        /// </summary>
        /// <param name="fileToDelete">The remote file path to be deleted</param>
        void Delete(string fileToDelete);

        /// <summary>
        ///     Deletes a file from the remote storage.
        /// </summary>
        /// <param name="fileToDelete">The remote file path to be deleted</param>
        Task DeleteAsync(string fileToDelete, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        Stream GetContent(string fileName);

        /// <summary>
        /// Retrieves an encrypted file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="encryptionKeys">(Optional) Specifies encryption keys if the file is encrypted.</param>
        Stream GetContent(string fileName, StorageEncryptionKeys encryptionKeys);

        /// <summary>
        /// Retrieves an encrypted file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="encryptionKeys">(Optional) Specifies encryption keys if the file is encrypted.</param>
        /// <param name="contentType">Outputs the content type.</param>
        /// <returns>The file content.</returns>
        Stream GetContent(string fileName, StorageEncryptionKeys encryptionKeys, out string contentType);

        /// <summary>
        /// Retrieves an encrypted file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="encryptionKeys">(Optional) Specifies encryption keys if the file is encrypted.</param>
        /// <param name="contentLength">Outputs the content length.</param>
        /// <param name="contentType">Outputs the content type.</param>
        /// <returns>The file content.</returns>        
        Stream GetContent(string fileName, StorageEncryptionKeys encryptionKeys, out long contentLength, out string contentType);

        /// <summary>
        /// Retrieves a connected file stream from the remote storage.
        /// The Caller should dispose the Stream object.
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="encryptionKeys">(Optional) Specifies encryption keys if the file is encrypted.</param>
        /// <param name="contentLength">Outputs the content length.</param>
        /// <param name="contentType">Outputs the content type.</param>
        /// <returns>The file content.</returns>
        Stream GetStreamedContent(string fileName, StorageEncryptionKeys encryptionKeys, out long contentLength, out string contentType);

        /// <summary>
        /// Retrieves a file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <returns>The file content.</returns>
        Task<FileStorageResponse> GetContentAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves an encrypted file from the remote storage
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="encryptionKeys">(Optional) Specifies encryption keys if the file is encrypted.</param>
        /// <returns>The file content.</returns>
        Task<FileStorageResponse> GetContentAsync(string fileName, StorageEncryptionKeys encryptionKeys, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves a connected file stream from the remote storage.
        /// The Caller should dispose the response.Stream object.
        /// </summary>
        /// <param name="fileName">Remote file path</param>
        /// <param name="encryptionKeys">(Optional) Specifies encryption keys if the file is encrypted.</param>
        /// <returns>The file content.</returns>
        Task<FileStorageResponse> GetStreamedContentAsync(string fileName, StorageEncryptionKeys encryptionKeys, CancellationToken cancellationToken = default(CancellationToken));

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
        Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));

        string GetPreSignedUrl(string fileName, TimeSpan expiresIn);
    }
}
