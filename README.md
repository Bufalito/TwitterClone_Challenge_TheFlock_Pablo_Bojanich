# TwitterClone — Full-Stack Challenge (The Flock)

Clon de Twitter/X desarrollado como challenge full-stack. Incluye autenticación JWT, timeline cronológico, sistema de follows, likes, hashtags trending y un sistema completo de replies con vista de hilo.

## Stack tecnológico

| Capa          | Tecnología                                           |
| ------------- | ---------------------------------------------------- |
| Frontend      | Next.js 16.2 · React 19 · TypeScript 5 · Tailwind CSS 4 |
| State         | Zustand 5                                            |
| Backend       | ASP.NET Core net10.0 · Clean Architecture            |
| ORM / Queries | EF Core 9 + Dapper (queries ad-hoc)                  |
| Auth          | JWT Bearer · ASP.NET Core Identity `IPasswordHasher` |
| Base de datos | PostgreSQL 16                                        |
| Contenedores  | Docker · Docker Compose                              |
| Tests         | Jest 30 + Testing Library · xUnit · Playwright 1.59  |

## Estructura del proyecto

```
├── backend/
│   ├── src/
│   │   ├── Api/              # Controllers, Program.cs, configuración HTTP
│   │   ├── Application/      # Interfaces, servicios, DTOs, casos de uso
│   │   ├── Domain/           # Entidades, reglas de negocio, value objects
│   │   └── Infrastructure/   # EF Core, migraciones, seeders, JWT service
│   ├── tests/
│   │   ├── Api.IntegrationTests/   # Tests de integración con WebApplicationFactory
│   │   └── DomainModel.Tests/      # Tests de dominio y reglas de negocio
│   └── TwitterClone.slnx
├── frontend/
│   ├── src/
│   │   ├── app/              # Next.js App Router (páginas y layouts)
│   │   ├── components/       # Componentes reutilizables
│   │   ├── lib/api.ts        # Cliente HTTP tipado (Fetch API nativo)
│   │   └── store/authStore.ts # Estado de autenticación (Zustand)
│   ├── __tests__/            # Tests unitarios con Jest + Testing Library
│   └── e2e/                  # Tests E2E con Playwright
├── docker-compose.yml
└── README.md
```

---

## Setup paso a paso

### Opción A — Docker Compose (recomendado)

**Requisitos:** Docker Desktop instalado y corriendo.

```bash
# 1. Clonar el repositorio
git clone <url-del-repo>
cd TwitterClone_Challenge_TheFlock_Pablo_Bojanich

# 2. Levantar todos los servicios (DB + backend + frontend)
docker compose up --build
```

Cuando el backend inicia, muestra un prompt para cargar datos de prueba:

```
╔════════════════════════════════════════════╗
║     TwitterClone Database Setup            ║
╚════════════════════════════════════════════╝

Do you want to seed the database with sample data?
Type 'yes' to seed, or press Enter to skip:
```

Escribir `yes` + Enter para generar los datos de prueba.

| Servicio   | URL                              |
| ---------- | -------------------------------- |
| Frontend   | http://localhost:3000            |
| Backend    | http://localhost:8080            |
| Health     | http://localhost:8080/api/health |
| PostgreSQL | localhost:5432                   |

---

### Opción B — Desarrollo local (sin Docker)

**Requisitos:** .NET 10 SDK, Node.js 22+, PostgreSQL 16 corriendo en localhost:5432.

#### 1. Base de datos

```bash
# Crear la base de datos (solo la primera vez)
psql -U postgres -c "CREATE DATABASE twitterclone;"
```

#### 2. Backend

```bash
cd backend

# Restaurar dependencias
dotnet restore

# Aplicar migraciones de EF Core
dotnet ef database update --project src/Infrastructure --startup-project src/Api

# Iniciar el servidor (puerto 5171 por defecto)
dotnet run --project src/Api
```

El backend arrancará en `http://localhost:5171`. Responder `yes` al prompt de seed para cargar datos de prueba.

#### 3. Frontend

```bash
cd frontend

# Instalar dependencias
npm install

# Crear archivo de variables de entorno
echo "NEXT_PUBLIC_API_URL=http://localhost:5171" > .env.local

# Iniciar el servidor de desarrollo (puerto 3000)
npm run dev
```

---

## Variables de entorno

### Backend

Las variables se pueden setear via `appsettings.Development.json` o como variables de entorno del sistema operativo.

| Variable                                | Descripción                                            | Valor por defecto (Docker)                                            |
| --------------------------------------- | ------------------------------------------------------ | --------------------------------------------------------------------- |
| `ConnectionStrings__DefaultConnection`  | Connection string de PostgreSQL                        | `Host=db;Port=5432;Database=twitterclone;Username=postgres;Password=postgres` |
| `ASPNETCORE_ENVIRONMENT`                | Entorno de ejecución (`Development` / `Production`)    | `Development`                                                         |
| `ASPNETCORE_URLS`                       | URL(s) en las que escucha el servidor                  | `http://+:8080`                                                       |
| `JwtSettings__Secret`                   | Clave secreta para firmar tokens JWT (mín. 32 chars)   | Valor en `appsettings.json`                                           |
| `JwtSettings__Issuer`                   | Issuer del token JWT                                   | `TwitterCloneApi`                                                     |
| `JwtSettings__Audience`                 | Audience del token JWT                                 | `TwitterCloneClient`                                                  |
| `JwtSettings__ExpiryMinutes`            | Tiempo de expiración del token en minutos              | `60`                                                                  |
| `TWITTERCLONE_AUTO_SEED`                | `true` para seedear sin prompt (usado en tests E2E)    | no definida                                                           |
| `TWITTERCLONE_SKIP_SEED_PROMPT`         | `true` para saltar el prompt silenciosamente           | no definida                                                           |

### Frontend

| Variable               | Descripción                             | Valor por defecto (Docker) |
| ---------------------- | --------------------------------------- | -------------------------- |
| `NEXT_PUBLIC_API_URL`  | URL base del backend API                | `http://localhost:8080`    |

---

## Seed — Datos de prueba

### Seed interactivo (local / Docker)

Al iniciar el backend en modo Development, aparece un prompt. Responder `yes` para cargar los datos.

### Seed automático (sin prompt)

```bash
TWITTERCLONE_AUTO_SEED=true dotnet run --project src/Api
```

### Datos generados

| Tipo               | Cantidad |
| ------------------ | -------- |
| Usuarios           | 12       |
| Tweets             | 36       |
| Relaciones de follow | 48     |
| Likes              | 36       |

### Credenciales de prueba

Todos los usuarios comparten la misma contraseña: **`Password123!`**

| Username  | Email                   |
| --------- | ----------------------- |
| `johndoe` | john@example.com        |
| `janedoe` | jane@example.com        |
| `alice`   | alice@example.com       |
| `bob`     | bob@example.com         |
| `charlie` | charlie@example.com     |
| `diana`   | diana@example.com       |
| `eve`     | eve@example.com         |
| `frank`   | frank@example.com       |
| `grace`   | grace@example.com       |
| `henry`   | henry@example.com       |
| `isabel`  | isabel@example.com      |
| `jack`    | jack@example.com        |

### Resetear la base de datos

```bash
# Detener servicios y borrar el volumen de PostgreSQL
docker compose down -v

# Levantar de nuevo y responder 'yes' al prompt de seed
docker compose up --build
```

---

## Tests

### Tests unitarios — Frontend (Jest)

```bash
cd frontend

# Ejecutar todos los tests
npm test

# Modo watch (re-ejecuta al guardar)
npm run test:watch
```

Cobertura:
- `authStore.test.ts` — Zustand store de autenticación (login, register, logout)
- `login.test.tsx` — Página de login (validación de formulario, manejo de errores)
- `tweetComposer.test.tsx` — Compositor de tweets (caracteres, submit)
- `tweetList.test.tsx` — Lista de tweets (render, likes, navegación)
- `tweetLikes.test.tsx` — Toggle de likes
- `userFollow.test.tsx` — Botón follow/unfollow
- `userSearch.test.tsx` — Búsqueda de usuarios

### Tests de dominio y de integración — Backend (xUnit)

```bash
cd backend

# Ejecutar todos los tests
dotnet test

# Solo tests de dominio
dotnet test tests/DomainModel.Tests

# Solo tests de integración
dotnet test tests/Api.IntegrationTests
```

Los tests de integración usan `WebApplicationFactory` con una base de datos en memoria (`Testing` environment), sin necesidad de PostgreSQL.

### Tests E2E — Playwright

Los tests E2E requieren PostgreSQL corriendo localmente en localhost:5432.

```bash
cd frontend

# Instalar navegadores de Playwright (solo la primera vez)
npx playwright install

# Ejecutar tests E2E (Playwright levanta backend y frontend automáticamente)
npx playwright test

# Modo UI interactivo
npx playwright test --ui

# Ver reporte del último run
npx playwright show-report
```

Playwright levanta automáticamente:
- **Backend** en `http://localhost:5172` con `TWITTERCLONE_AUTO_SEED=true`
- **Frontend** en `http://localhost:3000` apuntando al backend de test

Cobertura E2E:
- Registro e inicio de sesión
- Publicar tweets, likes
- Seguir y dejar de seguir usuarios
- Timeline personalizado
- Vista de perfil de usuario
- Hilo de replies

---

## Decisiones técnicas

### EF Core vs Dapper

Se utiliza **EF Core 9** como ORM principal para las operaciones CRUD estándar (crear/leer/actualizar/eliminar entidades). EF Core aporta:
- Migraciones automáticas y control de esquema
- Fuertemente tipado y seguro ante refactors
- Carga de relaciones con `Include` (navegación de grafos de objetos)

Para las queries que no encajan bien con LINQ (ej. `GetTrendingHashtagsAsync`), se utiliza **Dapper** vía la conexión de la misma `DbContext`, evitando una segunda conexión a la base de datos. Esto permite SQL directo cuando la expresividad lo requiere, sin abandonar el modelo de EF para el resto del sistema.

### Autenticación

Se optó por **JWT stateless** con los siguientes fundamentos:

- **`IPasswordHasher<User>`** de ASP.NET Core Identity: hashing seguro (PBKDF2 con salt y stretching) sin depender de toda la infraestructura de Identity (no hay `UserManager`, ni `SignInManager`, ni tablas de Identity). Esto mantiene el modelo de dominio limpio.
- **Tokens sin refresh**: Para la escala del challenge, un token de 60 minutos es suficiente. Un sistema de refresh tokens añadiría complejidad (tabla de tokens revocados, rotación, etc.) sin beneficio práctico en este contexto.
- **`AllowAnonymous` selectivo**: Los endpoints de lectura pública (explore, perfil de usuario, hilo de tweets) aceptan tanto usuarios autenticados como anónimos. El token opcional permite marcar los likes del usuario actual en la respuesta.
- **Frontend**: El token se persiste en `localStorage` via Zustand con `persist` middleware. Se descartó `httpOnly cookie` para simplificar el cliente, aceptando el trade-off de XSS vs CSRF.

### Timeline

El timeline personal (`GET /api/timeline`) devuelve los tweets propios del usuario más los tweets de los usuarios que sigue, excluyendo replies (solo tweets raíz). La estrategia:

1. Se obtienen los IDs de los usuarios seguidos en una query separada.
2. Se filtra `WHERE (userId == me OR userId IN followedIds) AND parentTweetId IS NULL`.
3. Los contadores de likes y replies se calculan con subconsultas correlacionadas dentro de la proyección LINQ, lo que EF Core traduce a SQL eficiente sin traer colecciones completas a memoria.
4. Se aplica paginación por offset (`Skip` / `Take`) con orden descendente por fecha.

Esta aproximación es correcta para la escala del challenge. En producción, un sistema real requeriría un feed materializado (fan-out on write) o una cache de timeline por usuario para soportar alto volumen.

---

## Uso de IA

Este proyecto fue desarrollado con la asistencia de **GitHub Copilot** (modelo Claude Sonnet 4.5 / 4.6) en Visual Studio Code, **Chat GPT** (estructuracion de plan / analisis y armados de PR).

### Cómo se usó

- **Scaffolding inicial**: generación de la estructura de Clean Architecture (capas Domain / Application / Infrastructure / Api) y los archivos de proyecto `.csproj`.
- **Implementación de features**: las interfaces, servicios y controladores de cada feature (follows, likes, timeline, replies) fueron generados y revisados iterativamente con el agente.
- **Migraciones EF Core**: el agente generó el código de configuración (`IEntityTypeConfiguration`) y guió la creación de migraciones.
- **Tests**: los tests unitarios de Jest y los de integración de xUnit fueron generados con el agente y ajustados para reflejar el comportamiento real del sistema.
- **Debugging**: errores de compilación (CS8803, tipos duplicados, Tailwind v4 breaking changes) fueron diagnosticados y corregidos con ayuda del agente.
- **Este README**: redactado con el agente a partir del contexto real del código.

### Criterio de uso

El código generado fue siempre revisado y validado manualmente. El agente actuó como par de programación acelerador: propuso implementaciones que luego se ajustaron según las necesidades del dominio. No se aceptó código sin entenderlo.

---

## Comandos de referencia rápida

```bash
# Docker — levantar todo
docker compose up --build

# Docker — levantar en background
docker compose up --build -d

# Docker — ver logs
docker compose logs -f

# Docker — detener
docker compose down

# Docker — resetear DB
docker compose down -v

# Backend — correr localmente
cd backend && dotnet run --project src/Api

# Backend — correr tests
cd backend && dotnet test

# Backend — agregar migración
cd backend && dotnet ef migrations add <NombreMigracion> --project src/Infrastructure --startup-project src/Api

# Frontend — desarrollo
cd frontend && npm run dev

# Frontend — tests unitarios
cd frontend && npm test

# Frontend — tests E2E
cd frontend && npx playwright test
```
