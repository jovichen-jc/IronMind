FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY IronMind.slnx ./
COPY IronMind.API/IronMind.API.csproj IronMind.API/
COPY IronMind.Core/IronMind.Core.csproj IronMind.Core/
COPY IronMind.Data/IronMind.Data.csproj IronMind.Data/
COPY IronMind.Services/IronMind.Services.csproj IronMind.Services/

RUN dotnet restore IronMind.API/IronMind.API.csproj

COPY . .
RUN dotnet publish IronMind.API/IronMind.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "IronMind.API.dll"]
