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
- user authentication flow is not the priority. so added basic auth flow

## Architecture overview

## Domain model explanation

## Database design explanation

- Tenants
- Users

## API design explanation

## Validation approach

- use DataAnotation for primary validation logic.
- use custom validation for complex logic
- Tenant validation is done via common filter attribute.

## Tenant isolation approach

For the assessment purpose, I have used `TenantId` column in every table to map records to tenant. it's the quickest way to implement multi-tenant system. 

for large enterprise level apps, it should support 1 database per tenant.

## Indexing and performance strategy

## Testing approach

## Azure deployment and monitoring considerations

## Security considerations

## Known limitations

1. simple authentication is implementated. in future we can extend it to RBAC.
2. current implementation has 1 user per tenant, in future we can extend it to teams.
3. tenant detection is current done with `X-Tenant-Id` header. we can use slug based or domain based detection.

## What you would improve with more time