# Qwiik Invoicing

Qwiik Invoicing is a small multi-tenant invoicing app built with a .NET 9 Web API, Entity Framework Core, SQL Server, and a Nuxt 4 frontend. It includes customer management, invoice CRUD, tenant-scoped data access, basic authentication, and seeded demo data for local review.

## Tech stack

| Area | Technology |
| --- | --- |
| API | ASP.NET Core 9, Controllers, ASP.NET Identity bearer auth |
| Data | Entity Framework Core, SQL Server |
| Logging | Serilog |
| Frontend | Nuxt 4, Vue 3, TypeScript |
| UI | Tailwind CSS 4, daisyUI |
| Local services | Docker Compose, SQL Server 2022 |

## Prerequisites

- .NET 9 SDK
- Node.js 22 or later
- Docker Desktop, if running SQL Server or the API through Docker

## Live deployment

| App | URL |
| --- | --- |
| Frontend | https://qwiik-invoicing.vercel.app |
| Backend | https://qwiik-invoicing.onrender.com |


| Field | Value |
| --- | --- |
| Email | `demo@qwiik.local` |
| Password | `Demo123!` |

## Quick start with Docker

From the repository root:

```powershell
docker compose up -d --build
```

This starts:

- API: `http://localhost:5156`
- SQL Server: `localhost,1433`

The API applies EF Core migrations and seeds demo data on startup.

Then run the frontend:

```powershell
cd frontend
npm install
npm run dev
```

Open the Nuxt app at the URL printed by the dev server, usually `http://localhost:3000`.

## Run locally without Docker API

Start SQL Server locally and make sure the API connection string is valid in `src/Qwiik.Api/appsettings.Development.json`.

Run the API:

```powershell
cd src/Qwiik.Api
dotnet run
```

Run the frontend in another terminal:

```powershell
cd frontend
npm install
npm run dev
```

By default the frontend proxies API calls from `/api/*` to `http://localhost:5156`.

To point the frontend at a different API origin:

```powershell
npm run dev
```

## Demo login

The startup seed creates a demo tenant and user:

| Field | Value |
| --- | --- |
| Email | `demo@qwiik.local` |
| Password | `Demo123!` |

After login, the frontend fetches the tenant and stores the session in browser local storage.

## API overview

Development OpenAPI metadata is available at:

```text
GET /openapi/v1.json
```

Authenticated customer and invoice requests must include:

```text
Authorization: Bearer <access-token>
X-Tenant-Id: <tenant-id>
```

## Project structure

```text
.
+-- frontend/                 Nuxt frontend application
|   +-- pages/                Route pages
+-- src/Qwiik.Api/            ASP.NET Core API
|   +-- Controllers/          Auth, customers, and invoices endpoints
|   +-- Data/                 EF Core DbContext, models, and seed data
|   +-- Filters/              Tenant validation filter
|   +-- Mapping/              Mapster configuration
|   +-- Migrations/           EF Core migrations
|   +-- Requests/             Request and response DTOs
+-- docker-compose.yml        API and SQL Server services
+-- SOLUTION_NOTES.md         Architecture and assessment notes
+-- AI_USAGE.md               Notes about AI-assisted development
```

## Useful commands

```powershell
# Build API
dotnet build Qwiik.slnx

# Run API
dotnet run --project src/Qwiik.Api

# Install frontend dependencies
npm install --prefix frontend

# Run frontend dev server
npm run dev --prefix frontend

# Build frontend
npm run build --prefix frontend

# Stop Docker services
docker compose down
```

## Notes and limitations

- Authentication is intentionally simple for the assessment scope.
- Tenant selection currently uses the `X-Tenant-Id` header.
- Invoice deletion is a hard delete.
- There is no audit trail, PDF generation, email delivery, or payment collection.
- Automated tests are not included yet.
