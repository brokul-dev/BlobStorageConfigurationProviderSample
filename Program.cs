using Azure.Identity;
using BlobStorageConfigurationProviderSample;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

builder.Configuration.AddBlobJson(
    account: "myaccount",
    container: "myconfig",
    blobName: "config.json",
    credential: new AzureCliCredential(),
    loggerFactory: app.Services.GetRequiredService<ILoggerFactory>());  

app.MapGet("/", (IConfiguration configuration) =>
{
    var configSection = configuration.GetSection("BlobConfig");

    var config1 = configSection["Config1"];
    var config2 = configSection["Config2"];

    return $"Config1: {config1}, Config2: {config2}";
});

app.Run();
