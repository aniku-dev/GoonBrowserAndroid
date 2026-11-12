#if ANDROID
using Android.Content;
using Android.OS;
using Android.Provider;

namespace GoonBrowserAndroid.Services.Download
{
    public partial class DownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;

        public DownloadService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // made the ai slop function if statements
        // conform to the coding conventions better
        public async Task DownloadAsync(string url)
        {
            using var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType;

            // Try to get filename from Content-Disposition
            var fileName = response.Content.Headers.ContentDisposition?
                .FileNameStar?
                .Trim('"');

            fileName ??= response.Content.Headers.ContentDisposition?
                .FileName?
                .Trim('"');

            // Fall back to URL filename
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = Path.GetFileName(
                    new Uri(response.RequestMessage?.RequestUri?.ToString() ?? url).LocalPath);
            }

            // Determine extension from MIME type if missing
            if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            {
                var extension = GetExtensionFromMimeType(contentType);

                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".bin";
                }

                fileName = (string.IsNullOrWhiteSpace(fileName)
                    ? Guid.NewGuid().ToString()
                    : fileName) + extension;
            }

            var context = Platform.AppContext;

            var values = new ContentValues();

            values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
            values.Put(
                MediaStore.IMediaColumns.MimeType,
                contentType ?? GetMimeType(fileName));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                values.Put(
                    MediaStore.IMediaColumns.RelativePath,
                    Android.OS.Environment.DirectoryDownloads);
            }

            var uri = context.ContentResolver.Insert(
                MediaStore.Downloads.ExternalContentUri,
                values);

            if (uri == null)
            {
                throw new Exception("Failed to create download");
            }

            try
            {
                await using var input = await response.Content.ReadAsStreamAsync();

                await using var output =
                    context.ContentResolver.OpenOutputStream(uri)
                    ?? throw new Exception("Unable to open output stream");

                await input.CopyToAsync(output);
            }
            catch
            {
                context.ContentResolver.Delete(uri, null, null);
                throw;
            }
        }

        private static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.TrimStart('.');

            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream";
            }


            var mime = Android.Webkit.MimeTypeMap.Singleton?.GetMimeTypeFromExtension(extension.ToLowerInvariant());

            return mime ?? "application/octet-stream";
        }

        private static string? GetExtensionFromMimeType(string? mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
            {
                return null;
            }

            return mimeType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                "image/svg+xml" => ".svg",
                "image/bmp" => ".bmp",
                "image/avif" => ".avif",
                "application/pdf" => ".pdf",
                "text/plain" => ".txt",
                "application/zip" => ".zip",
                "video/mp4" => ".mp4",
                "audio/mpeg" => ".mp3",
                _ => null
            };
        }
    }
}
#endif