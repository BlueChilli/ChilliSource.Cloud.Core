using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    /// <summary>
    /// Extension methods for System.Drawing.Image.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Converts System.Drawing.Image to System.IO.MemoryStream.
        /// </summary>
        /// <param name="image">An System.Drawing.Image to convert.</param>
        /// <returns>A System.IO.MemoryStream.</returns>
        public static MemoryStream ToStream(this Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Converts System.Drawing.Image to System.IO.MemoryStream with specified image format.
        /// </summary>
        /// <param name="image">An System.Drawing.Image to convert.</param>
        /// <param name="format">An System.Drawing.Imaging.ImageFormat that specifies the format.</param>
        /// <returns>A System.IO.MemoryStream.</returns>
        public static MemoryStream ToStream(this Image image, ImageFormat format)
        {
            var ms = new MemoryStream();
            image.Save(ms, format);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Gets a string that contains the image's Multipurpose Internet Mail Extensions (MIME) type.
        /// </summary>
        /// <param name="image">An System.Drawing.Image.</param>
        /// <returns>A string that contains the image's Multipurpose Internet Mail Extensions (MIME) type.</returns>
        public static string GetMimeType(this Image image)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == image.RawFormat.Guid)
                    return codec.MimeType;
            }

            return "image/unknown";
        }

        /// <summary>
        /// Converts byte array to System.Drawing.Image.
        /// </summary>
        /// <param name="data">A byte array to convert.</param>
        /// <returns>An System.Drawing.Image.</returns>
        public static Image ToImage(this byte[] data)
        {
            //keep memory stream open!
            return Image.FromStream(new MemoryStream(data));
        }

        /// <summary>
        /// Gets image file extension based on the image format.
        /// </summary>
        /// <param name="format">An System.Drawing.Imaging.ImageFormat that specifies the format.</param>
        /// <returns>Image file extension.</returns>
        public static string FileExtension(this ImageFormat format)
        {
            try
            {
                return ImageCodecInfo.GetImageEncoders()
                        .First(x => x.FormatID == format.Guid)
                        .FilenameExtension
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .First()
                        .Trim('*')
                        .ToLower();
            }
            catch (Exception)
            {
                return "." + format.ToString().ToLower();
            }
        }

        /// <summary>
        /// Parse a filename to return a ImageFormat type eg file.png returns ImageFormat.Png
        /// </summary>
        /// <param name="fileName">Filename or extension to parse</param>
        /// <returns>ImageFormat</returns>
        public static ImageFormat GetImageFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException(
                    string.Format("Unable to determine file extension for fileName: {0}", fileName));

            switch (extension.ToLower())
            {
                case @".bmp":
                    return ImageFormat.Bmp;

                case @".gif":
                    return ImageFormat.Gif;

                case @".ico":
                    return ImageFormat.Icon;

                case @".jpg":
                case @".jpeg":
                    return ImageFormat.Jpeg;

                case @".png":
                    return ImageFormat.Png;

                case @".tif":
                case @".tiff":
                    return ImageFormat.Tiff;

                case @".wmf":
                    return ImageFormat.Wmf;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Crops the System.Drawing.Image to specified width and height ratio.
        /// </summary>
        /// <param name="originalImage">An System.Drawing.Image to process.</param>
        /// <param name="widthRatio">The specified width ratio.</param>
        /// <param name="heightRatio">The specified height ratio.</param>
        /// <returns>An System.Drawing.Image.</returns>
        public static Image CropToRatio(this Image originalImage, int widthRatio, int heightRatio)
        {
            float newRatio = (float)widthRatio / (float)heightRatio;
            float oldRatio = (float)originalImage.Width / (float)originalImage.Height;

            int newWidth = newRatio < oldRatio ? Convert.ToInt32(originalImage.Height * newRatio) : originalImage.Width;
            int newHeight = newRatio < oldRatio ? originalImage.Height : Convert.ToInt32(originalImage.Width / newRatio);


            // create a blank image to paint the new picture onto and set its resolution
            Bitmap newImageCanvas = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
            newImageCanvas.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

            // find the location to draw the resized image onto the new canvas
            int oldImageX = newRatio < oldRatio ? (originalImage.Width - newWidth) / 2 : 0;
            int oldImageY = newRatio < oldRatio ? 0 : (originalImage.Height - newHeight) / 2;

            // create the new image
            using (Graphics g = Graphics.FromImage(newImageCanvas))
            {
                // set the image background to white
                g.Clear(System.Drawing.Color.White);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // draw the rescaled original image onto the new canvas
                g.DrawImage(originalImage,
                    new Rectangle(0, 0, newWidth, newHeight),
                    new Rectangle(oldImageX, oldImageY, newWidth, newHeight),
                    GraphicsUnit.Pixel);
            }

            // return the reulsting scaled image on its background canvas
            return newImageCanvas;
        }

        /// <summary>
        /// Gets the most represented color in the image.
        /// </summary>
        /// <param name="image">An System.Drawing.Image.</param>
        /// <param name="maxDictionarySize">The maximum number of colors to check.</param>
        /// <returns>A System.Drawing.Color.</returns>
        public static Color GetMostRepresentedColor(this Image image, int maxDictionarySize = 1000)
        {
            using (Bitmap bmp = new Bitmap(image))
            {
                var colors = new Dictionary<int, int>();
                int colorsFound = 0;
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        int key = bmp.GetPixel(x, y).ToArgb();
                        if (colors.ContainsKey(key)) colors[key]++;
                        else
                        {
                            colorsFound++;
                            if (colorsFound < maxDictionarySize) colors.Add(key, 1);
                        }
                    }
                }
                int max = 0, maxIndex = 0;
                foreach (var key in colors.Keys)
                {
                    if (colors[key] > max)
                    {
                        max = colors[key];
                        maxIndex = key;
                    }
                }
                return Color.FromArgb(maxIndex);
            }
        }

        /// <summary>
        /// Overlay an image onto another image with both images centered.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="secondImage"></param>
        /// <returns></returns>
        public static Bitmap MergeImage(this Image image, Image secondImage)
        {
            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }

            int outputImageWidth = image.Width > secondImage.Width ? image.Width : secondImage.Width;

            int outputImageHeight = image.Height > secondImage.Height ? image.Height : secondImage.Height;

            Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(image,
                    new Rectangle(new Point((outputImageWidth - image.Width) / 2, (outputImageHeight - image.Height) / 2), image.Size),
                    new Rectangle(new Point(), image.Size), GraphicsUnit.Pixel);
                graphics.DrawImage(secondImage,
                    new Rectangle(new Point((outputImageWidth - secondImage.Width) / 2, (outputImageHeight - secondImage.Height) / 2), secondImage.Size),
                    new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);
            }

            return outputImage;
        }

    }
}
