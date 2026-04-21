'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '@/store/authStore';
import ProtectedRoute from '@/components/ProtectedRoute';
import TweetComposer from '@/components/TweetComposer';
import TweetList from '@/components/TweetList';
import { api, TweetResponse } from '@/lib/api';

export default function DashboardPage() {
  const router = useRouter();
  const { user, logout, token } = useAuthStore();
  const [tweets, setTweets] = useState<TweetResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadTweets = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.tweets.getRecent(50, token || undefined);
      setTweets(data);
    } catch (err: any) {
      setError(err.message || 'Failed to load tweets');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTweets();
  }, []);

  const handleTweetCreated = (newTweet: TweetResponse) => {
    setTweets([newTweet, ...tweets]);
  };

  const handleTweetDeleted = (tweetId: string) => {
    setTweets(tweets.filter((t) => t.id !== tweetId));
  };

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  return (
    <ProtectedRoute>
      <div className="min-h-screen bg-gray-900 text-white">
        <nav className="border-b border-gray-800 bg-gray-950 sticky top-0 z-10">
          <div className="mx-auto max-w-4xl px-4 sm:px-6">
            <div className="flex h-16 items-center justify-between">
              <Link href="/dashboard" className="text-xl font-bold hover:text-blue-400 transition-colors">
                🐦 TwitterClone
              </Link>
              <div className="flex items-center gap-4">
                <Link
                  href={`/user/${user?.username}`}
                  className="text-sm text-gray-400 hover:text-white transition-colors"
                >
                  @{user?.username}
                </Link>
                <button
                  onClick={handleLogout}
                  className="rounded-lg bg-gray-800 px-4 py-2 text-sm font-medium transition hover:bg-gray-700"
                >
                  Logout
                </button>
              </div>
            </div>
          </div>
        </nav>

        <main className="mx-auto max-w-4xl px-4 py-6 sm:px-6">
          {/* Tweet Composer */}
          <TweetComposer onTweetCreated={handleTweetCreated} />

          {/* Feed Header */}
          <div className="mb-4">
            <h2 className="text-xl font-bold">Recent Tweets</h2>
          </div>

          {/* Loading State */}
          {loading && (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
          )}

          {/* Error State */}
          {error && (
            <div className="bg-red-900/20 border border-red-700 rounded-lg p-4 text-red-400">
              {error}
            </div>
          )}

          {/* Tweet List */}
          {!loading && !error && (
            <TweetList tweets={tweets} onTweetDeleted={handleTweetDeleted} />
          )}
        </main>
      </div>
    </ProtectedRoute>
  );
}
