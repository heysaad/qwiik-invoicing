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

- The API uses REST style resources for `customers` and `invoices`.
- List endpoints return a common paged response shape with `items`, `pageNumber`, `pageSize` and `totalCount`.
- Create endpoints return `201 Created`.
- Update endpoints return `200 OK`.
- Delete endpoints return `204 No Content`.
- Missing records return `404 Not Found`.
- duplicate invoice numbers returns `409 Conflict`.
- Validation failures return `400 Bad Request` using validation problem details.
- TenantId `X-Tenant-Id` header and validated before controller actions by `[ValidateTenant]`.
- Auth uses ASP.NET Identity bearer endpoints, with `/register` creating a user for the current tenant.

| Method | Endpoint | Purpose |
| --- | --- | --- |
| POST | `/register` | create a tenant user |
| GET | `/customers` | list tenant customers with paging |
| GET | `/customers/{id}` | get one tenant customer |
| POST | `/customers` | create customer |
| PUT | `/customers/{id}` | update customer |
| DELETE | `/customers/{id}` | delete customer if no invoices reference it |
| GET | `/invoices` | list tenant invoices with customer and items |
| GET | `/invoices/{id}` | get one tenant invoice |
| POST | `/invoices` | create invoice with line items |
| PUT | `/invoices/{id}` | update invoice and replace line items |
| DELETE | `/invoices/{id}` | delete invoice |

## Validation approach

- use DataAnnotation for primary validation logic.
- use custom validation for complex logic.
- Tenant validation is done via common filter attribute.
- invoice due date must be on or after issue date.
- invoice must have at least one item.
- invoice item description must not be empty.


## Tenant isolation approach

For the assessment purpose, I have used `TenantId` column in every table to map records to tenant. it's the quickest way to implement multi-tenant system. 

for large enterprise level apps, it should support 1 database per tenant.

## Indexing and performance strategy

| Table | Index | Description
| --- | --- | ---
| Common | `TenantId` first in composite indexes | every query is tenant scoped
| Invoices | `TenantId + InvoiceNumber` unique | avoid duplicate invoice numbers inside same tenant
| Invoices | `TenantId + IssueDate` | invoice list is sorted by issue date 
| Customers | `TenantId + Name`, `TenantId + Email` | list/search like queries
| InvoiceItems | `TenantId + InvoiceId` | load invoice items faster

used paging in list endpoints to avoid loading all data at once.

## Testing approach

- For this assessment I focused on keeping the code simple and verifiable by running the API locally.
- Manual testing can be done through Postman.
- Recommended test cases:
  - register user.
  - create, update, list and delete customers inside one tenant.
  - prevent deleting a customer that has invoices.
  - create invoice with valid customer and items.
  - reject invoice with duplicate invoice number inside same tenant.
  - allow same invoice number in different tenants.
  - reject invoice where due date is before issue date.
  - verify user from one tenant cannot access another tenant's data.

## Azure deployment and monitoring considerations

- Use docker image to deploy it to `Azure Container Apps`.
- Use `Azure SQL Database` for db.
- Store connection strings in `Azure Key Vault`.
- Run EF Core migrations during deployment pipeline or through a controlled migration job.
- Enable HTTPS only and configure proper CORS rules if a frontend consumes this API.
- Use Application Insights for request tracing, dependency tracking, exceptions and performance metrics.
- Use Serilog sinks for structured logs and include tenant id in log scope where useful.
- Configure health checks for API and database connectivity.
- Add alerts for high error rate, slow requests, failed database connections and unusual authentication failures.
- For scaling, keep the API stateless and scale out app instances behind Azure-managed load balancing.

## Security considerations

- used ASP.NET Identity with bearer token authentication.
- password hashing is handled by Identity.
- use HTTPS.
- tenant header is validated using common `[ValidateTenant]`. filter also checks whether user belongs to that tenant.
- all customer and invoice queries include `TenantId` condition to avoid cross tenant data access.
- connection string from user secrets / environment variables, not hardcoded config.
- invoice totals are calculated server side instead of trusting client-provided total.
- controllers protected with `[Authorize]` to allow only logged in users to access.
- don't use `[ValidateTenant]` attribute for tenant scope endpoints. eg. auth/register, forgot password etc

## Known limitations

1. simple authentication is implemented. in future we can extend it to RBAC.
2. current implementation has 1 user per tenant, in future we can extend it to teams.
3. tenant detection is currently done with `X-Tenant-Id` header. we can use slug based or domain based detection.
4. invoice status workflow is basic and does not check for backward transitions.
5. invoice delete is hard delete. production systems may prefer soft delete.
6. no audit trail.
7. there are no automated tests yet.

## What you would improve with more time

- add full integration test.
- move business rules from controllers to services as the project grows.
- add RBAC.
- add feature to keep history of invoice changes (audio logs).
- use soft delete.
- invoice PDF generation and email delivery.
- event driven architecture using `Azure Service Bus` or `RabbitMQ`. to generate PDF, Reports, Email etc
- payment tracking and status transitions.
- add idempotency key to handle client retries safely.
- background jobs for overdue invoice reminders.