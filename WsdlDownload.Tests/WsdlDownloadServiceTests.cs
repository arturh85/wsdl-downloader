using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System.Net.Mime;
using WsdlDownload.Core;

namespace WsdlDownload.Tests;

[UsesVerify]
public class WsdlDownloadServiceTests : IDisposable
{
    private readonly DirectoryInfo testDirectory;

    public WsdlDownloadServiceTests()
    {
        this.testDirectory = new DirectoryInfo(Guid.NewGuid().ToString());
        this.testDirectory.Create();
    }

    [Fact]
    public async Task SmokeTest()
    {
        // load fixtures
        using var rootWsdlFixture = File.OpenRead("Fixtures/root.wsdl");
        using var subXsdFixture = File.OpenRead("Fixtures/sub.xsd");
        using var leaf1XsdFixture = File.OpenRead("Fixtures/leaf1.xsd");
        using var leaf2XsdFixture = File.OpenRead("Fixtures/leaf2.xsd");

        // configure mocks
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://server.example.com:8080/Service?wsdl")
                .Respond(MediaTypeNames.Text.Xml, rootWsdlFixture);
        mockHttp.When("http://server.example.com:8080/Service?xsd=sub.xsd")
                .Respond(MediaTypeNames.Text.Xml, subXsdFixture);
        mockHttp.When("http://server.example.com:8080/Service?xsd=leaf1.xsd")
                .Respond(MediaTypeNames.Text.Xml, leaf1XsdFixture);
        mockHttp.When("http://server.example.com:8080/Service?xsd=leaf2.xsd")
                .Respond(MediaTypeNames.Text.Xml, leaf2XsdFixture);

        // configure services
        var services = new ServiceCollection();
        services.AddSingleton(mockHttp.ToHttpClient());
        services.AddSingleton<WsdlDownloadService>();
        var provider = services.BuildServiceProvider();
        var wsdlService = provider.GetRequiredService<WsdlDownloadService>();

        // run object under test
        await wsdlService.DownloadWsdlRecursive(
            "http://server.example.com:8080/Service?wsdl", 
            Path.Combine(testDirectory.FullName, "bar.wsdl")
        );

        // verify expectations
        testDirectory.EnumerateFiles().Count().Should().Be(1);
        var resultWsdl = File.ReadAllText(Path.Combine(testDirectory.FullName, "bar.wsdl"));
        await Verify(resultWsdl)!;
    }
    
    [Fact]
    public async Task SmokePasswordTest()
    {
        // load fixtures
        using var rootWsdlFixture = File.OpenRead("Fixtures/root.wsdl");

        // configure mocks
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://server.example.com:8080/Service?wsdl")
                .Respond(MediaTypeNames.Text.Xml, rootWsdlFixture);

        // configure services
        var services = new ServiceCollection();
        services.AddSingleton(mockHttp.ToHttpClient());
        services.AddSingleton<WsdlDownloadService>();
        var provider = services.BuildServiceProvider();
        var wsdlService = provider.GetRequiredService<WsdlDownloadService>();

        // run object under test
        await wsdlService.DownloadWsdl(
            "http://server.example.com:8080/Service?wsdl", 
            Path.Combine(testDirectory.FullName, "test.wsdl"),
            "username",
            "password"
        );

        // verify expectations
        testDirectory.EnumerateFiles().Count().Should().Be(1);
        var resultWsdl = File.ReadAllText(Path.Combine(testDirectory.FullName, "test.wsdl"));
        await Verify(resultWsdl)!;
    }
    
    [Fact]
    public async Task SmokeListPasswordTest()
    {
        // load fixtures
        using var rootWsdlFixture = File.OpenRead("Fixtures/root.wsdl");
        var input = testDirectory.Parent + "\\..\\..\\..\\Fixtures\\input.csv";
        var output = testDirectory.FullName + "\\Output";
        Directory.CreateDirectory(output);

        // configure mocks
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://server.example.com:8080/Service?wsdl")
                .Respond(MediaTypeNames.Text.Xml, rootWsdlFixture);

        // configure services
        var services = new ServiceCollection();
        services.AddSingleton(mockHttp.ToHttpClient());
        services.AddSingleton<WsdlDownloadService>();
        var provider = services.BuildServiceProvider();
        var wsdlService = provider.GetRequiredService<WsdlDownloadService>();

        // run object under test
        var lines = await wsdlService.DownloadWsdls(
            input,
            output
        );

        // verify expectations
        new DirectoryInfo(output).EnumerateFiles().Count().Should().Be(lines);
    }

    public void Dispose()
    {
        foreach (var file in testDirectory.EnumerateFiles())
        {
            file.Delete();
        }
        testDirectory.Delete(true);
    }
}