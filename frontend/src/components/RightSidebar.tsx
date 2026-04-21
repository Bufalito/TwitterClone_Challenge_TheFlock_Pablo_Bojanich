'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { api, UserSearchResult, TrendingHashtag } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';

export default function RightSidebar() {
  const router = useRouter();
  const { token } = useAuthStore();
  const [suggestedUsers, setSuggestedUsers] = useState<UserSearchResult[]>([]);
  const [trendingHashtags, setTrendingHashtags] = useState<TrendingHashtag[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<UserSearchResult[]>([]);
  const [showSearchResults, setShowSearchResults] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      try {
        // Load trending hashtags
        const hashtags = await api.tweets.getTrending(5);
        setTrendingHashtags(hashtags);

        // Load suggested users
        const users = await api.user.getSuggestions(3, token);
        setSuggestedUsers(users);
      } catch (err) {
        console.error('Error loading sidebar data:', err);
      }
    };
    loadData();
  }, [token]);

  useEffect(() => {
    const searchUsers = async () => {
      if (searchQuery.trim().length < 2) {
        setSearchResults([]);
        setShowSearchResults(false);
        return;
      }

      try {
        const results = await api.user.search(searchQuery.trim());
        setSearchResults(results.slice(0, 5));
        setShowSearchResults(true);
      } catch (err) {
        console.error('Error searching users:', err);
        setSearchResults([]);
      }
    };

    const debounce = setTimeout(() => {
      searchUsers();
    }, 300);

    return () => clearTimeout(debounce);
  }, [searchQuery]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
    }
  };

  const handleFollow = async (userId: string) => {
    if (!token) {
      alert('Please login to follow users');
      return;
    }

    try {
      await api.user.follow(token, userId);
      // Remove from suggestions after following
      setSuggestedUsers(prev => prev.filter(u => u.id !== userId));
      // Reload suggestions
      const users = await api.user.getSuggestions(3, token);
      setSuggestedUsers(users);
    } catch (err: any) {
      alert(`Error: ${err.message}`);
    }
  };

  return (
    <div className="h-screen overflow-y-auto py-4 px-4">
      {/* Search Bar */}
      <div className="mb-4 relative">
        <form onSubmit={handleSearch}>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onFocus={() => searchQuery.trim().length >= 2 && setShowSearchResults(true)}
            onBlur={() => setTimeout(() => setShowSearchResults(false), 200)}
            placeholder="Buscar"
            className="w-full rounded-full bg-gray-800 px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </form>

        {/* Search Results Dropdown */}
        {showSearchResults && searchResults.length > 0 && (
          <div className="absolute z-50 w-full mt-2 bg-gray-800 rounded-xl shadow-xl border border-gray-700 overflow-hidden">
            {searchResults.map((user) => (
              <Link
                key={user.id}
                href={`/user/${user.username}`}
                className="flex items-center gap-3 p-3 hover:bg-gray-700 transition"
              >
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-500 text-white font-bold shrink-0">
                  {user.displayName.charAt(0).toUpperCase()}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="font-bold text-sm truncate">{user.displayName}</div>
                  <div className="text-gray-500 text-xs truncate">@{user.username}</div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>

      {/* What's Happening */}
      {trendingHashtags.length > 0 && (
        <div className="mb-4 overflow-hidden rounded-2xl bg-gray-800">
          <div className="p-4">
            <h2 className="text-xl font-bold">Qué está pasando</h2>
          </div>
          {trendingHashtags.map((trend, index) => (
            <Link
              key={index}
              href={`/search?q=${encodeURIComponent(trend.hashtag)}`}
              className="block hover:bg-gray-700 transition cursor-pointer p-4"
            >
              <div className="text-sm text-gray-500">Tendencia</div>
              <div className="font-bold">{trend.hashtag}</div>
              <div className="text-sm text-gray-500">{trend.count} {trend.count === 1 ? 'tweet' : 'tweets'}</div>
            </Link>
          ))}
        </div>
      )}

      {/* Who to Follow */}
      {suggestedUsers.length > 0 && (
        <div className="overflow-hidden rounded-2xl bg-gray-800">
          <div className="p-4">
            <h2 className="text-xl font-bold">A quién seguir</h2>
          </div>
          {suggestedUsers.map((user) => (
            <div
              key={user.id}
              className="flex items-center justify-between p-4 hover:bg-gray-700 transition"
            >
              <Link href={`/user/${user.username}`} className="flex items-center gap-3 flex-1 min-w-0">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-500 text-white font-bold flex-shrink-0">
                  {user.displayName.charAt(0).toUpperCase()}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="font-bold text-sm hover:underline truncate">{user.displayName}</div>
                  <div className="text-gray-500 text-sm truncate">@{user.username}</div>
                </div>
              </Link>
              <button
                onClick={(e) => {
                  e.preventDefault();
                  handleFollow(user.id);
                }}
                className="rounded-full bg-white px-4 py-1.5 font-bold text-black text-sm transition hover:bg-gray-200 flex-shrink-0"
              >
                Seguir
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Footer */}
      <div className="mt-4 px-4 text-xs text-gray-500 flex flex-wrap gap-2">
        <a href="#" className="hover:underline">Términos de Servicio</a>
        <a href="#" className="hover:underline">Política de Privacidad</a>
        <a href="#" className="hover:underline">Accesibilidad</a>
        <div>© 2026 TwitterClone</div>
      </div>
    </div>
  );
}
