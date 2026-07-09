# Coink User Management API

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL%2018-336791.svg)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-green.svg)]()

Esta es una solución robusta y escalable desarrollada en **.NET 8** que implementa una arquitectura limpia (**Clean Architecture**) para la gestión y registro de usuarios con una estructura geográfica jerárquica (País, Departamento, Municipio).

---

## 🚀 Inicio Rápido

Elige el método de ejecución que prefieras:

- **[Ejecución con Docker](#-ejecución-con-docker)** (Recomendado - Más fácil y rápido)
- **[Ejecución sin Docker](#-ejecución-sin-docker)** (Requiere instalaciones locales)

---

## 🐳 Ejecución con Docker

Esta es la forma más sencilla de ejecutar la solución. Docker se encargará de configurar automáticamente PostgreSQL y la API.

### Prerrequisitos

- **Docker Desktop** instalado y ejecutándose
  - Windows/Mac: Descargar desde [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
  - Linux: Instalar Docker Engine y Docker Compose

### Pasos de Ejecución

1. **Clonar o descargar el repositorio** (si aún no lo has hecho)

2. **Abrir una terminal** en la raíz del proyecto (donde está el archivo `docker-compose.yml`)

3. **Ejecutar Docker Compose:**
   ```bash
   docker-compose up -d
   ```
   Este comando:
   - Descargará las imágenes necesarias (si es la primera vez)
   - Construirá la imagen de la API
   - Iniciará PostgreSQL 18
   - Inicializará la base de datos con los scripts SQL
   - Iniciará la API .NET

4. **Verificar que los servicios estén corriendo:**
   ```bash
   docker-compose ps
   ```
   Deberías ver ambos servicios (`coink-api` y `coink-postgres`) con estado `Up`.

5. **Acceder a la API:**
   - **Swagger UI**: http://localhost:8080/swagger
   - **API Base**: http://localhost:8080
   - **Endpoint de usuarios**: http://localhost:8080/api/users

### Comandos Útiles de Docker

```bash
# Ver los logs de la API
docker-compose logs -f api

# Ver los logs de PostgreSQL
docker-compose logs -f postgres

# Detener los servicios
docker-compose down

# Detener los servicios y eliminar volúmenes (cuidado: elimina datos)
docker-compose down -v

# Reconstruir las imágenes (útil después de cambios en el código)
docker-compose up -d --build

# Reiniciar un servicio específico
docker-compose restart api
```

### Configuración de Docker

La configuración de Docker está en `docker-compose.yml`:

- **API**: Puerto 8080 (configurable en docker-compose.yml)
- **PostgreSQL**: Puerto 5432 (configurable en docker-compose.yml)
- **Base de datos**: `coink_users`
- **Usuario PostgreSQL**: `postgres`
- **Contraseña PostgreSQL**: `postgres123` (cambiar en producción)

Para cambiar las credenciales o puertos, edita el archivo `docker-compose.yml`.

---

## 💻 Ejecución sin Docker

Si prefieres ejecutar la solución directamente en tu máquina local sin Docker.

### Prerrequisitos

- **.NET 8 SDK** instalado
  - Descargar desde [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
  - Verificar instalación: `dotnet --version`
- **PostgreSQL 18** o superior instalado y ejecutándose
  - Descargar desde [postgresql.org/download](https://www.postgresql.org/download/)
  - Crear usuario y base de datos
- Un IDE de su preferencia (Visual Studio, VS Code, JetBrains Rider)

### Pasos de Ejecución

#### 1. Configurar PostgreSQL

1. Abrir su cliente de PostgreSQL (pgAdmin, DBeaver, psql, etc.)

2. Crear una base de datos:
   ```sql
   CREATE DATABASE coink_users;
   ```

3. Ejecutar los scripts SQL en el siguiente orden:
   
   Los scripts están en la carpeta `scripts/`:
   - **01_Schema_and_Table_Creation.sql**: Crea las tablas (Countries, Departments, Municipalities, Users)
   - **02_create_Stored_Procedures.sql**: Crea los stored procedures
   - **03_Insert_data.sql**: Inserta datos maestros de ejemplo
   
   **Orden de ejecución** (importante para la integridad de las llaves foráneas):
   1. Primero: `01_Schema_and_Table_Creation.sql`
   2. Segundo: `02_create_Stored_Procedures.sql`
   3. Tercero: `03_Insert_data.sql`

#### 2. Configurar la API

1. Localizar el archivo `appsettings.json` en el proyecto **UserManagement.API**

2. Actualizar la cadena de conexión con sus credenciales:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=coink_users;Username=TU_USUARIO;Password=TU_PASSWORD"
     }
   }
   ```

#### 3. Ejecutar la Aplicación

Desde una terminal en la raíz de la solución:

```bash
# 1. Restaurar las dependencias de NuGet
dotnet restore

# 2. Compilar la solución
dotnet build

# 3. Ejecutar la API
dotnet run --project UserManagement.API
```

La API se iniciará y mostrará en la consola la URL donde está disponible (generalmente `https://localhost:5001` o `http://localhost:5000`).

#### 4. Acceder a la API

Una vez ejecutada, podrás acceder a:

- **Swagger UI**: https://localhost:5001/swagger (o el puerto que muestre la consola)
- **API Base**: https://localhost:5001
- **Endpoint de usuarios**: https://localhost:5001/api/users

---

## 📚 Documentación de API

### Endpoints Disponibles

#### GET /api/users
Obtiene todos los usuarios registrados.

**Respuesta exitosa (200 OK):**
```json
[
  {
    "id": 1,
    "name": "John Doe",
    "phone": "+573001234567",
    "address": "Calle 123 # 45-67",
    "municipalityId": 1,
    "municipalityName": "Medellín",
    "departmentName": "Antioquia",
    "countryName": "Colombia"
  }
]
```

#### GET /api/users/{id}
Obtiene un usuario por su ID.

**Respuesta exitosa (200 OK):**
```json
{
  "succeeded": true,
  "data": {
    "id": 1,
    "name": "John Doe",
    "phone": "+573001234567",
    "address": "Calle 123 # 45-67",
    "municipalityId": 1
  },
  "message": null
}
```

#### POST /api/users
Registra un nuevo usuario.

**Cuerpo de la petición (JSON):**
```json
{
  "name": "John Doe",
  "phone": "+573001234567",
  "address": "Calle 123 # 45-67",
  "municipalityId": 1
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "succeeded": true,
  "data": 1,
  "message": "User created successfully"
}
```

**Validaciones:**
- `name`: No puede estar vacío
- `phone`: Debe cumplir formato numérico internacional válido (7 a 15 dígitos)
- `address`: No puede estar vacío
- `municipalityId`: Debe existir en la base de datos

#### PUT /api/users/{id}
Actualiza la información de un usuario existente.

**Cuerpo de la petición (JSON):**
```json
{
  "name": "Jane Doe",
  "phone": "+573009876543",
  "address": "Calle 456 # 78-90",
  "municipalityId": 2
}
```

**Respuesta exitosa (204 No Content)**

#### DELETE /api/users/{id}
Elimina un usuario.

**Respuesta exitosa (204 No Content)**

### Documentación Interactiva

La mejor forma de probar la API es usando **Swagger UI**, disponible en:
- Con Docker: http://localhost:8080/swagger
- Sin Docker: https://localhost:5001/swagger (o el puerto que muestre la consola)

Swagger proporciona una interfaz interactiva donde puedes probar todos los endpoints directamente desde el navegador.

---

## 📖 Documentación Adicional

- **[ARCHITECTURE.md](./ARCHITECTURE.md)**: Documentación técnica detallada sobre arquitectura, diseño de base de datos, diagramas y flujos de proceso.
- **[DESIGN_PATTERNS.md](./DESIGN_PATTERNS.md)**: Análisis completo de los patrones de diseño implementados en la solución (Repository, Dependency Injection, Service Layer, DTO, Middleware, etc.).

---

## ⚙️ Configuración Avanzada

### Variables de Entorno

Puedes configurar la aplicación usando variables de entorno en lugar de `appsettings.json`:

- `ASPNETCORE_ENVIRONMENT`: Entorno de ejecución (Development, Production)
- `ConnectionStrings__DefaultConnection`: Cadena de conexión a PostgreSQL
- `ASPNETCORE_URLS`: URLs donde la API escuchará

### Logs

Los logs se guardan en:
- Con Docker: Dentro del contenedor en `/app/logs/` (ver con `docker-compose logs api`)
- Sin Docker: En la carpeta `UserManagement.API/logs/`

---

## 🆘 Solución de Problemas

### La API no inicia

1. Verificar que PostgreSQL esté corriendo
2. Verificar la cadena de conexión en `appsettings.json`
3. Verificar que la base de datos `coink_users` exista
4. Verificar que los scripts SQL se hayan ejecutado correctamente

### Error de conexión a la base de datos

1. Verificar que PostgreSQL esté ejecutándose
2. Verificar credenciales (usuario y contraseña)
3. Verificar que el puerto 5432 esté disponible
4. Verificar firewall (si aplica)

### Docker: Contenedor no inicia

1. Verificar que Docker Desktop esté ejecutándose
2. Ver logs: `docker-compose logs api` o `docker-compose logs postgres`
3. Verificar que los puertos 8080 y 5432 no estén en uso
4. Reconstruir: `docker-compose up -d --build`

---

## 📄 Licencia

Este proyecto es parte de una evaluación técnica.
