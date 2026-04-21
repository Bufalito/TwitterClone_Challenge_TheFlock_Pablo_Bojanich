# TwitterClone — Full-Stack Challenge (The Flock)

Clon de Twitter/X desarrollado como challenge full-stack.

## Stack tecnológico

| Capa       | Tecnología                        |
| ---------- | --------------------------------- |
| Frontend   | Next.js 16 · React 19 · Tailwind CSS 4 |
| Backend    | ASP.NET Core 9 Web API            |
| Base de datos | PostgreSQL 16                   |
| Contenedores | Docker · Docker Compose          |

## Estructura del proyecto

```
├── backend/
│   ├── src/
│   │   ├── Api/              # Punto de entrada, endpoints
│   │   ├── Application/      # Casos de uso, interfaces
│   │   ├── Domain/           # Entidades, value objects
│   │   └── Infrastructure/   # EF Core, repositorios, servicios externos
│   ├── Dockerfile
│   └── TwitterClone.slnx
├── frontend/
│   ├── src/app/              # Next.js App Router
│   ├── Dockerfile
│   └── package.json
├── docker-compose.yml
└── README.md
```

## Requisitos previos

- [Docker](https://docs.docker.com/get-docker/) y Docker Compose
- (Opcional para desarrollo local) .NET 9 SDK, Node.js 22+

## Levantar el entorno completo

```bash
# Clonar el repo
git clone <url-del-repo>
cd TwitterClone_Challenge_TheFlock_Pablo_Bojanich

# Levantar todos los servicios
docker compose up --build
```

Una vez levantado:

| Servicio  | URL                          |
| --------- | ---------------------------- |
| Frontend  | http://localhost:3000         |
| Backend   | http://localhost:8080         |
| Health    | http://localhost:8080/api/health |
| PostgreSQL | localhost:5432              |

## Desarrollo local (sin Docker)

### Backend

```bash
cd backend
dotnet restore
dotnet run --project src/Api
```

### Frontend

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

## Variables de entorno

### Backend (`backend/.env.example`)

| Variable | Descripción |
| -------- | ----------- |
| `ConnectionStrings__DefaultConnection` | Connection string de PostgreSQL |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución (`Development`, `Production`) |

### Frontend (`frontend/.env.example`)

| Variable | Descripción |
| -------- | ----------- |
| `NEXT_PUBLIC_API_URL` | URL base del backend API |

## Comandos útiles

```bash
# Levantar servicios
docker compose up --build

# Levantar en segundo plano
docker compose up --build -d

# Ver logs
docker compose logs -f

# Detener servicios
docker compose down

# Detener y borrar volúmenes (resetea la DB)
docker compose down -v
```

## Seed Data (Datos de Prueba)

Al iniciar el backend en modo Development, se te preguntará si deseas generar datos de prueba.

### ¿Cómo usar el seed?

1. **Al levantar con Docker Compose:**
   ```bash
   docker compose up --build
   ```
   El backend te mostrará un prompt:
   ```
   ╔════════════════════════════════════════════╗
   ║     TwitterClone Database Setup            ║
   ╚════════════════════════════════════════════╝
   
   Do you want to seed the database with sample data?
   This will create 12 test users, tweets, follows, and likes.
   
   ⚠️  Warning: Skip this if your database already has data.
   
   Type 'yes' to seed, or press Enter to skip:
   ```
   
2. **Escribe `yes`** y presiona Enter para generar los datos.

3. **Si ya tienes datos**, solo presiona Enter para continuar sin seed.

### Datos generados

El seed crea:
- ✅ **12 usuarios** con perfiles completos
- ✅ **36 tweets** con contenido variado (#hashtags incluidos)
- ✅ **48 relaciones de follow** (red social conectada)
- ✅ **36 likes** distribuidos entre usuarios

### Credenciales de prueba

Todos los usuarios tienen la misma contraseña para facilitar las pruebas:

| Username   | Email              | Password      | Descripción                                    |
|------------|-------------------|---------------|------------------------------------------------|
| `johndoe`  | john@example.com  | `Password123!` | Software engineer                              |
| `janedoe`  | jane@example.com  | `Password123!` | Tech enthusiast                                |
| `alice`    | alice@example.com | `Password123!` | Designer & Developer                           |
| `bob`      | bob@example.com   | `Password123!` | Full-stack developer                           |
| `charlie`  | charlie@example.com | `Password123!` | Data scientist                               |
| `diana`    | diana@example.com | `Password123!` | Product manager                                |
| `eve`      | eve@example.com   | `Password123!` | Frontend developer                             |
| `frank`    | frank@example.com | `Password123!` | Backend engineer                               |
| `grace`    | grace@example.com | `Password123!` | Computer scientist                             |
| `henry`    | henry@example.com | `Password123!` | Entrepreneur                                   |
| `isabel`   | isabel@example.com | `Password123!` | UX/UI Designer                                |
| `jack`     | jack@example.com  | `Password123!` | DevOps engineer                                |

**💡 Tip:** Puedes iniciar sesión con cualquiera de estos usuarios usando su username y la contraseña `Password123!`

### Resetear la base de datos

Si quieres volver a generar los datos de prueba desde cero:

```bash
# Detener servicios y borrar volúmenes
docker compose down -v

# Levantar de nuevo y responder 'yes' al prompt
docker compose up --build
```
