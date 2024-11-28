## Prerequisites

1. Install [.NET SDK](https://dotnet.microsoft.com/download) (version matching the project requirements).
2. Ensure the database server (e.g., SQL Server) is set up and running.
3. Verify that the connection string is properly configured in the `appsettings.json` file of the API project (`DrugWarehouseManagement.API`).

---

## Migration Commands

### 1. Navigate to the Repository Directory
```bash
cd DrugWarehouseManagement.Repository
```
### 2. Add a Migration
To create a new migration, run the following command:
```bash
dotnet ef migrations add <MigrationName> --startup-project ../DrugWarehouseManagement.API
dotnet ef migrations add v2 --startup-project ../DrugWarehouseManagement.API
```
Replace `<MigrationName>` with a descriptive name for the migration (e.g., `InitialCreate`, `AddNewTable`, etc.).

### 3. Update the Database
Apply the migrations to the database using this command:
```bash
dotnet ef database update --startup-project ../DrugWarehouseManagement.API
```

---

## Troubleshooting

1. **Command not found**: Ensure the `dotnet-ef` tool is installed globally.
   ```bash
   dotnet tool install --global dotnet-ef
   ```
2. **Database connection error**: Verify that the database server is running and reachable.

---

## Resources

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core Command Line Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
```

You can adjust the details in the **Prerequisites** and **Notes** sections to fit your project's specifics.
