# backend/

Chua scaffold. Theo CLAUDE.md, cau truc du kien:

```
backend/
  JobHunter.API/       # dotnet new webapi, chua Controllers/Services/Repositories/Models/DTOs
  JobHunter.Tests/      # dotnet new xunit
```

Chay khi bat dau Phase 0 (Setup):

```
dotnet new webapi -n JobHunter.API -o backend/JobHunter.API
dotnet new xunit -n JobHunter.Tests -o backend/JobHunter.Tests
dotnet add backend/JobHunter.Tests reference backend/JobHunter.API
```
