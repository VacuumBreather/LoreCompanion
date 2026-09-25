using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using LoreCompanion.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace LoreCompanion.Utilities
{
    public sealed class CachedDataLoader : IDisposable, IAsyncDisposable
    {
        private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions { SizeLimit = 1024 });
        private readonly string _diskPath = AppHelper.CacheFolder;

        // Deduplicate simultaneous requests for the same URL
        private readonly ConcurrentDictionary<string, Task<byte[]?>> _inFlightRequests = new();

        // Track active background disk write operations
        private readonly ConcurrentDictionary<Task, Task> _activeDiskWrites = new();

        // Signal cancellation to all background operations upon application shutdown
        private readonly CancellationTokenSource _shutdownCts = new();
        private HttpClient? _httpClient;

        private bool _isDisposed;

        public CachedDataLoader()
        {
            Directory.CreateDirectory(_diskPath);
        }

        private static ILogger Logger { get; } = LogManager.GetLogger();

        private HttpClient HttpClient
        {
            get
            {
                if (_httpClient is null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.Configure();
                }

                return _httpClient;
            }
        }

        public async Task<byte[]?> GetDataAsync(string dataUrl, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(dataUrl))
            {
                return null;
            }

            // Fast path: return memory cache
            if (_memoryCache.TryGetValue(dataUrl, out byte[]? cachedData))
            {
                return cachedData;
            }

            // Obtain or start the shared in-flight download task (tied to application lifetime)
            var downloadTask = _inFlightRequests.GetOrAdd(
                dataUrl,
                url => LoadDataInternalAsync(url, _shutdownCts.Token));

            try
            {
                // Await the shared task with the caller's specific CancellationToken
                return await downloadTask.WaitAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Caller canceled their wait; the background download continues for others
                return null;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            CancelPendingWrites();
            _memoryCache.Dispose();
            _shutdownCts.Dispose();
            _httpClient?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            CancelPendingWrites();

            // Await any remaining disk writes to finish cancelling
            try
            {
                await Task.WhenAll(_activeDiskWrites.Keys);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error during disk write cancellation");

                // Ignore cancellation and file write exceptions during teardown
            }

            _memoryCache.Dispose();
            _shutdownCts.Dispose();
            _httpClient?.Dispose();
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Ignore errors during temp file cleanup
            }
        }

        private void CancelPendingWrites()
        {
            if (!_shutdownCts.IsCancellationRequested)
            {
                _shutdownCts.Cancel();
            }
        }

        private async Task<byte[]?> LoadDataInternalAsync(string targetUrl, CancellationToken token)
        {
            try
            {
                if (_memoryCache.TryGetValue(targetUrl, out byte[]? cachedData))
                {
                    return cachedData;
                }

                var diskFilePath = GetDiskFilePath(targetUrl);

                // Check disk cache in user temp directory
                if (File.Exists(diskFilePath))
                {
                    try
                    {
                        var diskData = await File.ReadAllBytesAsync(diskFilePath, token);
                        _memoryCache.Set(targetUrl, diskData, new MemoryCacheEntryOptions { Size = 1 });

                        return diskData;
                    }
                    catch (Exception e) when (!token.IsCancellationRequested)
                    {
                        // If reading disk cache fails (e.g., file corruption), proceed to download
                        Logger.Error(e, "Error reading disk cache for {Url}", targetUrl);
                    }
                }

                if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri) ||
                    uri.Scheme is not "http" and not "https")
                {
                    return null;
                }

                // Download data over HTTP/HTTPS
                var data = await HttpClient.GetByteArrayAsync(uri, token);

                if (data.Length == 0)
                {
                    return null;
                }

                _memoryCache.Set(targetUrl, data, new MemoryCacheEntryOptions { Size = 1 });

                // Queue background disk write linked to the shutdown cancellation
                QueueBackgroundDiskWrite(diskFilePath, data);

                return data;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error downloading data from {Url}", targetUrl);

                return null;
            }
            finally
            {
                // Always remove from in-flight requests once finished
                _inFlightRequests.TryRemove(targetUrl, out var _);
            }
        }

        private string GetDiskFilePath(string key)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

            return Path.Combine(_diskPath, hash);
        }

        private void QueueBackgroundDiskWrite(string path, byte[] data)
        {
            if (_isDisposed || _shutdownCts.IsCancellationRequested)
            {
                return;
            }

            // Link shutdown token to the background write
            var shutdownToken = _shutdownCts.Token;

            var writeTask = Task.Run(
                async () =>
                {
                    var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";

                    try
                    {
                        await File.WriteAllBytesAsync(tempPath, data, shutdownToken);
                        File.Move(tempPath, path, true);
                    }
                    catch (OperationCanceledException)
                    {
                        // Clean up temp file on shutdown cancellation
                        TryDeleteFile(tempPath);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Error writing to disk");
                        TryDeleteFile(tempPath);
                    }
                },
                shutdownToken);

            _activeDiskWrites.TryAdd(writeTask, writeTask);

            // Remove task from tracking once completed
            _ = writeTask.ContinueWith(t => _activeDiskWrites.TryRemove(t, out var _), TaskScheduler.Default);
        }
    }
}