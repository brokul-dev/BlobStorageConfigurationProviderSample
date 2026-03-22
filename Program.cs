using Azure.Identity;
using BlobStorageConfigurationProviderSample;

var builder = WebApplication.CreateBuilder(args);

// TODO: replace with your own bootstrap logger 
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger("Bootstrap");

builder.Configuration.AddBlobJson(
    account: "stbrokuldev",
    container: "myconfig",
    blobName: "config.json",
    credential: new AzureCliCredential(),
    logger: logger);

var app = builder.Build();

app.MapGet("/", (IConfiguration configuration) =>
{
    var configSection = configuration.GetSection("BlobConfig");

    var config1 = configSection["config1"];
    var config2 = configSection["config2"];

    return $"config1: {config1}, config2: {config2}";
});

app.Run();