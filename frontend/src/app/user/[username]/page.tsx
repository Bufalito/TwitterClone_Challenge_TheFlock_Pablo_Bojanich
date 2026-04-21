'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { api, UserProfile, TweetResponse } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';
import TweetList from '@/components/TweetList';

export default function UserProfilePage() {
  const params = useParams();
  const router = useRouter();
  const username = params.username as string;
  const { token } = useAuthStore();

  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [tweets, setTweets] = useState<TweetResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [tweetsLoading, setTweetsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [followLoading, setFollowLoading] = useState(false);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await api.user.getByUsername(username, token || undefined);
        setProfile(data);
      } catch (err: any) {
        setError(err.message || 'Failed to load profile');
      } finally {
        setLoading(false);
      }
    };

    const fetchTweets = async () => {
      try {
        setTweetsLoading(true);
        const data = await api.tweets.getByUser(username, token || undefined);
        setTweets(data);
      } catch (err: any) {
        console.error('Failed to load tweets:', err);
      } finally {
        setTweetsLoading(false);
      }
    };

    if (username) {
      fetchProfile();
      fetchTweets();
    }
  }, [username, token]);

  const handleFollowToggle = async () => {
    if (!token || !profile) return;

    try {
      setFollowLoading(true);
      
      if (profile.isFollowedByCurrentUser) {
        await api.user.unfollow(token, profile.id);
        setProfile({
          ...profile,
          isFollowedByCurrentUser: false,
          followersCount: (profile.followersCount || 0) - 1,
        });
      } else {
        await api.user.follow(token, profile.id);
        setProfile({
          ...profile,
          isFollowedByCurrentUser: true,
          followersCount: (profile.followersCount || 0) + 1,
        });
      }
    } catch (err: any) {
      console.error('Failed to follow/unfollow:', err);
    } finally {
      setFollowLoading(false);
    }
  };

  const handleTweetDeleted = (tweetId: string) => {
    setTweets(tweets.filter((t) => t.id !== tweetId));
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4"></div>
          <p className="text-gray-400">Loading profile...</p>
        </div>
      </div>
    );
  }

  if (error || !profile) {
    return (
      <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center p-4">
        <div className="max-w-md w-full bg-gray-800 rounded-lg p-6 text-center">
          <h1 className="text-2xl font-bold mb-4">User Not Found</h1>
          <p className="text-gray-400 mb-6">{error || `User @${username} does not exist.`}</p>
          <button
            onClick={() => router.push('/')}
            className="bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-6 rounded-lg transition-colors"
          >
            Go Home
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <div className="max-w-4xl mx-auto p-4 sm:p-6 lg:p-8">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={() => router.back()}
            className="text-blue-400 hover:text-blue-300 mb-4 flex items-center gap-2"
          >
            ← Back
          </button>
        </div>

        {/* Profile Card */}
        <div className="bg-gray-800 rounded-lg p-6 sm:p-8 mb-6">
          {/* Avatar */}
          <div className="flex flex-col sm:flex-row items-center sm:items-start gap-6 mb-6">
            <div className="w-24 h-24 sm:w-32 sm:h-32 rounded-full bg-gray-700 flex items-center justify-center text-4xl sm:text-5xl font-bold">
              {profile.avatar || profile.displayName.charAt(0).toUpperCase()}
            </div>
            
            <div className="flex-1 text-center sm:text-left">
              <h1 className="text-2xl sm:text-3xl font-bold mb-2">{profile.displayName}</h1>
              <p className="text-gray-400 text-lg mb-4">@{profile.username}</p>
              
              {/* Follow Button */}
              {token && (
                <div className="mb-4">
                  <button
                    onClick={handleFollowToggle}
                    disabled={followLoading}
                    className={`px-6 py-2 rounded-full font-semibold transition-colors ${
                      profile.isFollowedByCurrentUser
                        ? 'bg-gray-700 hover:bg-gray-600 text-white'
                        : 'bg-blue-500 hover:bg-blue-600 text-white'
                    } disabled:opacity-50 disabled:cursor-not-allowed`}
                  >
                    {followLoading ? 'Loading...' : profile.isFollowedByCurrentUser ? 'Following' : 'Follow'}
                  </button>
                </div>
              )}
              
              {/* Stats */}
              <div className="flex gap-6 justify-center sm:justify-start text-sm">
                <div>
                  <span className="font-bold text-white">{profile.tweetsCount || 0}</span>
                  <span className="text-gray-400 ml-1">Tweets</span>
                </div>
                <div>
                  <span className="font-bold text-white">{profile.followingCount || 0}</span>
                  <span className="text-gray-400 ml-1">Following</span>
                </div>
                <div>
                  <span className="font-bold text-white">{profile.followersCount || 0}</span>
                  <span className="text-gray-400 ml-1">Followers</span>
                </div>
              </div>
            </div>
          </div>

          {/* Bio */}
          {profile.bio && (
            <div className="mb-6">
              <p className="text-gray-300 text-base sm:text-lg">{profile.bio}</p>
            </div>
          )}

          {/* Joined Date */}
          <div className="text-gray-400 text-sm">
            Joined {new Date(profile.createdAtUtc).toLocaleDateString('en-US', {
              month: 'long',
              year: 'numeric'
            })}
          </div>
        </div>

        {/* Tweets Section */}
        <div>
          <h2 className="text-xl font-bold mb-4">Tweets</h2>
          
          {tweetsLoading ? (
            <div className="flex justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
          ) : (
            <TweetList tweets={tweets} onTweetDeleted={handleTweetDeleted} />
          )}
        </div>
      </div>
    </div>
  );
}
