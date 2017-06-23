using ChilliSource.Cloud.Core.Compression;
using ChilliSource.Cloud.Core.Images;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class ImageExtensionTests
    {
        public static string GetTestDataFolder()
        {
            string startupPath = Environment.CurrentDirectory;
            var pathItems = startupPath.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(x => string.Equals("bin", x));
            string projectPath = String.Join(Path.DirectorySeparatorChar.ToString(), pathItems.Take(pathItems.Length - pos - 1));
            return Path.Combine(projectPath, "Data");
        }

        [Fact]
        public void GetImageFormat_FromFilename_ShouldReturnImageFormat()
        {
            var filename1 = "myimage.jpg";
            Assert.Equal(ImageFormat.Jpeg, filename1.GetImageFormat());

            var filename2 = "myimage.BMP";
            Assert.Equal(ImageFormat.Bmp, filename2.GetImageFormat());

            var filename3 = "myimage.tif";
            Assert.Equal(ImageFormat.Tiff, filename3.GetImageFormat());

            var filename4 = "myimage";
            Assert.Throws<ArgumentException>(() => filename4.GetImageFormat());

            var filename5 = "myimage.abc";
            Assert.Throws<NotImplementedException>(() => filename5.GetImageFormat());

            var filename6 = "myimage.abc.ico";
            Assert.Equal(ImageFormat.Icon, filename6.GetImageFormat());

            var filename7 = ".gif";
            Assert.Equal(ImageFormat.Gif, filename7.GetImageFormat());

        }

        [Fact]
        public void GetImageExtension_FromImageFormat_ReturnsExtension()
        {
            Assert.Equal(".gif", ImageFormat.Gif.FileExtension());
            Assert.Equal(".tif", ImageFormat.Tiff.FileExtension());
            Assert.Equal(".jpg", ImageFormat.Jpeg.FileExtension());
        }

        [Fact]
        public void GetMimeType_FromImage_ReturnsMimeType()
        {
            var path = GetTestDataFolder();
            var image1 = Image.FromFile($"{path}/bitmap1.bmp");
            var image2 = Image.FromFile($"{path}/png1.png");

            Assert.Equal("image/bmp", image1.GetMimeType());
            Assert.Equal("image/png", image2.GetMimeType());
        }

        [Fact]
        public void GetMostRepresentedColor_FromImage_ReturnsColor()
        {
            var path = GetTestDataFolder();
            var image = Image.FromFile($"{path}/bitmap2.bmp");

            Assert.Equal("#02AEEE", image.GetMostRepresentedColor().ToHexString());
        }

        [Fact]
        public void ImageToByteArray_AndBackToImage_Works()
        {
            var path = GetTestDataFolder();
            var image = Image.FromFile($"{path}/bitmap1.bmp");

            var bytes = image.ToStream(ImageFormat.Bmp).ToArray();
            var image2 = bytes.ToImage();

            Assert.Equal(image.PhysicalDimension, image2.PhysicalDimension);
            Assert.Equal(image.GetMimeType(), image2.GetMimeType());
        }

        [Fact]
        public void CropToRatio_ResizesImageBy_NewRatio()
        {
            var path = GetTestDataFolder();
            var image = Image.FromFile($"{path}/bitmap2.bmp");

            var image2 = image.CropToRatio(1, 1);
            var image3 = image.CropToRatio(2, 1);
            var image4 = image.CropToRatio(5, 3);

            Assert.Equal(image2.Size.Width, 60);
            Assert.Equal(image2.Size.Height, 60);

            Assert.Equal(image3.Size.Width, 120);
            Assert.Equal(image3.Size.Height, 60);

            Assert.Equal(image4.Size.Width, 100);
            Assert.Equal(image4.Size.Height, 60);
        }


    }
}
