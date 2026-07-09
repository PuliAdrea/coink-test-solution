# Design Patterns - Coink User Management API

This document describes the design patterns identified in the solution and how they are implemented.

---

## 📋 Summary of Identified Patterns

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Clean Architecture** | Overall Structure | Separation of concerns through layered architecture |
| **Repository Pattern** | Infrastructure Layer | Data access abstraction |
| **Dependency Injection** | API Layer | Inversion of Control |
| **Service Layer Pattern** | Application Layer | Centralized business logic |
| **DTO Pattern** | Application Layer | Data transfer between layers |
| **Middleware Pattern** | API Layer | Request processing |
| **Wrapper/Response Pattern** | Application Layer | Standardized API responses |
| **Exception Handling Pattern** | Domain Layer | Centralized error handling |
| **Interface Segregation** | Domain Layer | Specific contracts |
| **Strategy Pattern** | API Layer | Exception handling strategy |
| **Builder Pattern** | API Layer | Application configuration |

---

## 🏗️ Architectural Patterns

### 1. Clean Architecture

**Description:** Organizes the application into independent layers with dependencies pointing inward.

**Implementation:**

```
UserManagement.Domain (Core - No Dependencies)
    ↑
UserManagement.Application (Depends on Domain)
    ↑
UserManagement.Infrastructure (Depends on Domain)
    ↑
UserManagement.API (Depends on all layers)
```

**Code Evidence:**
- Each project references only its inner layers.
- Interfaces are defined in the Domain layer.
- Implementations reside in the Infrastructure layer.
- Business logic is contained within the Application layer.

**Benefits:**
- Framework independence
- Testability
- Maintainability
- Flexibility to replace implementations

---

## 🔄 Creational Patterns

### 2. Builder Pattern

**Description:** Builds complex objects step by step.

**Implementation:** `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();
```

**Purpose:** Configure the application in a fluent and readable way.

---

## 🎯 Structural Patterns

### 3. Repository Pattern

**Description:** Abstracts data access logic by providing an object-oriented interface.

**Implementation:**

**Interface** (`IUserRepository` - Domain Layer):

```csharp
public interface IUserRepository
{
    Task<int> RegisterUserAsync(User user);
    Task<IEnumerable<dynamic>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}
```

**Implementation** (`UserRepository` - Infrastructure Layer):

```csharp
public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    // Implementation using Dapper and PostgreSQL
}
```

**Benefits:**
- Decouples business logic from data access.
- Simplifies unit testing through mocking.
- Allows persistence implementations to be replaced.
- Centralizes data access logic.

**Usage:**

```csharp
// Registration in Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Usage in the Service Layer
private readonly IUserRepository _userRepository;
```

---

### 4. Dependency Injection (DI)

**Description:** Implements inversion of control by injecting dependencies instead of creating them directly.

**Implementation:** `Program.cs`

```csharp
// Service registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

**Usage in Classes:**

```csharp
// Service Layer
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
}

// Controller
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
}
```

**Service Lifetimes:**
- `AddScoped`: One instance per HTTP request (Repository, Service)
- `AddSingleton`: One instance for the application's lifetime (framework services)
- `AddTransient`: A new instance every time it is requested (not used in this project)

**Benefits:**
- Loose coupling
- Testability
- Maintainability
- Flexibility

---

### 5. DTO Pattern (Data Transfer Object)

**Description:** Transfers data between layers without exposing business logic.

**Implementation:**

**Input DTO** (`CreateUserDto`):

```csharp
public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int MunicipalityId { get; set; }
}
```

**Output DTO** (`UserResponseDto`):

```csharp
public record UserResponseDto(
    int Id,
    string Name,
    string Phone,
    string Address,
    string MunicipalityName,
    string DepartmentName,
    string CountryName
);
```

**Usage:**
- `CreateUserDto`: Receives data from the client (API Layer).
- `UserResponseDto`: Returns data to the client (API Layer).
- `User` (Entity): Used internally (Domain Layer).

**Benefits:**
- Decouples layers
- Improves security by hiding internal entities
- Supports calculated fields
- Simplifies API versioning

---

### 6. Wrapper/Response Pattern

**Description:** Encapsulates API responses using a standardized format.

**Implementation:** `ApiResponse<T>`

```csharp
public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
    public T? Data { get; set; }
}
```

**Usage:**

```csharp
// Successful response
return Ok(new ApiResponse<int>(userId, "User created successfully"));

// Error response (Middleware)
var responseModel = new ApiResponse<string>(error.Message)
{
    Succeeded = false
};
```

**Benefits:**
- Consistent API responses
- Improved error handling
- Predictable structure for frontend applications
- Easily extensible

---

## 🎨 Behavioral Patterns

### 7. Service Layer Pattern

**Description:** Encapsulates business logic within a dedicated service layer.

**Implementation:**

**Interface** (`IUserService`):

```csharp
public interface IUserService
{
    Task<int> RegisterUserAsync(CreateUserDto dto);
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task UpdateAsync(int id, CreateUserDto dto);
    Task DeleteAsync(int id);
}
```

**Implementation** (`UserService`):

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public async Task<int> RegisterUserAsync(CreateUserDto dto)
    {
        // Business validation
        // DTO-to-Entity mapping
        // Repository call
        // Result transformation
    }
}
```

**Responsibilities:**
- Business validation
- DTO-to-Entity mapping
- Operation orchestration
- Operational logging

**Benefits:**
- Separation of concerns
- Business logic reuse
- Testability
- Maintainability

---

### 8. Middleware Pattern

**Description:** Uses a processing pipeline to handle HTTP requests and responses.

**Implementation:** `ErrorHandlerMiddleware`

```csharp
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            await HandleExceptionAsync(context, error);
        }
    }
}
```

**Registration** (`Program.cs`):

```csharp
app.UseMiddleware<ErrorHandlerMiddleware>();
```

**Flow:**

```
Request → Middleware → Controller → Service → Repository → Database
                                                                  ↓
Response ← Middleware ← Controller ← Service ← Repository ←──────┘
                ↓
         (Error Handling)
```

**Benefits:**
- Centralized error handling
- Supports cross-cutting concerns (logging, authentication, etc.)
- Reusability
- Separation of concerns

---

### 9. Strategy Pattern (Implicit)

**Description:** Selects different behaviors at runtime.

**Implementation:** Exception handling within `ErrorHandlerMiddleware`

```csharp
switch (error)
{
    case NotFoundException:
        response.StatusCode = (int)HttpStatusCode.NotFound;
        break;

    case ValidationException:
    case ArgumentException:
        response.StatusCode = (int)HttpStatusCode.BadRequest;
        break;

    case KeyNotFoundException:
        response.StatusCode = (int)HttpStatusCode.NotFound;
        break;

    default:
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        responseModel.Message = "Internal Server Error. Please contact support.";
        break;
}
```

**Benefits:**
- Different behavior based on exception type
- Easy extensibility
- Improved maintainability

---

### 10. Exception Handling Pattern

**Description:** Uses structured exception handling with domain-specific exception types.

**Implementation:**

**Custom Exceptions** (Domain Layer):

```csharp
// NotFoundException.cs
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

// ValidationException.cs
public class ValidationException : Exception
{
    // Specific implementation
}
```

**Usage:**

```csharp
// Service Layer
if (user == null)
{
    throw new NotFoundException("User", id);
}

// Middleware
case NotFoundException:
    response.StatusCode = (int)HttpStatusCode.NotFound;
    break;
```

**Benefits:**
- Semantic exception types
- Type-specific error handling
- Better logging and debugging
- Self-documenting code

---

### 11. Interface Segregation Principle (ISP)

**Description:** Interfaces should be specific and should not force implementations to include unnecessary methods.

**Implementation:**

```csharp
public interface IUserRepository
{
    Task<int> RegisterUserAsync(User user);
    Task<IEnumerable<dynamic>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}

public interface IUserService
{
    Task<int> RegisterUserAsync(CreateUserDto dto);
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task UpdateAsync(int id, CreateUserDto dto);
    Task DeleteAsync(int id);
}
```

**Benefits:**
- Small, focused interfaces
- Easier testing
- Better encapsulation
- Compliance with SOLID principles

---

## 📊 Pattern Usage by Layer

### Domain Layer
- **Interface Segregation:** Focused interfaces
- **Exception Handling Pattern:** Domain-specific exceptions

### Application Layer
- **Service Layer Pattern:** Business logic
- **DTO Pattern:** Data transfer
- **Wrapper Pattern:** Standardized responses

### Infrastructure Layer
- **Repository Pattern:** Data access

### API Layer
- **Dependency Injection:** Service registration
- **Middleware Pattern:** Request processing
- **Strategy Pattern:** Exception handling
- **Builder Pattern:** Application configuration

---

## 🔍 Applied SOLID Principles

1. **S**ingle Responsibility: Each class has one responsibility.
2. **O**pen/Closed: Extensible through interfaces.
3. **L**iskov Substitution: Well-defined abstractions.
4. **I**nterface Segregation: Small, focused interfaces.
5. **D**ependency Inversion: Dependencies rely on abstractions rather than implementations.

---

## 📝 Final Notes

This solution demonstrates the practical and effective application of multiple design patterns working together to build an architecture that is:

- **Maintainable:** Easy to understand and modify.
- **Testable:** Each component can be tested independently.
- **Scalable:** Ready to grow with future requirements.
- **Flexible:** Supports changes with minimal impact.
- **Loosely Coupled:** Promotes low coupling between components.

This combination of design patterns follows software engineering best practices and facilitates the long-term maintenance and evolution of the codebase.
```