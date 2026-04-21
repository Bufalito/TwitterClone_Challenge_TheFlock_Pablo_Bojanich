'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';
import ProtectedRoute from '@/components/ProtectedRoute';
import TweetComposer from '@/components/TweetComposer';
import TweetList from '@/components/TweetList';
import LeftSidebar from '@/components/LeftSidebar';
import RightSidebar from '@/components/RightSidebar';
import { api, TweetResponse } from '@/lib/api';

export default function DashboardPage() {
  const router = useRouter();
  const { user, logout, token } = useAuthStore();
  const [tweets, setTweets] = useState<TweetResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadTweets = async () => {
    if (!token) return;
    
    try {
      setLoading(true);
      setError(null);
      
      const data = await api.tweets.getRecent(100, token);
      setTweets(data);
    } catch (err: any) {
      setError(err.message || 'Failed to load tweets');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTweets();
  }, [token]);

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
        {/* Twitter-like 3-column layout */}
        <div className="container mx-auto flex max-w-7xl">
          {/* Left Sidebar - Hidden on mobile, visible from md */}
          <div className="hidden md:flex md:w-20 lg:w-64 xl:w-72 border-r border-gray-800 sticky top-0 h-screen">
            <div className="w-full">
              <LeftSidebar />
            </div>
          </div>

          {/* Main Content - Always visible */}
          <div className="flex-1 min-w-0 border-r border-gray-800">
            {/* Mobile Header - Only visible on mobile */}
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
            <div className="sticky top-0 z-10 border-b border-gray-800 bg-gray-900/95 backdrop-blur hidden md:block">
              <div className="flex h-14 items-center justify-between px-4">
                <h1 className="text-xl font-bold">Inicio</h1>
              </div>
            </div>

            {/* Tweet Composer */}
            <div className="border-b border-gray-800">
              <TweetComposer onTweetCreated={handleTweetCreated} />
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

            {/* Tweet List */}
            {!loading && !error && (
              <>
                {tweets.length === 0 ? (
                  <div className="text-center py-12 text-gray-500">
                    <div className="text-4xl mb-4">📝</div>
                    <div className="text-xl font-bold mb-2">Todavía no hay tweets</div>
                    <div>Sé el primero en publicar algo</div>
                  </div>
                ) : (
                  <TweetList tweets={tweets} onTweetDeleted={handleTweetDeleted} />
                )}
              </>
            )}
          </div>

          {/* Right Sidebar - Hidden on mobile and tablet, visible from lg */}
          <div className="hidden lg:block lg:w-80 xl:w-96">
            <div className="sticky top-0">
              <RightSidebar />
            </div>
          </div>
        </div>

        {/* Mobile Bottom Navigation */}
        <div className="fixed bottom-0 left-0 right-0 border-t border-gray-800 bg-gray-900 md:hidden">
          <div className="flex items-center justify-around py-3">
            <button className="flex flex-col items-center gap-1 text-white">
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
