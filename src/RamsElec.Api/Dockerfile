FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/RamsElec.Shared/RamsElec.Shared.csproj src/RamsElec.Shared/
COPY src/RamsElec.Api/RamsElec.Api.csproj src/RamsElec.Api/
RUN dotnet restore src/RamsElec.Api/RamsElec.Api.csproj

COPY src/RamsElec.Shared/ src/RamsElec.Shared/
COPY src/RamsElec.Api/ src/RamsElec.Api/
RUN dotnet publish src/RamsElec.Api/RamsElec.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RamsElec.Api.dll"]
