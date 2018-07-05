using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core.Images;
using System.Drawing.Imaging;

namespace ChilliSource.Cloud.Core
{
    public static class StorageCommandExtensions
    {
        public static StorageCommand SetStreamSource(this StorageCommand command, Stream stream)
        {
            var source = StorageCommand.CreateSourceProvider(async () => stream, false);

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetStreamSource(this StorageCommand command, Stream stream, string fileExtension)
        {
            var source = StorageCommand.CreateSourceProvider(async () => stream, false);
            command.Extension = fileExtension;

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetUrlSource(this StorageCommand command, string url)
        {
            var source = StorageCommand.CreateSourceProvider(async () =>
            {
                var downloaded = await DownloadHelper.GetDataAsync(url);
                if (!downloaded.HasOkStatus())
                {
                    throw new ApplicationException($"Error downloading data at '{url}'", downloaded.Exception);
                }
                command.ContentType = command.ContentType.DefaultTo(downloaded.ContentType);
                var fileName = $"{Guid.NewGuid().ToShortGuid()}{Path.GetExtension(new Uri(url).AbsolutePath)}";
                command.FileName = command.FileName.DefaultTo(fileName);
                return new MemoryStream(downloaded.Data);
            }, true);

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetImageSource(this StorageCommand command, Image image)
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            var source = StorageCommand.CreateSourceProvider(async () =>
            {
                ImageFormat format = null;
                if (!String.IsNullOrEmpty(command.Extension))
                {
                    format = command.Extension.GetImageFormat();
                }
                else
                {
                    format = image.GetImageFormat();
                    command.Extension = format.FileExtension();
                }
                return image.ToStream(format);
            }, true);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetByteArraySource(this StorageCommand command, byte[] byteArray)
        {
            var source = StorageCommand.CreateSourceProvider(async () => new MemoryStream(byteArray), true);

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetFilePathSource(this StorageCommand command, string filePath)
        {
            var source = StorageCommand.CreateSourceProvider(async () => new FileStream(filePath, FileMode.Open, FileAccess.Read), true);

            return command.SetSourceProvider(source);
        }
    }
}
