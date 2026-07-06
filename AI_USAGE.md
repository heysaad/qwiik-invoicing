Tool used: Codex with Visual Studio Code

## Things generated with AI

### 1. setup users and authentication

it created a quick prototype with basic auth flow

Review and Correction:
- AI generated whole code in `Program.cs`. I asked it to move to AuthController
- It assumed tenant id as request param, I changed the logic to read from header

### 2. Asked to scafffold invoice controller

Review and Corrections:
- Ai generated only inline class mappings. I asked it to use Mapster to map VMs
- Improve db models manually
- Db structure did not have line items
- It put Customer details in Invoice table directly eg. CustomerName, CustomerEmail. I asked it to use separate table for customer and use FK.

### 3. frontend

complete UI and vibe coded. following is the prompt use.

```
build the frontend in vuejs to demostrate this api flow. put code under [frontend](frontend/) folder.

1. a register account screen
2. CRUD customers
3. CRUD invoices
4. api need `X-Tenant-Id` as header. so make sure you always pass it after login
5. use daisyUI with tailwindcss.
6. use default theme css.
7. make better UX flow. eg. create customer, delete confirmation in modal. 
8. create invoice will have it's own page
9. UI must be clean, compact, modern.
```

Review and Corrections:
- UI fixes, 
- forms redesign, 
- layout corrections