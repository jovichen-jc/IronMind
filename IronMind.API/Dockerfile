FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY IronMind.Core/IronMind.Core.csproj IronMind.Core/
COPY IronMind.Data/IronMind.Data.csproj IronMind.Data/
COPY IronMind.Services/IronMind.Services.csproj IronMind.Services/
COPY IronMind.API/IronMind.API.csproj IronMind.API/
COPY IronMind.Tests/IronMind.Tests.csproj IronMind.Tests/
COPY IronMind.slnx .

COPY . .

RUN dotnet publish IronMind.API/IronMind.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IronMind.API.dll"]
