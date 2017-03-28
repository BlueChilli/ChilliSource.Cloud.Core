using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChilliSource.Cloud.Web
{
    public static class StorageCommandWebExtensions
    {
        public static StorageCommand SetHttpPostedFileSource(this StorageCommand command, HttpPostedFileBase file)
        {
            var source = StorageCommand.CreateSourceProvider(async () =>
            {
                string fileName = String.Format("{0}{1}", command.FileName.DefaultTo(Guid.NewGuid().ToShortGuid()), command.Extension.DefaultTo(Path.GetExtension(file.FileName)));
                command.FileName = command.FileName.DefaultTo(fileName);
                command.ContentType = command.ContentType.DefaultTo(file.ContentType);

                return await GetFileStreamAsync(file).IgnoreContext();
            }, true);

            return command.SetSourceProvider(source);
        }

        private static async Task<MemoryStream> GetFileStreamAsync(HttpPostedFileBase file)
        {
            int? fileLength = null;
            try
            {
                fileLength = file.ContentLength;
            }
            catch {/* noop */ }

            var memStream = fileLength == null ? new MemoryStream() : new MemoryStream(fileLength.Value);

            var bufferSize = Math.Min(32 * 1024, fileLength ?? 32 * 1024);
            await file.InputStream.CopyToAsync(memStream, bufferSize);
            memStream.Position = 0;
            return memStream;
        }
    }
}
