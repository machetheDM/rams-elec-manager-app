# Rams @Elec Manager App

[![.NET MAUI + API CI](https://github.com/machetheDM/rams-elec-manager-app/actions/workflows/ci.yml/badge.svg)](https://github.com/machetheDM/rams-elec-manager-app/actions/workflows/ci.yml)

A tablet/phone companion app for **Rams @Elec (Pty) Ltd** — a South African electrical &
refrigeration services company. Built with **.NET MAUI** (C#) and **ASP.NET Core**.

## What it does

| Feature | Description |
|---|---|
| **Invoicing** | Create SARS-compliant invoices, generate PDFs, send via SMS/WhatsApp |
| **Job Management** | View and manage jobs synced from the web platform |
| **Analytics** | Dashboard with KPIs and charts mirroring the web portal |
| **Payment Tracking** | Record EFT and FNB SpeedPoint payments |
| **Payment Agent** | AI-powered email monitoring for automatic EFT reconciliation |
| **Offline Support** | SQLite local database with sync engine |

## Architecture

```
┌─────────────────────┐     ┌──────────────────────┐
│  .NET MAUI App      │────▶│  ASP.NET Core API    │
│  (Android/iOS/Win)  │     │  (EF Core → Postgres)│
│                     │     │                      │
│  MVVM + SQLite      │     │  QuestPDF invoices   │
│  LiveCharts2        │     │  JWT auth            │
│  CommunityToolkit   │     │  S3 + Twilio + n8n   │
└─────────────────────┘     └──────────┬───────────┘
                                       │
                            ┌──────────▼───────────┐
                            │    PostgreSQL         │
                            │    (shared with web   │
                            │     platform)         │
                            └──────────────────────┘
```

## Tech Stack

| Layer | Technology |
|---|---|
| Mobile App | .NET MAUI 9 |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Local DB | SQLite (sqlite-net-pcl) |
| Charts | LiveCharts2 (SkiaSharp) |
| PDF | QuestPDF |
| Backend | ASP.NET Core 9 + EF Core 9 |
| Database | PostgreSQL 16 |
| Auth | JWT Bearer |

## Prerequisites

- .NET 9 SDK with MAUI workloads
- PostgreSQL 16 (or Docker)
- Android SDK (for mobile builds)

## Quick Start

```bash
# Start database
docker compose up db -d

# Run API
cd src/RamsElec.Api && dotnet run

# Run MAUI app (Windows)
cd src/RamsElec.App && dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

## Project Structure

```
src/
├── RamsElec.App/       # .NET MAUI mobile/tablet app
├── RamsElec.Api/       # ASP.NET Core Web API
└── RamsElec.Shared/    # Shared models, DTOs, enums
```

## Related

- [Rams @Elec Intelligence Platform](https://github.com/machetheDM/rams-elec-intelligence-platform-deployment) — the web platform this app syncs with
- [Showcase Repository](https://github.com/machetheDM/rams-elec-intelligence-platform) — public portfolio

## License

Proprietary — Rams @Elec (Pty) Ltd
