# Frontend — TwitterClone

Aplicación frontend construida con [Next.js](https://nextjs.org) 16, React 19 y Tailwind CSS 4.

## Inicio rápido

```bash
# Instalar dependencias
npm install

# Copiar variables de entorno
cp .env.example .env.local

# Levantar servidor de desarrollo
npm run dev
```

Abrir [http://localhost:3000](http://localhost:3000) en el navegador.

Se puede editar la página principal en `src/app/page.tsx`. La página se actualiza automáticamente al guardar cambios.

## Scripts disponibles

| Comando         | Descripción                    |
| --------------- | ------------------------------ |
| `npm run dev`   | Servidor de desarrollo (Turbopack) |
| `npm run build` | Build de producción            |
| `npm run start` | Servidor de producción         |
| `npm run lint`  | Ejecutar ESLint                |

## Variables de entorno

Ver archivo `.env.example` para las variables requeridas.

| Variable              | Descripción                |
| --------------------- | -------------------------- |
| `NEXT_PUBLIC_API_URL` | URL base del backend API   |
