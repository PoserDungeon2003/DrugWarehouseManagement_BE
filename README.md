# **Project Architecture Documentation**

This document provides an overview of the project structure and describes the purpose of each layer. The architecture follows a modular, scalable, and maintainable design pattern, with a clear separation of concerns.

---

## **Table of Contents**
1. [Project Overview](#project-overview)
2. [Folder Structure](#folder-structure)
   - [Repository Layer](#repository-layer)
   - [Service Layer](#service-layer)
   - [Common Layer](#common-layer)
   - [API Layer](#api-layer)
3. [Key Components](#key-components)
   - [Unit of Work](#unit-of-work)
   - [Dependency Injection](#dependency-injection)

---

## **Project Overview**
This project is built using a multi-layered architecture to ensure clear separation of responsibilities and ease of maintenance. The project is divided into four main layers:
- **Repository Layer**: Handles database operations.
- **Service Layer**: Contains business logic.
- **Common Layer**: Holds shared constants, enumerations, and utilities.
- **API Layer**: Serves as the entry point for external clients through HTTP requests.

---

## **Folder Structure**

### **1. Repository Layer** (`DrugWarehouseManagement.Repository.csproj`)
Responsible for interacting with the database and providing a data abstraction layer.

```plaintext
Repository/
├── Migrations/        # Database migration files (e.g., initial schema, updates)
├── Models/            # Database entity classes
├── Interface/         # Contracts for repositories
├── Repositories/      # Repository implementations
├── IUnitOfWork.cs     # Interface defining transactional boundaries
└── UnitOfWork.cs      # Implementation of the Unit of Work pattern
```

- **Purpose**:
  - Define and implement data access logic.
  - Ensure transactional consistency using the **Unit of Work** pattern.
  - Encapsulate database interactions to keep other layers decoupled.

---

### **2. Service Layer** (`DrugWarehouseManagement.Service.csproj`)
Encapsulates business logic and facilitates communication between the API and Repository layers.

```plaintext
Service/
├── Interface/         # Contracts for services
├── Services/          # Implementations of service logic
└── DTO/               # Data Transfer Objects
    ├── Request/       # Request DTOs (input data)
    └── Response/      # Response DTOs (output data)
```

- **Purpose**:
  - Implement application-specific logic.
  - Process and validate data received from the API layer.
  - Use DTOs to transfer data between layers.

---

### **3. Common Layer** (`DrugWarehouseManagement.Common.csproj`)
Provides shared resources and utilities used across the project.

```plaintext
Common/
├── Consts/            # Constants used throughout the application
└── Enum/              # Enumerations for standardizing values
```

- **Purpose**:
  - Centralize reusable components.
  - Maintain consistency in values and utilities.

---

### **4. API Layer** (`DrugWarehouseManagement.API.csproj`)
Acts as the entry point for the application, handling client requests and returning appropriate responses.

```plaintext
API/
├── Controllers/       # API endpoints (e.g., UserController, ProductController)
├── Middleware/        # Custom middleware (e.g., exception handling, logging)
│   ├── Program.cs     # Application startup file
│   └── ServiceRegister.cs # Dependency Injection container setup
```

- **Purpose**:
  - Route client requests to appropriate services.
  - Ensure request validation and response formatting.
  - Manage middleware for cross-cutting concerns like logging and error handling.

---

## **Key Components**

### **1. Unit of Work**
- Centralized management of multiple repositories to ensure transactional consistency.
- Implements the `IUnitOfWork` interface to manage database operations.

**Example:**
```csharp
public interface IUnitOfWork
{
    Task SaveChangesAsync();
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
    IAccountRepository AccountRepository { get; }
}
```

### **2. Dependency Injection**
- Configured in `ServiceRegister.cs` to ensure all dependencies are injected dynamically during runtime.

**Example:**
```csharp
public static class ServiceRegister
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<TokenHandlerService>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountService, AccountService>();
    }
}
```

---

## **Naming Conventions**
- **Interfaces**: Prefixed with `I`, followed by the model name and layer (e.g., `IAccountRepository`, `IAccountService`).
- **Classes**: Named with the model name and layer (e.g., `AccountRepository`, `AccountService`, `AccountController`).
- **Endpoints**: Always start with `/api/[Model]/[Function]` (e.g., `/api/Account/Login`).

---

## **Coding Principles**
Follow these steps to add a new feature or functionality:

### **Step 1: Repository Layer**
1. Create an interface in the `Interface` folder that inherits from `IGenericRepository<T>`.
   ```csharp
   public interface IAccountRepository : IGenericRepository<Account>
   {
   }
   ```

2. Implement the interface in the `Repositories` folder, inheriting from `GenericRepository<T>` and the newly created interface.
   ```csharp
   public class AccountRepository : GenericRepository<Account>, IAccountRepository
   {
       public AccountRepository(DrugWarehouseContext context) : base(context)
       {
       }
   }
   ```

3. Add new functions directly in the interface and implement them in the repository.

---

### **Step 2: Unit of Work**
1. Add the repository interface to `IUnitOfWork.cs`:
   ```csharp
   IAccountRepository AccountRepository { get; }
   ```

2. Implement it in `UnitOfWork.cs`:
   ```csharp
   public IAccountRepository AccountRepository { get; private set; }

   public UnitOfWork(DrugWarehouseContext context)
   {
       _context = context;
       AccountRepository ??= new AccountRepository(_context);
   }
   ```

---

### **Step 3: Service Layer**
1. Create a new interface in the `Interface` folder for the service:
   ```csharp
   public interface IAccountService
   {
       Task<AccountLoginResponse> LoginWithEmail(AccountLoginRequest request);
   }
   ```

2. Implement the service in the `Services` folder:
   ```csharp
   public class AccountService : IAccountService
   {
       private readonly IUnitOfWork _unitOfWork;
       private readonly IPasswordHasher<string> _passwordHasher;
       private readonly TokenHandlerService _tokenHandler;

       public AccountService(IUnitOfWork unitOfWork, TokenHandlerService tokenHandler)
       {
           _unitOfWork = unitOfWork;
           _passwordHasher = new PasswordHasher<string>();
           _tokenHandler = tokenHandler;
       }
   }
   ```

- **Important**: Inject dependencies using interfaces (`IUnitOfWork`), not concrete classes.
- **Business Logic**: Handle all business rules in the service layer. Do not implement business logic in the repository layer.

---

### **Step 4: Dependency Injection**
Register the service and repository in `ServiceRegister.cs`:
```csharp
services.AddScoped<IAccountRepository, AccountRepository>();
services.AddScoped<IAccountService, AccountService>();
```

Group related services and repositories together to improve readability and avoid conflicts.

---

### **Step 5: Controller Creation**
1. Add a new controller:
   - Right-click the `Controllers` folder.
   - Select `Add` > `Controller`.
   - Choose the `API` tab and select **Option 2** to generate the controller.

2. Follow naming conventions for endpoints:
   - **Format**: `/api/[Model]/[Function]`
   - Example:
     ```csharp
     [ApiController]
     [Route("api/[controller]")]
     public class AccountController : ControllerBase
     {
         private readonly IAccountService _accountService;

         public AccountController(IAccountService accountService)
         {
             _accountService = accountService;
         }
     }
     ```


---

This documentation provides a high-level overview of the project structure and key components. If you have questions or need further clarification, feel free to reach out. 😊
