using Azure.Identity;
using BlobStorageConfigurationProviderSample;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

builder.Configuration.AddBlobJson(
    account: "myaccount",
    container: "mycontainer",
    blobName: "config.json",
    credential: new AzureCliCredential(),
    loggerFactory: loggerFactory);  

app.MapGet("/", () => "Hello World!");

app.Run();
