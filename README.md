# WSDL downloader

The goal of this cli app is to automate wsdl download process.

At the moment two commands are available:

## WSDL downloader from csv
This command allows you to download a number of wsdl files in a specific folder from a list stored in a csv

```csv
http://server.example.com:8080/Service?wsdl;;;;
http://server.example.com:8080/Service?wsdl;username;password;test_1;
http://server.example.com:8080/Service?wsdl;username;password;test_2;
```
Each line of the csv must contain the url. It also support credentials and filename but those are optional.

### 🐳 Running via Docker

To download a `.wsdl` or `.xsd` file with docker run:

```bash
docker run -v $(pwd):/out arturh85/wsdl-downloader --csvInputFile "test.csv" --outputFolderPath /out/Service.wsdl
```

## Recursive WSDL downloader
This command recursivly downloads a `WSDL` or `XSD` file with `xsd:include` / `xsd:import` tags like this:

```xml
<?xml version='1.0' encoding='UTF-8'?>
<definitions xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:tns="http://gateway.example.com/"
             xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" xmlns="http://schemas.xmlsoap.org/wsdl/"
             name="Service" targetNamespace="http://gateway.example.com/">
	<types>
		<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:tns="http://gateway.example.com/"
					xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" xmlns="http://schemas.xmlsoap.org/wsdl/">

			<!-- this import will be followed & replaced with its contents -->
			<xsd:import namespace="http://gateway.example.com/"
						schemaLocation="http://server.example.com:8080/Service?xsd=example-service.xsd"/>

		</xsd:schema>
	</types>
    ...
```

It replaces all imports/includes with their contents and writes a  single `.wsdl` / `.xsd` file to make it compatible with `SOAP` clients which do not support them. 

### 🐳 Running via Docker

To download a `.wsdl` or `.xsd` file by using the recursive command with docker run:

```bash
docker run -v $(pwd):/out arturh85/wsdl-downloader --sourceUrl "http://server.example.com:8080/Service?wsdl" --outputPath /out/Service.wsdl
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
| `dotnet build`        | Builds the project                                             |
| `dotnet test`         | Run tests                                                      |

## 👀 Want to learn more?

This tool is written in [C#](https://docs.microsoft.com/en-US/dotnet/csharp/tour-of-csharp/) using [dotnet 6](https://docs.microsoft.com/en-US/dotnet/fundamentals/)
- [Minimal API](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Cocona CLI](https://github.com/mayuki/Cocona)
- [XmlDocument Docs](https://docs.microsoft.com/de-de/dotnet/api/system.xml.xmldocument)