using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace LoreCompanion.Utilities
{
    public sealed class CachedDataLoader : IDisposable, IAsyncDisposable
    {
        private static readonly HttpClient HttpClient = new();

        private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions { SizeLimit = 1024 });

        private readonly string _diskPath = Path.Combine(Path.GetTempPath(), nameof(LoreCompanion), "Cache");

        // Deduplicate simultaneous requests for the same URL
        private readonly ConcurrentDictionary<string, Task<byte[]?>> _inFlightRequests = new();

        // Track active background disk write operations
        private readonly ConcurrentDictionary<Task, Task> _activeDiskWrites = new();

        // Signal cancellation to all background operations upon application shutdown
        private readonly CancellationTokenSource _shutdownCts = new();

        private bool _isDisposed;

        public CachedDataLoader()
        {
            Directory.CreateDirectory(_diskPath);
        }

        private static ILogger Logger { get; } = LogManager.GetLogger();

        public Task<byte[]?> GetDataAsync(string dataUrl, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(dataUrl))
            {
                return Task.FromResult<byte[]?>(null);
            }

            // Return from memory cache if already loaded
            if (_memoryCache.TryGetValue(dataUrl, out byte[]? cachedData))
            {
                return Task.FromResult(cachedData);
            }

            // Deduplicate concurrent loads for the exact same URL
            return _inFlightRequests.GetOrAdd(dataUrl, url => LoadDataInternalAsync(url, token));
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
                        Logger.Error(e, "Error reading disk cache");

                        // If reading disk cache fails (e.g., file corruption), proceed to download
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
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error downloading data");

                return null;
            }
            finally
            {
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