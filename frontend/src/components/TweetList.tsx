'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { TweetResponse } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';
import { api } from '@/lib/api';

interface TweetListProps {
  tweets: TweetResponse[];
  onTweetDeleted?: (tweetId: string) => void;
}

export default function TweetList({ tweets, onTweetDeleted }: TweetListProps) {
  const router = useRouter();
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
      <div className="text-center py-12 text-gray-500">
        <p className="text-lg">No tweets yet.</p>
        <p className="text-sm mt-2">Be the first to tweet!</p>
      </div>
    );
  }

  return (
    <div>
      {tweets.map((tweet) => (
        <div
          key={tweet.id}
          className="border-b border-gray-800 p-4 hover:bg-gray-800/50 transition-colors cursor-pointer"
          onClick={() => router.push(`/tweet/${tweet.id}`)}
        >
          <div className="flex gap-3">
            {/* Avatar */}
            <Link
              href={`/user/${tweet.username}`}
              className="shrink-0"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold hover:opacity-80 transition-opacity">
                {tweet.displayName.charAt(0).toUpperCase()}
              </div>
            </Link>

            {/* Content */}
            <div className="flex-1 min-w-0">
              {/* Header */}
              <div className="flex items-center gap-2 mb-1">
                <Link
                  href={`/user/${tweet.username}`}
                  className="font-bold text-white hover:underline truncate"
                  onClick={(e) => e.stopPropagation()}
                >
                  {tweet.displayName}
                </Link>
                <Link
                  href={`/user/${tweet.username}`}
                  className="text-gray-500 hover:underline truncate"
                  onClick={(e) => e.stopPropagation()}
                >
                  @{tweet.username}
                </Link>
                <span className="text-gray-500">·</span>
                <span className="text-gray-500 text-sm shrink-0">
                  {formatDate(tweet.createdAtUtc)}
                </span>

                {/* Delete button */}
                {user && user.id === tweet.userId && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      handleDelete(tweet.id);
                    }}
                    disabled={deletingId === tweet.id}
                    className="ml-auto text-gray-500 hover:text-red-400 transition-colors disabled:opacity-50"
                    aria-label="Delete tweet"
                  >
                    {deletingId === tweet.id ? '⏳' : '🗑️'}
                  </button>
                )}
              </div>

              {/* Tweet Content */}
              <p className="text-white text-base whitespace-pre-wrap wrap-break-word mb-3">
                {tweet.content}
              </p>

              {/* Actions */}
              <div className="flex items-center gap-8 text-gray-500">
                {/* Reply */}
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    router.push(`/tweet/${tweet.id}`);
                  }}
                  className="flex items-center gap-2 group hover:text-blue-400 transition-colors"
                  aria-label="Reply to tweet"
                >
                  <span className="text-xl group-hover:bg-blue-400/10 rounded-full p-1.5 transition">
                    💬
                  </span>
                  <span className="text-sm">{tweet.repliesCount ?? 0}</span>
                </button>

                {/* Like */}
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    handleLike(tweet.id);
                  }}
                  disabled={likingId === tweet.id}
                  className={`flex items-center gap-2 group transition-colors disabled:opacity-50 ${
                    likedTweets.has(tweet.id)
                      ? 'text-pink-600'
                      : 'hover:text-pink-600'
                  }`}
                  aria-label={likedTweets.has(tweet.id) ? 'Unlike tweet' : 'Like tweet'}
                >
                  <span className={`text-xl ${likedTweets.has(tweet.id) ? '' : 'group-hover:bg-pink-600/10'} rounded-full p-1.5 transition`}>
                    {likedTweets.has(tweet.id) ? '❤️' : '🤍'}
                  </span>
                  <span className="text-sm">
                    {likeCounts[tweet.id] || 0}
                  </span>
                </button>
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
