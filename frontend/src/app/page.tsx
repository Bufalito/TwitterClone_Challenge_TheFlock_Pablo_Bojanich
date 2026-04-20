"use client";

import { useEffect, useState } from "react";

export default function Home() {
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
  }, []);

  return (
    <div className="flex min-h-screen items-center justify-center bg-black text-white">
      <main className="flex flex-col items-center gap-8">
        <h1 className="text-5xl font-bold tracking-tight">
          🐦 TwitterClone
        </h1>
        <p className="text-zinc-400 text-lg">Full-Stack Challenge — The Flock</p>

        <div className="mt-4 rounded-xl border border-zinc-800 bg-zinc-900 p-6 w-80 text-center">
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
      </main>
    </div>
  );
}
