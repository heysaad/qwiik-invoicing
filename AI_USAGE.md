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
- 