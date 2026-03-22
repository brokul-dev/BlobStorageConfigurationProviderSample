using Azure.Core;

namespace BlobStorageConfigurationProviderSample;

public static class BlobStorageConfigurationExtensions
{
    extension(IConfigurationBuilder builder)
    {
        public IConfigurationBuilder AddBlobJson(
            string account,
            string container,
            string blobName,
            TokenCredential credential,
            ILogger logger) =>
                builder.Add(new BlobStorageConfigurationSource(
                    account, container, blobName, credential, logger));
    }
}