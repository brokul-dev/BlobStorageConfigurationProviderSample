using Azure.Core;

namespace BlobStorageConfigurationProviderSample;

public static class BlobStorageConfigurationExtensions
{
    public static IConfigurationBuilder AddBlobJson(
        this IConfigurationBuilder builder, string account, string container, string blobName, 
        TokenCredential credential, ILoggerFactory loggerFactory) => 
            builder.Add(new BlobStorageConfigurationSource(
                account, container, blobName, credential, loggerFactory));
}