# Recursive WSDL downloader

This utility recursivly downloads a WSDL documents with `xsd:include` tags like this:

```xml
<?xml version='1.0' encoding='UTF-8'?>
<definitions xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:tns="http://gateway.example.com/"
             xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" xmlns="http://schemas.xmlsoap.org/wsdl/"
             name="ibeService" targetNamespace="http://gateway.example.com/">
	<types>
		<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:tns="http://gateway.example.com/"
					xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" xmlns="http://schemas.xmlsoap.org/wsdl/">

			<xsd:import namespace="http://gateway.example.com/"
						schemaLocation="http://server.example.com:8080/Service?xsd=example-service.xsd"/>

		</xsd:schema>
	</types>
    ...
```

And turns it and all the referenced xsd files into a single `.wsdl` file with no `xsd:import`s to make it compatible with `SOAP` clients which do not support them. You can also download .xsd files and merge them into a single file.

## 🐳 Running via Docker

To download a `.wsdl` or `.xsd` file with docker run:

```bash
docker run -v $(pwd):/out arturh85/wsdl-downloader "http://server.example.com:8080/Service?wsdl" /out/Service.wsdl
```

## 🚀 Project Structure

Inside this project you'll see the following important folders and files:

```
/
├── WsdlDownload.Cli/                # Cocona based cli app
├── WsdlDownload.Core/               # Core code
└── WsdlDownload.Tests/              # Test code
│   └── Fixtures/                    # Test fixtures
```

## 🧞 Commands

All commands are run from the root of the project, from a terminal:

| Command               | Action                                                         |
| :-------------------- | :------------------------------------------------------------- |
| `dotnet build`        | Installs [yarn](https://yarnpkg.com/) package manager globally |
| `dotnet test`         | Installs dependencies                                          |

## 👀 Want to learn more?

This tool is written in [C#](https://docs.microsoft.com/en-US/dotnet/csharp/tour-of-csharp/) using [dotnet 6](https://docs.microsoft.com/en-US/dotnet/fundamentals/)
- [Minimal API](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Cocona CLI](https://github.com/mayuki/Cocona)
- [XmlDocument Docs](https://docs.microsoft.com/de-de/dotnet/api/system.xml.xmldocument)