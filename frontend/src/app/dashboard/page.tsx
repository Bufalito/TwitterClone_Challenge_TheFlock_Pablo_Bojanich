'use client';

import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';
import ProtectedRoute from '@/components/ProtectedRoute';

export default function DashboardPage() {
  const router = useRouter();
  const { user, logout } = useAuthStore();

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  return (
    <ProtectedRoute>
      <div className="min-h-screen bg-black text-white">
        <nav className="border-b border-zinc-800 bg-zinc-900">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="flex h-16 items-center justify-between">
              <h1 className="text-xl font-bold">🐦 TwitterClone</h1>
              <button
                onClick={handleLogout}
                className="rounded-lg bg-zinc-800 px-4 py-2 text-sm font-medium transition hover:bg-zinc-700"
              >
                Logout
              </button>
            </div>
          </div>
        </nav>

        <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
            <h2 className="mb-4 text-2xl font-bold">Welcome, {user?.displayName}!</h2>
            
            <div className="space-y-3 text-zinc-300">
              <p>
                <span className="font-semibold text-white">Username:</span> @{user?.username}
              </p>
              <p>
                <span className="font-semibold text-white">Email:</span> {user?.email}
              </p>
              {user?.bio && (
                <p>
                  <span className="font-semibold text-white">Bio:</span> {user.bio}
                </p>
              )}
              <p className="text-sm text-zinc-500">
                Member since {user?.createdAtUtc ? new Date(user.createdAtUtc).toLocaleDateString() : 'N/A'}
              </p>
            </div>

            <div className="mt-6 rounded-lg bg-zinc-800 p-4">
              <p className="text-sm text-zinc-400">
                🚧 More features coming soon! This is a protected route that requires authentication.
              </p>
            </div>
          </div>
        </main>
      </div>
    </ProtectedRoute>
  );
}
