"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuthStore } from "@/store/authStore";
import UserSearch from "@/components/UserSearch";

export default function Home() {
  const { token, user, loadUser } = useAuthStore();
  const [health, setHealth] = useState<{
    status: string;
    timestamp: string;
  } | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080";
    fetch(`${apiUrl}/api/health`)
      .then((res) => res.json())
      .then(setHealth)
      .catch((err) => setError(err.message));

    // Load user if token exists
    if (token && !user) {
      loadUser();
    }
  }, [token, user, loadUser]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-black text-white px-4">
      <main className="flex flex-col items-center gap-8 max-w-md w-full">
        <h1 className="text-5xl font-bold tracking-tight">
          🐦 TwitterClone
        </h1>
        <p className="text-zinc-400 text-lg text-center">Full-Stack Challenge — The Flock</p>

        <div className="mt-4 rounded-xl border border-zinc-800 bg-zinc-900 p-6 w-full text-center">
          <h2 className="text-sm font-semibold uppercase tracking-wider text-zinc-500 mb-3">
            Backend Status
          </h2>
          {error && (
            <p className="text-red-400 text-sm">
              ❌ Disconnected: {error}
            </p>
          )}
          {health && (
            <div className="space-y-1">
              <p
                className={`text-lg font-bold ${
                  health.status === "healthy"
                    ? "text-green-400"
                    : "text-red-400"
                }`}
              >
                {health.status === "healthy" ? "✅" : "❌"} {health.status}
              </p>
              <p className="text-xs text-zinc-500">{health.timestamp}</p>
            </div>
          )}
          {!health && !error && (
            <p className="text-zinc-500 text-sm animate-pulse">
              Connecting...
            </p>
          )}
        </div>

        {/* User Search */}
        <div className="w-full">
          <UserSearch />
        </div>

        {token && user ? (
          <div className="w-full space-y-4 rounded-xl border border-zinc-800 bg-zinc-900 p-6">
            <p className="text-center text-zinc-300">
              Welcome back, <span className="font-semibold text-white">{user.displayName}</span>!
            </p>
            <Link 
              href="/dashboard"
              className="block w-full rounded-lg bg-blue-600 px-4 py-3 text-center font-semibold text-white transition hover:bg-blue-700"
            >
              Go to Dashboard
            </Link>
          </div>
        ) : (
          <div className="w-full space-y-4">
            <Link 
              href="/register"
              className="block w-full rounded-lg bg-blue-600 px-4 py-3 text-center font-semibold text-white transition hover:bg-blue-700"
            >
              Sign up
            </Link>
            <Link 
              href="/login"
              className="block w-full rounded-lg border border-zinc-700 bg-zinc-800 px-4 py-3 text-center font-semibold text-white transition hover:bg-zinc-700"
            >
              Sign in
            </Link>
          </div>
        )}
      </main>
    </div>
  );
}
