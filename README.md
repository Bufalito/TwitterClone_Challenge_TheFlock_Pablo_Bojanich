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
