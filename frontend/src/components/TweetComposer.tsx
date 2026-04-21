'use client';

import { useState } from 'react';
import { api, TweetResponse } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';

interface TweetComposerProps {
  onTweetCreated?: (tweet: TweetResponse) => void;
}

export default function TweetComposer({ onTweetCreated }: TweetComposerProps) {
  const { token } = useAuthStore();
  const [content, setContent] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const maxLength = 280;
  const remainingChars = maxLength - content.length;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!token) {
      setError('You must be logged in to tweet');
      return;
    }

    if (content.trim().length === 0) {
      setError('Tweet cannot be empty');
      return;
    }

    if (content.length > maxLength) {
      setError(`Tweet cannot exceed ${maxLength} characters`);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const tweet = await api.tweets.create(token, { content });
      setContent('');
      onTweetCreated?.(tweet);
    } catch (err: any) {
      setError(err.message || 'Failed to post tweet');
    } finally {
      setLoading(false);
    }
  };

  if (!token) {
    return null;
  }

  return (
    <div className="p-4">
      <form onSubmit={handleSubmit}>
        <div className="flex gap-4">
          <div className="flex-shrink-0">
            <div className="h-12 w-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold">
              {/* User avatar placeholder */}
              👤
            </div>
          </div>
          <div className="flex-1">
            <textarea
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="What's happening?"
              className="w-full bg-transparent text-white text-xl placeholder-gray-500 focus:outline-none resize-none border-none"
              rows={3}
              maxLength={maxLength}
            />

            <div className="flex items-center justify-between mt-4 pt-4 border-t border-gray-800">
              <div className="flex items-center gap-2">
                <span
                  className={`text-sm ${
                    remainingChars < 20
                      ? 'text-red-400'
                      : remainingChars < 50
                      ? 'text-yellow-400'
                      : 'text-gray-500'
                  }`}
                >
                  {remainingChars} characters left
                </span>
              </div>

              <button
                type="submit"
                disabled={loading || content.trim().length === 0 || content.length > maxLength}
                className="bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-6 rounded-full transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Posting...' : 'Tweet'}
              </button>
            </div>

            {error && (
              <div className="mt-4 p-3 bg-red-900/20 border border-red-700 rounded-lg text-red-400 text-sm">
                {error}
              </div>
            )}
          </div>
        </div>
      </form>
    </div>
  );
}
