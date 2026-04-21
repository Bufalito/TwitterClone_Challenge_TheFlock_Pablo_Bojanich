'use client';

import { useState } from 'react';
import Link from 'next/link';
import { TweetResponse } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';
import { api } from '@/lib/api';

interface TweetListProps {
  tweets: TweetResponse[];
  onTweetDeleted?: (tweetId: string) => void;
}

export default function TweetList({ tweets, onTweetDeleted }: TweetListProps) {
  const { token, user } = useAuthStore();
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [likingId, setLikingId] = useState<string | null>(null);
  const [likedTweets, setLikedTweets] = useState<Set<string>>(
    new Set(tweets.filter(t => t.isLikedByCurrentUser).map(t => t.id))
  );
  const [likeCounts, setLikeCounts] = useState<Record<string, number>>(
    tweets.reduce((acc, tweet) => ({ ...acc, [tweet.id]: tweet.likesCount }), {})
  );

  const handleDelete = async (tweetId: string) => {
    if (!token || !confirm('Are you sure you want to delete this tweet?')) {
      return;
    }

    setDeletingId(tweetId);
    try {
      await api.tweets.delete(token, tweetId);
      onTweetDeleted?.(tweetId);
    } catch (err: any) {
      alert(`Failed to delete tweet: ${err.message}`);
    } finally {
      setDeletingId(null);
    }
  };

  const handleLike = async (tweetId: string) => {
    if (!token) {
      alert('Please login to like tweets');
      return;
    }

    const isLiked = likedTweets.has(tweetId);
    
    setLikingId(tweetId);
    
    // Optimistic update
    setLikedTweets((prev) => {
      const newSet = new Set(prev);
      if (isLiked) {
        newSet.delete(tweetId);
      } else {
        newSet.add(tweetId);
      }
      return newSet;
    });

    setLikeCounts((prev) => ({
      ...prev,
      [tweetId]: isLiked ? prev[tweetId] - 1 : prev[tweetId] + 1,
    }));

    try {
      if (isLiked) {
        await api.tweets.unlike(token, tweetId);
      } else {
        await api.tweets.like(token, tweetId);
      }
    } catch (err: any) {
      // Revert optimistic update on error
      setLikedTweets((prev) => {
        const newSet = new Set(prev);
        if (isLiked) {
          newSet.add(tweetId);
        } else {
          newSet.delete(tweetId);
        }
        return newSet;
      });

      setLikeCounts((prev) => ({
        ...prev,
        [tweetId]: isLiked ? prev[tweetId] + 1 : prev[tweetId] - 1,
      }));

      alert(`Failed to ${isLiked ? 'unlike' : 'like'} tweet: ${err.message}`);
    } finally {
      setLikingId(null);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined,
    });
  };

  if (tweets.length === 0) {
    return (
      <div className="bg-gray-800 rounded-lg p-8 text-center text-gray-400">
        <p className="text-lg">No tweets yet.</p>
        <p className="text-sm mt-2">Be the first to tweet!</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {tweets.map((tweet) => (
        <div
          key={tweet.id}
          className="bg-gray-800 rounded-lg p-4 sm:p-6 hover:bg-gray-750 transition-colors"
        >
          {/* Header */}
          <div className="flex items-start justify-between mb-3">
            <div className="flex items-center gap-3 flex-1 min-w-0">
              <Link
                href={`/user/${tweet.username}`}
                className="w-10 h-10 sm:w-12 sm:h-12 rounded-full bg-gray-700 flex items-center justify-center text-lg font-bold hover:bg-gray-600 transition-colors flex-shrink-0"
              >
                {tweet.displayName.charAt(0).toUpperCase()}
              </Link>

              <div className="flex-1 min-w-0">
                <Link
                  href={`/user/${tweet.username}`}
                  className="hover:underline"
                >
                  <p className="font-semibold text-white truncate">
                    {tweet.displayName}
                  </p>
                  <p className="text-sm text-gray-400 truncate">
                    @{tweet.username}
                  </p>
                </Link>
              </div>

              <span className="text-sm text-gray-500 flex-shrink-0">
                {formatDate(tweet.createdAtUtc)}
              </span>
            </div>

            {/* Delete button */}
            {user && user.id === tweet.userId && (
              <button
                onClick={() => handleDelete(tweet.id)}
                disabled={deletingId === tweet.id}
                className="ml-2 text-red-400 hover:text-red-300 text-sm font-semibold disabled:opacity-50 flex-shrink-0"
                aria-label="Delete tweet"
              >
                {deletingId === tweet.id ? 'Deleting...' : '🗑️'}
              </button>
            )}
          </div>

          {/* Content */}
          <p className="text-white text-base sm:text-lg whitespace-pre-wrap break-words mb-3">
            {tweet.content}
          </p>

          {/* Footer */}
          <div className="flex items-center gap-6 text-sm text-gray-400">
            <button
              onClick={() => handleLike(tweet.id)}
              disabled={likingId === tweet.id}
              className={`flex items-center gap-2 transition-colors disabled:opacity-50 ${
                likedTweets.has(tweet.id)
                  ? 'text-red-500 hover:text-red-400'
                  : 'hover:text-red-500'
              }`}
              aria-label={likedTweets.has(tweet.id) ? 'Unlike tweet' : 'Like tweet'}
            >
              <span className="text-lg">
                {likedTweets.has(tweet.id) ? '❤️' : '🤍'}
              </span>
              <span>{likeCounts[tweet.id] || 0}</span>
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
