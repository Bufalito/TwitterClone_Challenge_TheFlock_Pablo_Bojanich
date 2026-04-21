# Frontend Authentication - TwitterClone

Implementación de autenticación JWT con Next.js 16, React 19, TypeScript y Zustand.

## 🚀 Features

- ✅ **Login y Register** - Páginas completas con validación
- ✅ **JWT Token Management** - Almacenamiento persistente con Zustand
- ✅ **Protected Routes** - Middleware para rutas autenticadas
- ✅ **Auth Store** - State management centralizado
- ✅ **Fetch API nativo** - Sin vulnerabilidades de axios
- ✅ **Mobile-first UI** - Diseño responsive con Tailwind CSS
- ✅ **Tests** - Cobertura de auth store y login flow

## 📦 Instalación

```bash
npm install
```

## 🔧 Configuración

Crear archivo `.env.local`:

```bash
NEXT_PUBLIC_API_URL=http://localhost:8080
```

## 🏃 Desarrollo

```bash
npm run dev
```

Abrir [http://localhost:3000](http://localhost:3000)

## 🧪 Tests

```bash
# Ejecutar todos los tests
npm test

# Watch mode
npm run test:watch
```

## 📁 Estructura

```
src/
├── app/
│   ├── login/page.tsx          # Página de login
│   ├── register/page.tsx       # Página de registro
│   ├── dashboard/page.tsx      # Página protegida (ejemplo)
│   └── page.tsx                # Home con estado de auth
├── components/
│   └── ProtectedRoute.tsx      # HOC para rutas protegidas
├── lib/
│   └── api.ts                  # Cliente API con fetch nativo
├── store/
│   └── authStore.ts            # Zustand store para auth
└── __tests__/
    ├── authStore.test.ts       # Tests del store
    └── login.test.tsx          # Tests de la página de login
```

## 🔐 Auth Flow

### 1. Register
```typescript
const { register } = useAuthStore();
await register({
  name: 'John Doe',
  username: 'johndoe',
  email: 'john@example.com',
  password: 'password123',
  bio: 'Optional bio', // opcional
});
// Auto-login después de registro exitoso
```

### 2. Login
```typescript
const { login } = useAuthStore();
await login({
  username: 'johndoe',
  password: 'password123',
});
// Token y user se guardan automáticamente
```

### 3. Logout
```typescript
const { logout } = useAuthStore();
logout(); // Limpia token y user del store
```

### 4. Acceder a usuario
```typescript
const { user, token } = useAuthStore();
if (token && user) {
  console.log(user.displayName); // Info del usuario
}
```

## 🛡️ Protected Routes

Usar el componente `ProtectedRoute` para páginas que requieren autenticación:

```tsx
import ProtectedRoute from '@/components/ProtectedRoute';

export default function DashboardPage() {
  return (
    <ProtectedRoute>
      <div>Contenido protegido</div>
    </ProtectedRoute>
  );
}
```

## 🎨 UI Components

Todas las páginas usan:
- **Tailwind CSS** para estilos
- **Mobile-first** approach
- **Dark theme** consistente con el backend

## 📝 API Client

El cliente API usa `fetch` nativo:

```typescript
import { api } from '@/lib/api';

// Login
const response = await api.auth.login({ username, password });

// Register
await api.auth.register({ name, username, email, password });

// Get current user (requiere token)
const user = await api.user.getMe(token);
```

## ✅ Tests Coverage

- **Auth Store**: 6 tests
  - Login exitoso
  - Login fallido
  - Register + auto-login
  - Logout
  - Clear error

- **Login Page**: 5 tests
  - Render del formulario
  - Mostrar errores
  - Submit del formulario
  - Loading state
  - Validación

## 🔄 State Persistence

El token y user se persisten en `localStorage` usando Zustand middleware:

```typescript
// Automáticamente guardado
login() -> localStorage.setItem('auth-storage', ...)

// Automáticamente cargado al refresh
useAuthStore() -> localStorage.getItem('auth-storage')
```

## 🚧 Next Steps

- Implementar refresh token
- Agregar rate limiting en el cliente
- Mejorar manejo de errores
- Agregar features sociales (tweets, likes, follows)

## 📄 License

MIT
