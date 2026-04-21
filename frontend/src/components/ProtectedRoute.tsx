'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';

export default function ProtectedRoute({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const { token, isLoading, loadUser } = useAuthStore();

  useEffect(() => {
    if (!token) {
      router.push('/login');
      return;
    }

    // Load user if not already loaded
    loadUser();
  }, [token, router, loadUser]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-black">
        <div className="text-white">Loading...</div>
      </div>
    );
  }

  if (!token) {
    return null;
  }

  return <>{children}</>;
}
