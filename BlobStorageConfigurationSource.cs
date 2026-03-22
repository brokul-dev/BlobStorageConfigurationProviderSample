using Azure.Core;

namespace BlobStorageConfigurationProviderSample;

public sealed class BlobStorageConfigurationSource(
    string account,
    string container,
    string blobName,
    TokenCredential credential,
    ILogger logger) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder _) =>
        new BlobStorageConfigurationProvider(
            account, container, blobName, credential, logger);
}