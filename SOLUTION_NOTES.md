## How to run the project

1. Run directly
```
cd ./src/Qwiik.Api/
dotnet run 
```

2. use Docker
```
docker compose up -d --build
```

## Your assumptions

- This project is only to demonstrate invoicing basic api structure.
- user authentication flow is not the priority. so added basic auth flow.
- each request will send `X-Tenant-Id` header.
- invoice number should be unique only inside same tenant.
- invoice total is calculated from invoice items and tax.
- payment collection and invoice PDF generation are out of scope.
- one user per tenant is enough for current scope.

## Architecture overview

| Layer | Responsibility |
| --- | --- |
| Controllers | expose customer, invoice and auth endpoints |
| Requests | request and response DTOs for API contract |
| Data/Models | EF Core entities for tenant, customer and invoice data |
| AppDbContext | database access using EF Core |
| Filters | common tenant validation before controller action |
| Mapping | Mapster config for entity to response mapping |

- API is kept simple with controller based structure.
- business validation is mostly inside controllers for this assessment scope.
- tenant validation is centralized in `ValidateTenant` filter.
- database migrations are included for schema creation.

## Domain model explanation

- tenant is the root boundary for business data.
- customer can have multiple invoices.
- invoice total is calculated from items subtotal + tax.
- invoice status is stored using enum for fixed values.

## Database design explanation

| Table | Purpose |
| --- | --- |
| Tenants | store tenant details like name, slug and active status |
| Users | ASP.NET Identity users mapped to tenant |
| Customers | customer details for each tenant |
| Invoices | invoice header details like number, dates, status and totals |
| InvoiceItems | invoice line items with quantity, price and line total |

- every table has `TenantId` for tenant isolation.
- `Invoice` belongs to one `Customer`.
- `Invoice` has many `InvoiceItems`.

## API design explanation

## Validation approach

- use DataAnotation for primary validation logic.
- use custom validation for complex logic
- Tenant validation is done via common filter attribute.

## Tenant isolation approach

For the assessment purpose, I have used `TenantId` column in every table to map records to tenant. it's the quickest way to implement multi-tenant system. 

for large enterprise level apps, it should support 1 database per tenant.

## Indexing and performance strategy

| Table | Index | Description
| --- | --- | ---
| Common | `TenantId` first in composite indexes | every query is tenant scoped
| Invoices | `TenantId + InvoiceNumber` unique | avoid duplicate invoice numbers inside same tenant
| Invoices | `TenantId + IssueDate` | invoice list is sorted by issue date 
| Customers | `TenantId + Name`, `TenantId + Email` | list/search like queies
| InvoiceItems | `TenantId + InvoiceId` | load invoice items faster

used paging in list endpoints to avoid loading all data at once.

## Testing approach

## Azure deployment and monitoring considerations

## Security considerations

- used ASP.NET Identity with bearer token authentication.
- password hashing is handled by Identity.
- use HTTPS.
- tenant header is validated using common `[ValidateTenant]`. filter also checks whether user belongs to that tenant.
- all customer and invoice queries include `TenantId` condition to avoid cross tenant data access.
- connection string from user secrets / environment variables, not hardcoded config.
- controllers protected with `[Authorize]` to allow only logged in users to access.
- don't use `[ValidateTenant]` attribute for tenant scope endpoints. eg. auth/register, forgot password etc

## Known limitations

1. simple authentication is implementated. in future we can extend it to RBAC.
2. current implementation has 1 user per tenant, in future we can extend it to teams.
3. tenant detection is current done with `X-Tenant-Id` header. we can use slug based or domain based detection.

## What you would improve with more time
