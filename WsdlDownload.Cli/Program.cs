using Cocona;
using Microsoft.Extensions.DependencyInjection;
using WsdlDownload.Core;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<WsdlDownloadService>();

var app = builder.Build();
app.AddCommand(async (
        [Argument(Description = "URL of WSDL or XSD file to download")] string sourceUrl,
        [Argument(Description = "Output path")] string outputPath,
        WsdlDownloadService wsdlDownloadService
    ) =>
{
    try
    {
        await wsdlDownloadService.DownloadWsdlRecursive(sourceUrl, outputPath);
        Console.WriteLine("Finished");
    } catch(Exception e)
    {
        Console.WriteLine(e);
    }
});

app.Run();
