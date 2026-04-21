'use client';

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '@/store/authStore';
import ProtectedRoute from '@/components/ProtectedRoute';
import LeftSidebar from '@/components/LeftSidebar';
import RightSidebar from '@/components/RightSidebar';
import { api, UserSearchResult } from '@/lib/api';

export default function SearchPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const query = searchParams.get('q') || '';
  const { user, logout } = useAuthStore();
  const [results, setResults] = useState<UserSearchResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (query) {
      handleSearch(query);
    }
  }, [query]);

  const handleSearch = async (searchQuery: string) => {
    if (!searchQuery.trim()) return;

    setLoading(true);
    setError(null);

    try {
      const data = await api.user.search(searchQuery);
      setResults(data);
    } catch (err: any) {
      setError(err.message || 'Error al buscar');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  return (
    <ProtectedRoute>
      <div className="min-h-screen bg-gray-900 text-white">
        <div className="container mx-auto flex max-w-7xl">
          {/* Left Sidebar */}
          <div className="hidden md:flex md:w-20 lg:w-64 xl:w-72 border-r border-gray-800 sticky top-0 h-screen">
            <div className="w-full">
              <LeftSidebar />
            </div>
          </div>

          {/* Main Content */}
          <div className="flex-1 min-w-0 border-r border-gray-800">
            {/* Mobile Header */}
            <div className="sticky top-0 z-10 border-b border-gray-800 bg-gray-900/95 backdrop-blur md:hidden">
              <div className="flex h-14 items-center justify-between px-4">
                <div className="text-xl font-bold">🐦</div>
                <button
                  onClick={handleLogout}
                  className="text-sm text-gray-400 hover:text-white"
                >
                  Logout
                </button>
              </div>
            </div>

            {/* Desktop Header */}
            <div className="sticky top-0 z-10 border-b border-gray-800 bg-gray-900/95 backdrop-blur">
              <div className="p-4">
                <Link href="/dashboard" className="text-blue-400 hover:underline mb-2 block">
                  ← Volver
                </Link>
                <h1 className="text-xl font-bold">
                  Resultados para: {query.startsWith('#') ? query : `"${query}"`}
                </h1>
              </div>
            </div>

            {/* Loading State */}
            {loading && (
              <div className="flex items-center justify-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
              </div>
            )}

            {/* Error State */}
            {error && (
              <div className="m-4 bg-red-900/20 border border-red-700 rounded-lg p-4 text-red-400">
                {error}
              </div>
            )}

            {/* Results */}
            {!loading && !error && (
              <div>
                {results.length === 0 ? (
                  <div className="text-center py-12 text-gray-500">
                    <div className="text-4xl mb-4">🔍</div>
                    <div className="text-xl font-bold mb-2">No se encontraron resultados</div>
                    <div>Intenta con otro término de búsqueda</div>
                  </div>
                ) : (
                  <div>
                    {results.map((user) => (
                      <Link
                        key={user.id}
                        href={`/user/${user.username}`}
                        className="flex items-center gap-4 p-4 border-b border-gray-800 hover:bg-gray-800/50 transition"
                      >
                        <div className="flex h-12 w-12 items-center justify-center rounded-full bg-blue-500 text-white font-bold flex-shrink-0">
                          {user.displayName.charAt(0).toUpperCase()}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="font-bold hover:underline truncate">{user.displayName}</div>
                          <div className="text-gray-500 truncate">@{user.username}</div>
                          {user.bio && (
                            <div className="text-sm text-gray-400 mt-1 line-clamp-2">{user.bio}</div>
                          )}
                          <div className="text-sm text-gray-500 mt-1">
                            {user.followersCount} {user.followersCount === 1 ? 'seguidor' : 'seguidores'}
                          </div>
                        </div>
                      </Link>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Right Sidebar */}
          <div className="hidden lg:block lg:w-80 xl:w-96">
            <div className="sticky top-0">
              <RightSidebar />
            </div>
          </div>
        </div>

        {/* Mobile Bottom Navigation */}
        <div className="fixed bottom-0 left-0 right-0 border-t border-gray-800 bg-gray-900 md:hidden">
          <div className="flex items-center justify-around py-3">
            <button
              onClick={() => router.push('/dashboard')}
              className="flex flex-col items-center gap-1 text-gray-500"
            >
              <span className="text-2xl">🏠</span>
            </button>
            <button
              onClick={() => router.push(`/user/${user?.username}`)}
              className="flex flex-col items-center gap-1 text-gray-500"
            >
              <span className="text-2xl">👤</span>
            </button>
            <button
              onClick={handleLogout}
              className="flex flex-col items-center gap-1 text-gray-500"
            >
              <span className="text-2xl">🚪</span>
            </button>
          </div>
        </div>
      </div>
    </ProtectedRoute>
  );
}
