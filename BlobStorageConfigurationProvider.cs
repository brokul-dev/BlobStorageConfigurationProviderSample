using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlobStorageConfigurationProviderSample;

public class BlobStorageConfigurationProvider(
    string account, string container, string blobName, TokenCredential credential, ILogger logger) : ConfigurationProvider, IDisposable
{
    private static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromMinutes(1);

    private readonly BlobClient _blob = new BlobContainerClient(
            new Uri($"https://{account}.blob.core.windows.net/{container}"), credential).GetBlobClient(blobName);

    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private PeriodicTimer? _timer;
    private Task? _pollTask;
    private ETag? _etag;
    private int _pollingStarted;
    private int _disposed;

    public override void Load()
    {
        LoadAsync(reload: false, _cts.Token).GetAwaiter().GetResult();
        StartPollingOnce();
    }

    private void StartPollingOnce()
    {
        if (Interlocked.Exchange(ref _pollingStarted, 1) != 0)
        {
            return;
        }

        _timer = new PeriodicTimer(DefaultRefreshInterval);
        _pollTask = PollAsync();
    }

    private async Task PollAsync()
    {
        try
        {
            while (_timer is not null &&
                   await _timer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    await LoadAsync(reload: true, _cts.Token);
                }
                catch (OperationCanceledException) when (_cts.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to refresh configuration from blob '{BlobUri}'. Keeping last known good configuration.",
                        _blob.Uri);
                }
            }
        }
        catch (OperationCanceledException) when (_cts.IsCancellationRequested)
        {
            // Expected during shutdown.
        }
    }

    private async Task LoadAsync(bool reload, CancellationToken ct)
    {
        var entered = false;
        var shouldReload = false;

        try
        {
            await _semaphore.WaitAsync(ct);
            entered = true;

            BlobDownloadResult result;
            try
            {
                var options = new BlobDownloadOptions();

                if (_etag is not null)
                {
                    options.Conditions = new BlobRequestConditions
                    {
                        IfNoneMatch = _etag.Value
                    };
                }

                var response = await _blob.DownloadContentAsync(options, ct);
                result = response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 304)
            {
                return; // Blob unchanged
            }

            await using var stream = result.Content.ToStream();
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            try
            {
                var newData = config
                    .AsEnumerable()
                    .Where(x => x.Value is not null)
                    .ToDictionary(x => x.Key, x => x.Value!, StringComparer.OrdinalIgnoreCase);

                Data = newData;
                _etag = result.Details.ETag;
                shouldReload = reload;
            }
            finally
            {
                (config as IDisposable)?.Dispose();
            }
        }
        finally
        {
            if (entered)
            {
                _semaphore.Release();
            }
        }

        if (shouldReload)
        {
            OnReload();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _cts.Cancel();
        _timer?.Dispose();

        if (_pollTask is not null)
        {
            try
            {
                _pollTask.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested)
            {
                // Expected during shutdown.
            }
        }

        _semaphore.Dispose();
        _cts.Dispose();
    }
}