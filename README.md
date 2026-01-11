# Coink User Management API

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL%2018-336791.svg)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-green.svg)]()

Esta es una solución robusta y escalable desarrollada en **.NET 8** que implementa una arquitectura limpia (**Clean Architecture**) para la gestión y registro de usuarios con una estructura geográfica jerárquica (País, Departamento, Municipio).

---

## 🛠️ Guía de Configuración e Instalación

Siga estos pasos para configurar el entorno y ejecutar la solución en una máquina local.

### 1. Prerrequisitos
* **.NET 8 SDK** instalado.
* **PostgreSQL 18** o superior en ejecución.
* Un IDE de su preferencia (Visual Studio, VS Code o JetBrains Rider).

### 2. Inicialización de la Base de Datos
1. Abra su cliente de PostgreSQL (pgAdmin, DBeaver, etc.).
2. Cree una base de datos con el nombre: `coink_users`.
3. Ejecute los scripts SQL en el siguiente orden para asegurar la integridad de las llaves foráneas:
   * **Paso 1 (DDL):** Ejecute el script de creación de tablas (`Countries`, `Departments`, `Municipalities`, `Users`).
   * **Paso 2 (Functions):** Ejecute el script que crea el Stored Procedure `sp_RegisterUser`.
   * **Paso 3 (DML Seeding):** Ejecute el script de inserción de datos maestros para poblar la jerarquía geográfica.

### 3. Configuración de la API
Localice el archivo `appsettings.json` en el proyecto **UserManagement.API** y actualice la cadena de conexión con sus credenciales locales:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=coink_users;Username=postgres;Password=TU_PASSWORD"
}
```
### 4. Ejecución
Desde una terminal situada en la raíz de la solución, ejecute los siguientes comandos para poner en marcha el sistema:

```bash
# 1. Restaurar las dependencias de NuGet en todos los proyectos
dotnet restore

# 2. Compilar la solución para verificar que no existan errores
dotnet build

# 3. Iniciar el proyecto de la API
dotnet run --project UserManagement.API

```

Una vez ejecutada, la API estará disponible y podrá acceder a la interfaz interactiva de Swagger en la siguiente URL para realizar pruebas: https://localhost:5001/swagger/index.html (o el puerto que le asigne su terminal).

### 📐 Arquitectura Técnica
Diagrama de Contenedores (C4)
El sistema sigue los principios de Clean Architecture, asegurando que la lógica de negocio no dependa de la base de datos o de los frameworks externos.

```mermaid
graph TD
    Client[Client Browser/Postman] -- HTTP POST --> API[Presentation Layer: API]
    API -- Call --> APP[Application Layer: Services]
    APP -- Interface --> DOM[Domain Layer: Entities/Interfaces]
    INF[Infrastructure Layer: Repositories] -- Implements --> DOM
    INF -- Dapper Query --> DB[(PostgreSQL: coink_users)]
```
 #### Diagrama Entidad-Relación (ERD)
La base de datos está normalizada para mantener la integridad de la jerarquía geográfica.
```mermaid
erDiagram
    Countries ||--o{ Departments : contains
    Departments ||--o{ Municipalities : contains
    Municipalities ||--o{ Users : resides_in

    Countries {
        int Id PK
        string Name
        string PhoneCode
    }
    Departments {
        int Id PK
        string Name
        int CountryId FK
    }
    Municipalities {
        int Id PK
        string Name
        int DepartmentId FK
    }
    Users {
        int Id PK
        string Name
        string Phone
        string Address
        int MunicipalityId FK
        timestamp CreatedAt
    }
```
#### Diagrama de Secuencia de Registro
Este flujo detalla cómo viaja la información desde el cliente hasta el procedimiento almacenado en PostgreSQL.
```mermaid
sequenceDiagram
    participant C as Client
    participant Ctrl as UsersController
    participant S as UserService
    participant R as UserRepository
    participant DB as PostgreSQL (sp_RegisterUser)

    C->>Ctrl: POST /api/users (CreateUserDto)
    Ctrl->>S: RegisterUserAsync(dto)
    S->>S: Validate Inputs (Phone Regex)
    S->>R: RegisterUserAsync(userEntity)
    R->>DB: CALL sp_RegisterUser(...)
    DB-->>R: Returns New ID
    R-->>S: Returns New ID
    S-->>Ctrl: Returns Result
    Ctrl-->>C: 200 OK (UserId)
```

### 🛡️ Documentación de Endpoints
Registro de Usuario
* URL: /api/users
* Método: POST
* Cuerpo (JSON):
```json
{
  "name": "John Doe",
  "phone": "+573001234567",
  "address": "Calle 123 # 45-67",
  "countryId": 1,
  "departmentId": 1,
  "municipalityId": 1
}
```
### Validaciones: El sistema verifica que el nombre y la dirección no estén vacíos, y que el teléfono cumpla con un formato numérico internacional válido (7 a 15 dígitos).