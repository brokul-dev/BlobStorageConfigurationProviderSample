using Azure.Identity;
using BlobStorageConfigurationProviderSample;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

builder.Configuration.AddBlobJson(
    account: "stbrokuldev",
    container: "myconfig",
    blobName: "config.json",
    credential: new AzureCliCredential(),
    loggerFactory: app.Services.GetRequiredService<ILoggerFactory>());

app.MapGet("/", (IConfiguration configuration) =>
{
    var configSection = configuration.GetSection("BlobConfig");

    var config1 = configSection["config1"];
    var config2 = configSection["config2"];

    return $"config1: {config1}, config2: {config2}";
});

app.Run();