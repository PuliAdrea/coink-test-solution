# Patrones de Diseño - Coink User Management API

Este documento describe los patrones de diseño identificados en la solución y cómo se implementan.

---

## 📋 Resumen de Patrones Identificados

| Patrón | Ubicación | Propósito |
|--------|-----------|-----------|
| **Clean Architecture** | Estructura general | Separación de responsabilidades en capas |
| **Repository Pattern** | Infrastructure Layer | Abstracción del acceso a datos |
| **Dependency Injection** | API Layer | Inversión de control |
| **Service Layer Pattern** | Application Layer | Lógica de negocio centralizada |
| **DTO Pattern** | Application Layer | Transferencia de datos |
| **Middleware Pattern** | API Layer | Procesamiento de requests |
| **Wrapper/Response Pattern** | Application Layer | Respuestas estandarizadas |
| **Exception Handling Pattern** | Domain Layer | Manejo centralizado de errores |
| **Interface Segregation** | Domain Layer | Contratos específicos |
| **Strategy Pattern** | API Layer | Manejo de excepciones |
| **Builder Pattern** | API Layer | Configuración de la aplicación |

---

## 🏗️ Patrones Arquitectónicos

### 1. Clean Architecture

**Descripción**: Separación del código en capas independientes con dependencias dirigidas hacia adentro.

**Implementación**:
```
UserManagement.Domain (Centro - Sin dependencias)
    ↑
UserManagement.Application (Depende de Domain)
    ↑
UserManagement.Infrastructure (Depende de Domain)
    ↑
UserManagement.API (Depende de todos)
```

**Evidencia en el código**:
- Cada proyecto solo referencia las capas internas
- Las interfaces están en Domain
- Las implementaciones están en Infrastructure
- La lógica de negocio está en Application

**Beneficios**:
- Independencia de frameworks
- Testabilidad
- Mantenibilidad
- Flexibilidad para cambiar implementaciones

---

## 🔄 Patrones Creacionales

### 2. Builder Pattern

**Descripción**: Construcción paso a paso de objetos complejos.

**Implementación**: `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();
```

**Propósito**: Configurar la aplicación de manera fluida y legible.

---

## 🎯 Patrones Estructurales

### 3. Repository Pattern

**Descripción**: Abstrae la lógica de acceso a datos proporcionando una interfaz más orientada a objetos.

**Implementación**:

**Interfaz** (`IUserRepository` - Domain Layer):
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

**Implementación** (`UserRepository` - Infrastructure Layer):
```csharp
public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    // Implementación usando Dapper y PostgreSQL
}
```

**Beneficios**:
- Desacoplamiento de la lógica de negocio del acceso a datos
- Facilita el testing (mocking)
- Permite cambiar la implementación de persistencia
- Centraliza la lógica de acceso a datos

**Uso**:
```csharp
// Inyección en Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Uso en Service Layer
private readonly IUserRepository _userRepository;
```

---

### 4. Dependency Injection (DI)

**Descripción**: Inversión de control donde las dependencias se inyectan en lugar de crearse directamente.

**Implementación**: `Program.cs`

```csharp
// Registro de servicios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

**Uso en clases**:

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

**Lifetimes utilizados**:
- `AddScoped`: Una instancia por request HTTP (Repository, Service)
- `AddSingleton`: Una instancia para toda la aplicación (configurado por framework)
- `AddTransient`: Nueva instancia cada vez (no utilizado en este proyecto)

**Beneficios**:
- Desacoplamiento
- Testabilidad (fácil de mockear)
- Mantenibilidad
- Flexibilidad

---

### 5. DTO Pattern (Data Transfer Object)

**Descripción**: Objetos que transportan datos entre capas sin lógica de negocio.

**Implementación**:

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

**Uso**:
- `CreateUserDto`: Recibe datos del cliente (API Layer)
- `UserResponseDto`: Retorna datos al cliente (API Layer)
- `User` (Entity): Usado internamente (Domain Layer)

**Beneficios**:
- Desacoplamiento entre capas
- Seguridad (no expone entidades internas)
- Flexibilidad (puede incluir datos calculados)
- Versionado de API

---

### 6. Wrapper/Response Pattern

**Descripción**: Encapsula respuestas en un formato estándar.

**Implementación**: `ApiResponse<T>`

```csharp
public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
    public T? Data { get; set; }
}
```

**Uso**:
```csharp
// Respuesta exitosa
return Ok(new ApiResponse<int>(userId, "User created successfully"));

// Respuesta de error (en Middleware)
var responseModel = new ApiResponse<string>(error.Message) { Succeeded = false };
```

**Beneficios**:
- Consistencia en respuestas de API
- Mejor manejo de errores
- Facilita el frontend (estructura predecible)
- Extensibilidad (puede agregar más campos)

---

## 🎨 Patrones de Comportamiento

### 7. Service Layer Pattern

**Descripción**: Encapsula la lógica de negocio en una capa de servicios.

**Implementación**:

**Interfaz** (`IUserService`):
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

**Implementación** (`UserService`):
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public async Task<int> RegisterUserAsync(CreateUserDto dto)
    {
        // Validaciones de negocio
        // Mapeo DTO -> Entity
        // Llamada al Repository
        // Transformación de resultados
    }
}
```

**Responsabilidades**:
- Validaciones de negocio
- Transformación entre DTOs y Entidades
- Orquestación de operaciones
- Logging de operaciones

**Beneficios**:
- Separación de responsabilidades
- Reutilización de lógica
- Testabilidad
- Mantenibilidad

---

### 8. Middleware Pattern

**Descripción**: Pipeline de componentes que procesan requests/responses.

**Implementación**: `ErrorHandlerMiddleware`

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

**Registro** (`Program.cs`):
```csharp
app.UseMiddleware<ErrorHandlerMiddleware>();
```

**Flujo**:
```
Request → Middleware → Controller → Service → Repository → Database
                                                                  ↓
Response ← Middleware ← Controller ← Service ← Repository ←──────┘
                ↓
         (Error Handling)
```

**Beneficios**:
- Manejo centralizado de errores
- Cross-cutting concerns (logging, autenticación, etc.)
- Reutilización
- Separación de responsabilidades

---

### 9. Strategy Pattern (Implícito)

**Descripción**: Permite seleccionar algoritmos en tiempo de ejecución.

**Implementación**: Manejo de excepciones en `ErrorHandlerMiddleware`

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

**Beneficios**:
- Comportamiento diferente según el tipo de excepción
- Extensibilidad (fácil agregar nuevos tipos)
- Mantenibilidad

---

### 10. Exception Handling Pattern

**Descripción**: Manejo estructurado de excepciones con tipos específicos.

**Implementación**:

**Excepciones personalizadas** (Domain Layer):
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
    // Implementación específica
}
```

**Uso**:
```csharp
// En Service Layer
if (user == null)
{
    throw new NotFoundException("User", id);
}

// Captura en Middleware
case NotFoundException:
    response.StatusCode = (int)HttpStatusCode.NotFound;
    break;
```

**Beneficios**:
- Tipos de excepción semánticos
- Manejo específico por tipo
- Mejor logging y debugging
- Documentación implícita

---

### 11. Interface Segregation Principle (ISP)

**Descripción**: Las interfaces deben ser específicas y no forzar a implementar métodos innecesarios.

**Implementación**:

```csharp
// Interfaz específica y enfocada
public interface IUserRepository
{
    Task<int> RegisterUserAsync(User user);
    Task<IEnumerable<dynamic>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}

// Interfaz de servicio enfocada
public interface IUserService
{
    Task<int> RegisterUserAsync(CreateUserDto dto);
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task UpdateAsync(int id, CreateUserDto dto);
    Task DeleteAsync(int id);
}
```

**Beneficios**:
- Interfaces pequeñas y específicas
- Facilita el testing
- Mejor encapsulación
- Cumple SOLID principles

---

## 📊 Resumen de Uso por Capa

### Domain Layer
- **Interface Segregation**: Interfaces específicas
- **Exception Handling Pattern**: Excepciones de dominio

### Application Layer
- **Service Layer Pattern**: Lógica de negocio
- **DTO Pattern**: Transferencia de datos
- **Wrapper Pattern**: Respuestas estándar

### Infrastructure Layer
- **Repository Pattern**: Acceso a datos

### API Layer
- **Dependency Injection**: Registro de servicios
- **Middleware Pattern**: Procesamiento de requests
- **Strategy Pattern**: Manejo de excepciones
- **Builder Pattern**: Configuración

---

## 🔍 Principios SOLID Aplicados

1. **S**ingle Responsibility: Cada clase tiene una responsabilidad
2. **O**pen/Closed: Extensible mediante interfaces
3. **L**iskov Substitution: Interfaces bien definidas
4. **I**nterface Segregation: Interfaces específicas
5. **D**ependency Inversion: Dependencias hacia abstracciones

---

## 📝 Notas Finales

La solución demuestra una aplicación práctica y efectiva de múltiples patrones de diseño que trabajan juntos para crear una arquitectura:

- **Mantenible**: Fácil de entender y modificar
- **Testeable**: Cada componente puede probarse independientemente
- **Escalable**: Preparada para crecer
- **Flexible**: Permite cambios sin afectar otras partes
- **Desacoplada**: Bajo acoplamiento entre componentes

Esta combinación de patrones sigue las mejores prácticas de desarrollo de software y facilita el mantenimiento a largo plazo del código.
