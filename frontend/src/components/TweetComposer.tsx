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
    <div className="bg-gray-800 rounded-lg p-4 sm:p-6 mb-6">
      <form onSubmit={handleSubmit}>
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="What's happening?"
          className="w-full bg-gray-900 text-white border border-gray-700 rounded-lg p-4 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
          rows={3}
          maxLength={maxLength}
        />

        <div className="flex items-center justify-between mt-4">
          <div className="flex items-center gap-4">
            <span
              className={`text-sm ${
                remainingChars < 20
                  ? 'text-red-400'
                  : remainingChars < 50
                  ? 'text-yellow-400'
                  : 'text-gray-400'
              }`}
            >
              {remainingChars} characters left
            </span>
          </div>

          <button
            type="submit"
            disabled={loading || content.trim().length === 0 || content.length > maxLength}
            className="bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-6 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Posting...' : 'Tweet'}
          </button>
        </div>

        {error && (
          <div className="mt-4 p-3 bg-red-900/20 border border-red-700 rounded-lg text-red-400 text-sm">
            {error}
          </div>
        )}
      </form>
    </div>
  );
}
