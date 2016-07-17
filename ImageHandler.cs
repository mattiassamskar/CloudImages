using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageProcessor;

namespace CloudImages
{
    public class ImageHandler
    {
        public static List<string> ResizeImages(IEnumerable<string> images)
        {
            return images.Select(ResizeImage).ToList();
        }

        public static string ResizeImage(string image)
        {
            var photoBytes = File.ReadAllBytes(image);
            var tempFile = Path.GetTempPath() + Guid.NewGuid() + ".jpg";

            using (var inStream = new MemoryStream(photoBytes))
            using (var outStream = new MemoryStream())
            using (var imageFactory = new ImageFactory())
            using (var fileStream = new FileStream(tempFile, FileMode.CreateNew))
            {
                imageFactory.Load(inStream)
                    .Constrain(new Size(1000, 1000))
                    .AutoRotate()
                    .Quality(100)
                    .Save(outStream);

                outStream.WriteTo(fileStream);
            }
            return tempFile;
        }
    }
}
