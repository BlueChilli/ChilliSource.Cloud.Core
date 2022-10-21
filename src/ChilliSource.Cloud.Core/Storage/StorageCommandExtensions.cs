using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;
using System.Threading;

#if NET_4X
using ChilliSource.Cloud.Core.Images;
using System.Drawing.Imaging;
#endif

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace ChilliSource.Cloud.Core
{
    public static class StorageCommandExtensions
    {
        public static StorageCommand SetStreamSource(this StorageCommand command, Stream stream)
        {
            var source = StorageCommand.CreateSourceProvider(async (cancellationToken) => stream, false);

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetStreamSource(this StorageCommand command, Stream stream, string fileExtension)
        {
            var source = StorageCommand.CreateSourceProvider(async (cancellationToken) => stream, false);
            command.Extension = fileExtension;

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetUrlSource(this StorageCommand command, string url)
        {
            var source = StorageCommand.CreateSourceProvider(async (cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var downloaded = await DownloadHelper.GetDataAsync(url); //cancellationToken not supported in WebClient :(
                if (!downloaded.HasOkStatus())
                {
                    throw new ApplicationException($"Error downloading data at '{url}'", downloaded.Exception);
                }
                command.ContentType = command.ContentType.DefaultTo(downloaded.ContentType);
                if (String.IsNullOrEmpty(command.Extension))
                {
                    command.Extension = Path.GetExtension(new Uri(url).AbsolutePath);
                    if (String.IsNullOrEmpty(command.Extension))
                    {
                        command.Extension = GlobalConfiguration.Instance.GetMimeMapping().GetExtension(command.ContentType);
                    }
                }
                return new MemoryStream(downloaded.Data);
            }, true);

            return command.SetSourceProvider(source);
        }

#if NET_4X
        public static StorageCommand SetImageSource(this StorageCommand command, Image image)
        {
            var source = StorageCommand.CreateSourceProvider(async (CancellationToken) =>
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

            return command.SetSourceProvider(source);
        }
#endif

        public static StorageCommand SetByteArraySource(this StorageCommand command, byte[] byteArray)
        {
            var source = StorageCommand.CreateSourceProvider(async (cancellationToken) => new MemoryStream(byteArray), true);

            return command.SetSourceProvider(source);
        }

        public static StorageCommand SetFilePathSource(this StorageCommand command, string filePath)
        {
            var source = StorageCommand.CreateSourceProvider(async (cancellationToken) => new FileStream(filePath, FileMode.Open, FileAccess.Read), true);

            return command.SetSourceProvider(source);
        }
    }
}
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously