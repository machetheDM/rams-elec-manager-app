# Deployment Guide

## MAUI App

### Windows (MSIX / unpackaged)
```bash
cd src/RamsElec.App
dotnet publish -f net9.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None
```
For MSIX signing, configure `PackageCertificateThumbprint` in the project file or use `dotnet publish` with a `.pfx`.

### Android (APK)
```bash
cd src/RamsElec.App
dotnet publish -f net9.0-android -c Release -p:AndroidPackageFormat=apk
```
A `keytool`-generated keystore is required for Google Play submission.

## API (Docker)
### Development
```bash
docker compose up -d
```

### Production
Create a `.env` file from `.env.example` and run:
```bash
docker compose -f docker-compose.prod.yml up -d
```

## Environment Variables
See `docker-compose.prod.yml` for the required variables.
