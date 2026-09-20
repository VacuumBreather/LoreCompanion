using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;
using Microsoft.Extensions.Caching.Memory;

namespace LoreCompanion.Views.Helpers
{
    public class ImageLoader
    {
        private static readonly HttpClient HttpClient = new();

        private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions
        {
            SizeLimit = 1024,
        });

        private readonly string _diskPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(LoreCompanion),
            "ImageCache");

        public ImageLoader()
        {
            Directory.CreateDirectory(_diskPath);
        }

        public async Task<byte[]?> GetImageAsync(string targetUrl, CancellationToken token)
        {
            try
            {
                if (_memoryCache.TryGetValue(targetUrl, out byte[]? data))
                {
                    return data;
                }

                var path = GetPath(targetUrl);

                if (File.Exists(path))
                {
                    data = await File.ReadAllBytesAsync(path, token);

                    _memoryCache.Set(targetUrl, data);

                    return data;
                };

                if (string.IsNullOrWhiteSpace(targetUrl) || !Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
                {
                    return null;
                }

                if (uri.Scheme is "http" or "https")
                {
                    data = await HttpClient.GetByteArrayAsync(uri, token);
                }

                if (data is null) return null;

                _memoryCache.Set(uri, data);

                // Don't make displaying the image wait for the disk-write.
                _ = WriteDiskAsync(path, data, token);

                return data;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        private string GetPath(string key)
        {
            var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key)));

            return Path.Combine(_diskPath, hash);
        }

        private static async Task WriteDiskAsync(string path, byte[] data, CancellationToken token)
        {
            var temp = path + ".tmp";

            try
            {
                await File.WriteAllBytesAsync(temp, data, token);

                File.Move(temp, path, overwrite: true);
            }
            catch
            {
                // Cache writes are generally best-effort.
                try
                {
                    File.Delete(temp);
                }
                catch
                {
                    // Ignore errors deleting the temporary file.
                }
            }
        }
    }
}