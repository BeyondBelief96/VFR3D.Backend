# VFR3D.Backend

A .NET 8 aviation data platform providing weather information, airport data, airspace boundaries, and flight planning services for VFR (Visual Flight Rules) pilots.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Environment Setup](#2-environment-setup)
  - [3. Docker Setup (Recommended)](#3-docker-setup-recommended)
  - [4. Running Without Docker](#4-running-without-docker)
- [Configuration](#configuration)
- [Database Migrations](#database-migrations)
- [Testing](#testing)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Contributing](#contributing)

## Overview

VFR3D.Backend is the server-side component of the VFR3D aviation platform. It consists of two main applications:

1. **VFR3D.API** - ASP.NET Core Web API serving client applications
2. **VFR3D.Azure.Functions** - Azure Functions app for scheduled data synchronization (cron jobs)

## Features

### Weather Data
- **METAR** - Current weather observations for airports
- **TAF** - Terminal Aerodrome Forecasts
- **PIREP** - Pilot Reports (turbulence, icing, weather)
- **AIRMET/SIGMET** - Aviation weather hazards
- **G-AIRMET** - Graphical AIRMETs

### Airport Information
- Airport search and details (FAA NASR data)
- Runway information with runway ends
- Communication frequencies (ATIS, Tower, Ground, etc.)
- Airport diagrams
- Chart supplements (FAA A/FD)

### Airspace & Navigation
- Airspace boundaries (Class B, C, D, E)
- Special Use Airspace (MOAs, Restricted, Prohibited)
- Obstacle data
- NOTAMs (Notices to Air Missions)

### Flight Planning
- Navigation log calculations (bearing, distance, headings)
- Winds aloft integration
- Magnetic variation calculations
- Aircraft performance profiles
- Weight & Balance calculations

### Aircraft Management
- User aircraft profiles
- Performance profiles
- Aircraft documents storage

## Technology Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 8, ASP.NET Core 8 |
| Database | PostgreSQL 15 with PostGIS |
| ORM | Entity Framework Core 9 |
| Spatial Data | NetTopologySuite |
| Authentication | Auth0 JWT Bearer |
| Cloud Storage | Azure Blob Storage |
| Serverless | Azure Functions (isolated worker) |
| API Docs | NSwag (OpenAPI/Swagger) |
| Testing | xUnit, FluentAssertions, NSubstitute, Testcontainers |
| Containerization | Docker, Docker Compose |

### External Data Sources
- NOAA Aviation Weather API (METAR, TAF, PIREP, AIRMET/SIGMET)
- FAA NASR data (airports, frequencies)
- FAA NMS API (NOTAMs)
- ArcGIS REST services (airspace boundaries)

## Architecture

This solution follows **Clean Architecture** principles:

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation                          │
│  ┌─────────────────────┐    ┌─────────────────────────────┐ │
│  │     VFR3D.API       │    │   VFR3D.Azure.Functions     │ │
│  │   (Controllers)     │    │    (Timer Triggers)         │ │
│  └─────────────────────┘    └─────────────────────────────┘ │
├─────────────────────────────────────────────────────────────┤
│                      Infrastructure                          │
│  ┌─────────────────────────────────────────────────────────┐│
│  │                VFR3D.Infrastructure                      ││
│  │  • Repositories  • Services  • EF Core  • External APIs ││
│  └─────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│                         Domain                               │
│  ┌─────────────────────────────────────────────────────────┐│
│  │                   VFR3D.Domain                           ││
│  │      • Entities  • Value Objects  • Enums  • Exceptions ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/downloads)
- (Optional) [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local) - for running Azure Functions locally
- (Optional) [Azure Storage Emulator/Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite) - for local Azure Storage emulation

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/BeyondBelief96/VFR3D.Backend
cd VFR3D.Backend
```

### 2. Environment Setup

#### Create Environment File

Copy the example environment file and configure your values:

```bash
cp .env.example .env
```

Edit `.env` with your actual values:

```env
# Database password for the PostgreSQL container
DB_PASSWORD=your_secure_password_here

# Azure Blob Storage connection string
# Get this from Azure Portal > Storage Account > Access Keys
CLOUD_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net

# FAA NMS API credentials (for NOTAM data)
# Apply at: https://api.faa.gov/
NMS_CLIENT_ID=your-client-id
NMS_CLIENT_SECRET=your-client-secret
```

#### Required Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `DB_PASSWORD` | PostgreSQL database password | Yes |
| `CLOUD_STORAGE_CONNECTION_STRING` | Azure Blob Storage connection string | Yes |
| `NMS_CLIENT_ID` | FAA NMS API client ID | No* |
| `NMS_CLIENT_SECRET` | FAA NMS API client secret | No* |

*These are only required if you need NOTAM functionality.

### 3. Docker Setup (Recommended)

The recommended way to run the backend locally is using Docker Compose, which sets up both PostgreSQL with PostGIS and the API with hot-reload.

#### Start the Services

```bash
docker-compose -f docker-compose.local.yml up
```

This starts:
- **PostgreSQL with PostGIS** on port `5432`
- **VFR3D API** on port `7014` with hot-reload enabled

The API will automatically:
1. Restore NuGet packages
2. Run database migrations
3. Start with `dotnet watch` for hot-reload

#### Verify the Setup

Once running, verify the API is working:

```bash
# Health check
curl http://localhost:7014/health

# Swagger UI
# Open in browser: http://localhost:7014/swagger
```

#### Stop the Services

```bash
docker-compose -f docker-compose.local.yml down
```

To also remove the database volume (fresh start):

```bash
docker-compose -f docker-compose.local.yml down -v
```

#### Rebuild After Changes

If you modify the Dockerfile or need a fresh build:

```bash
docker-compose -f docker-compose.local.yml up --build
```

### 4. Running Without Docker

If you prefer to run without Docker, you'll need a local PostgreSQL instance with PostGIS.

#### Install PostgreSQL with PostGIS

**Windows (using installer):**
1. Download PostgreSQL from https://www.postgresql.org/download/windows/
2. During installation, use Stack Builder to add PostGIS

**macOS (using Homebrew):**
```bash
brew install postgresql@15 postgis
brew services start postgresql@15
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt install postgresql-15 postgresql-15-postgis-3
```

#### Create the Database

```bash
# Connect to PostgreSQL
psql -U postgres

# Create user and database
CREATE USER vfr3d_development_user WITH PASSWORD 'your_password';
CREATE DATABASE vfr3d_development_database OWNER vfr3d_development_user;

# Connect to the database and enable PostGIS
\c vfr3d_development_database
CREATE EXTENSION postgis;

\q
```

#### Configure User Secrets (Alternative to appsettings)

For sensitive configuration, use .NET User Secrets:

```bash
cd VFR3D.API

# Initialize user secrets
dotnet user-secrets init

# Set database password
dotnet user-secrets set "Database:Password" "your_password"

# Set cloud storage connection string
dotnet user-secrets set "CloudStorage:ConnectionString" "your_connection_string"

# Set NMS API credentials
dotnet user-secrets set "NmsSettings:ClientId" "your_client_id"
dotnet user-secrets set "NmsSettings:ClientSecret" "your_client_secret"
```

#### Run Database Migrations

```bash
dotnet ef database update --project VFR3D.Infrastructure --startup-project VFR3D.API
```

#### Start the API

```bash
cd VFR3D.API
dotnet run
```

The API will be available at `https://localhost:7014` or `http://localhost:5014`.

## Configuration

### appsettings Files

Configuration is loaded hierarchically:

1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific overrides
3. Environment variables - Highest priority
4. User Secrets - Development only (not committed to source control)

### Configuration Sections

#### Database

```json
{
  "Database": {
    "Host": "localhost",
    "Database": "vfr3d_development_database",
    "Username": "vfr3d_development_user",
    "Password": "your_password",
    "Port": 5432
  }
}
```

#### Cloud Storage

```json
{
  "CloudStorage": {
    "ConnectionString": "your_connection_string",
    "UseManagedIdentity": false,
    "ChartSupplementsContainerName": "vfr3d-chart-supplements-centralus-test",
    "AirportDiagramsContainerName": "vfr3d-airport-diagrams-centralus-test"
  }
}
```

#### Auth0 (Authentication)

```json
{
  "Auth0Settings": {
    "Auth0ApiIdentifier": "your_api_identifier",
    "Auth0Domain": "your_tenant.auth0.com",
    "RequireAuthenticationInDevelopment": false
  }
}
```

Setting `RequireAuthenticationInDevelopment` to `false` bypasses authentication in development mode.

#### NMS API (NOTAMs)

```json
{
  "NmsSettings": {
    "BaseUrl": "https://api-staging.cgifederal-aim.com/nmsapi",
    "AuthBaseUrl": "https://api-staging.cgifederal-aim.com",
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret",
    "CacheDurationMinutes": 5,
    "DefaultRouteCorridorRadiusNm": 25,
    "RequestTimeoutSeconds": 30
  }
}
```

### Azure Functions Configuration

For Azure Functions, configuration is in `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_FUNCTIONS_ENVIRONMENT": "Development",
    "DOTNET_ENVIRONMENT": "Development"
  }
}
```

To disable specific functions during development, add:

```json
{
  "Values": {
    "AzureWebJobs.MetarFunction.Disabled": "true",
    "AzureWebJobs.TafFunction.Disabled": "true"
  }
}
```

## Database Migrations

### Add a New Migration

```bash
dotnet ef migrations add MigrationName --project VFR3D.Infrastructure --startup-project VFR3D.API
```

### Update Database

```bash
dotnet ef database update --project VFR3D.Infrastructure --startup-project VFR3D.API
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project VFR3D.Infrastructure --startup-project VFR3D.API
```

### View Migration SQL

```bash
dotnet ef migrations script --project VFR3D.Infrastructure --startup-project VFR3D.API
```

## Testing

### Run All Tests

```bash
dotnet test VFR3D.Tests/VFR3D.Tests.csproj
```

### Run Specific Tests

```bash
# By test name pattern
dotnet test VFR3D.Tests/VFR3D.Tests.csproj --filter "FullyQualifiedName~NavlogServiceTests"

# By category
dotnet test VFR3D.Tests/VFR3D.Tests.csproj --filter "Category=Unit"
```

### Run with Coverage

```bash
dotnet test VFR3D.Tests/VFR3D.Tests.csproj --collect:"XPlat Code Coverage"
```

### Testing Tools

- **xUnit** - Test framework
- **FluentAssertions** - Readable assertions
- **NSubstitute** - Mocking framework
- **Testcontainers.PostgreSql** - Integration tests with real database
- **RichardSzalay.MockHttp** - HTTP client mocking

## API Documentation

When the API is running, Swagger documentation is available at:

- **Swagger UI**: `http://localhost:7014/swagger`
- **OpenAPI JSON**: `http://localhost:7014/swagger/v1/swagger.json`

### Key Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/metar/{icao}` | Get METAR for an airport |
| `GET /api/taf/{icao}` | Get TAF for an airport |
| `GET /api/airport/search?query={term}` | Search airports |
| `GET /api/airport/{id}` | Get airport details |
| `GET /api/airspace/containing?lat={lat}&lon={lon}` | Get airspaces at a point |
| `POST /api/navlog/calculate` | Calculate navigation log |
| `GET /api/notam/airport/{icao}` | Get NOTAMs for an airport |

## Project Structure

```
VFR3D.Backend/
├── VFR3D.Domain/                 # Core domain layer (no dependencies)
│   ├── Entities/                 # Domain entities
│   ├── Enums/                    # Domain enumerations
│   ├── Exceptions/               # Domain exceptions
│   ├── ValueObjects/             # Value objects (immutable)
│   └── Utilities/                # Domain utilities
│
├── VFR3D.Infrastructure/         # Data access and external services
│   ├── Data/
│   │   ├── VFR3DDbContext.cs    # EF Core DbContext
│   │   └── Configurations/       # Entity configurations
│   ├── Migrations/               # EF Core migrations
│   ├── Repositories/             # Data repositories
│   ├── Services/
│   │   ├── Weather/             # METAR, TAF, PIREP services
│   │   ├── Airport/             # Airport, Runway services
│   │   ├── Airspace/            # Airspace services
│   │   ├── FlightPlanning/      # Navlog, Winds services
│   │   ├── CloudStorage/        # Azure Blob Storage
│   │   ├── Notam/               # NOTAM services
│   │   └── Cron/                # Background sync services
│   ├── Dtos/                     # Data transfer objects
│   ├── Mappers/                  # Entity-DTO mappers
│   ├── Interfaces/               # Service interfaces
│   └── Settings/                 # Configuration classes
│
├── VFR3D.API/                    # ASP.NET Core Web API
│   ├── Controllers/              # API controllers
│   ├── Authentication/           # Auth handlers
│   ├── Configuration/            # Service registration
│   ├── Middleware/               # Custom middleware
│   └── Program.cs                # Application entry point
│
├── VFR3D.Azure.Functions/        # Azure Functions
│   ├── Functions/                # Timer-triggered functions
│   └── Program.cs                # Functions host configuration
│
├── VFR3D.Tests/                  # Test project
│   ├── Unit/                     # Unit tests
│   └── Integration/              # Integration tests
│
├── .github/workflows/            # CI/CD pipelines
├── docker-compose.local.yml      # Local development compose
├── Dockerfile.local              # Development Dockerfile
├── .env.example                  # Environment template
└── VFR3D.Backend.sln            # Solution file
```

## Contributing

### Development Workflow

1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/your-feature-name
   ```

2. Make your changes and ensure tests pass:
   ```bash
   dotnet build VFR3D.Backend.sln
   dotnet test VFR3D.Tests/VFR3D.Tests.csproj
   ```

3. Commit with meaningful messages:
   ```bash
   git commit -m "Add feature description"
   ```

4. Push and create a Pull Request to `develop`:
   ```bash
   git push origin feature/your-feature-name
   ```

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Write unit tests for new functionality

### Branch Strategy

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches

## License

[Add your license information here]

## Support

For issues and feature requests, please use the [GitHub Issues](https://github.com/your-org/VFR3D.Backend/issues) page.
