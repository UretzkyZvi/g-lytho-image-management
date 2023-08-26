using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageManagement.Utilities
{
    public static class ImageUtilities
    {
        private const double BytesInMB = 1048576; // 1024 * 1024

        public static double BytesToMB(long bytes)
        {
            return bytes / BytesInMB;
        }

        public static async Task<ImageInfo> GetMetaDataFromURLAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetByteArrayAsync(url);
                if (response == null || response.Length == 0)
                {
                    return null;
                }

                using (var memoryStream = new System.IO.MemoryStream(response))
                {
                    var imageInfo = Image.Identify(memoryStream);
                    return imageInfo;
                }
            }
        }

        public static async Task<ImageInfo> GetMetaDataFromStreamAsync(Stream stream)
        {
            stream.Position = 0;
            return await Task.FromResult(Image.Identify(stream));
        }
    }
}
