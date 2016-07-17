using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CloudImages
{
    public enum PublishingStatus
    {
        Unknown,
        Resizing,
        Publishing,
        Cleaning,
        Finished
    }

    public class CloudHandler
    {
        public static async Task ResizeAndPublishImagesAsync(string album, IEnumerable<string> images, IProgress<PublishingStatus> progressIndicator)
        {
            await Task.Run(() =>
            {
                progressIndicator.Report(PublishingStatus.Resizing);
                var resizedImages = ImageHandler.ResizeImages(images);

                progressIndicator.Report(PublishingStatus.Publishing);
                PublishImages(album, resizedImages);

                progressIndicator.Report(PublishingStatus.Cleaning);
                FileHandler.DeleteFiles(resizedImages);

                progressIndicator.Report(PublishingStatus.Finished);
            });
        }

        private static void PublishImages(string album, IEnumerable<string> images)
        {
            var container = GetBlobContainer();

            if (!container.Exists())
                container.Create(BlobContainerPublicAccessType.Blob);

            var directory = container.GetDirectoryReference(album);

            var i = GetNextBlobNumber(directory);
            foreach (var image in images)
            {
                PublishImage(directory, image, i.ToString("000") + ".jpg");
                i++;
            }
        }

        public static IEnumerable<string> GetAlbumNames()
        {
            return GetBlobContainer()
                .ListBlobs()
                .OfType<CloudBlobDirectory>()
                .Select(dir => dir.Prefix.TrimEnd('/'));
        }

        public static void DeleteAlbum(string album)
        {
            throw new NotImplementedException();
        }

        private static void PublishImage(CloudBlobDirectory album, string image, string blobName)
        {
            var blob = album.GetBlockBlobReference(blobName);
            blob.UploadFromFile(image);
        }

        private static int GetNextBlobNumber(CloudBlobDirectory directory)
        {
            var blobNames = directory.ListBlobs()
                                     .Cast<CloudBlockBlob>()
                                     .Select(blob => blob.Uri.Segments.Last())
                                     .ToList();

            if (!blobNames.Any()) return 1;

            return blobNames.Select(name => int.Parse(name.Remove(name.Length - ".jpg".Length)))
                            .Max() + 1;
        }

        private static CloudBlobContainer GetBlobContainer()
        {
            return new CloudStorageAccount(new StorageCredentials(ApiKeys.AccountName, ApiKeys.Key), true)
                .CreateCloudBlobClient()
                .GetContainerReference(ApiKeys.ContainerName);
        }
    }
}
