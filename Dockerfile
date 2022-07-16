FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Test & Build
RUN cd WsdlDownload.Cli; dotnet restore && dotnet test && dotnet publish -c Release

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN adduser dotnet
USER dotnet
WORKDIR /home/dotnet/app
COPY --from=build-env /app/WsdlDownload.Cli/bin/Release/net6.0/ .

ENTRYPOINT ["dotnet", "WsdlDownload.Cli.dll"]