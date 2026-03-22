using Azure.Core;

namespace BlobStorageConfigurationProviderSample;

public sealed class BlobStorageConfigurationSource(
    string account,
    string container,
    string blobName,
    TokenCredential credential,
    ILoggerFactory loggerFactory) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder _) =>
        new BlobStorageConfigurationProvider(
            account,
            container,
            blobName,
            credential,
            loggerFactory.CreateLogger<BlobStorageConfigurationProvider>());
}