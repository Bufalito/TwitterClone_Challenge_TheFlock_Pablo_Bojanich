'use client';

import { useEffect, useState, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { api, TweetResponse } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';
import LeftSidebar from '@/components/LeftSidebar';
import RightSidebar from '@/components/RightSidebar';

function formatDate(dateString: string) {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'Justo ahora';
  if (diffMins < 60) return `${diffMins}m`;
  if (diffHours < 24) return `${diffHours}h`;
  if (diffDays < 7) return `${diffDays}d`;
  return date.toLocaleDateString('es-AR', { month: 'short', day: 'numeric' });
}

interface TweetCardProps {
  tweet: TweetResponse;
  isRoot?: boolean;
  onLikeToggle: (tweetId: string, liked: boolean) => void;
  likedTweets: Set<string>;
  likeCounts: Record<string, number>;
  onReplyClick?: () => void;
  onNavigate?: (tweetId: string) => void;
}

function TweetCard({ tweet, isRoot, onLikeToggle, likedTweets, likeCounts, onReplyClick, onNavigate }: TweetCardProps) {
  const { token } = useAuthStore();
  const [likingId, setLikingId] = useState<string | null>(null);

  const handleLike = async (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!token) return;
    const isLiked = likedTweets.has(tweet.id);
    setLikingId(tweet.id);
    onLikeToggle(tweet.id, !isLiked);
    try {
      if (isLiked) {
        await api.tweets.unlike(token, tweet.id);
      } else {
        await api.tweets.like(token, tweet.id);
      }
    } catch {
      onLikeToggle(tweet.id, isLiked);
    } finally {
      setLikingId(null);
    }
  };

  return (
    <div
      className={`p-4 border-b border-gray-800 ${!isRoot ? 'hover:bg-gray-800/50 transition-colors cursor-pointer' : ''}`}
      onClick={!isRoot ? () => onNavigate?.(tweet.id) : undefined}
    >
      <div className="flex gap-3">
        <Link href={`/user/${tweet.username}`} className="shrink-0" onClick={e => e.stopPropagation()}>
          <div className="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold hover:opacity-80 transition-opacity">
            {tweet.displayName.charAt(0).toUpperCase()}
          </div>
        </Link>

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <Link href={`/user/${tweet.username}`} className="font-bold text-white hover:underline" onClick={e => e.stopPropagation()}>
              {tweet.displayName}
            </Link>
            <Link href={`/user/${tweet.username}`} className="text-gray-500 hover:underline text-sm" onClick={e => e.stopPropagation()}>
              @{tweet.username}
            </Link>
            <span className="text-gray-500 text-sm">·</span>
            <span className="text-gray-500 text-sm">{formatDate(tweet.createdAtUtc)}</span>
          </div>

          <p className={`text-white whitespace-pre-wrap wrap-break-word mt-1 ${isRoot ? 'text-xl leading-relaxed' : 'text-base'}`}>
            {tweet.content}
          </p>

          {isRoot && (
            <p className="text-gray-500 text-sm mt-3 pb-3 border-b border-gray-800">
              {new Date(tweet.createdAtUtc).toLocaleString('es-AR', {
                hour: '2-digit', minute: '2-digit',
                day: 'numeric', month: 'short', year: 'numeric',
              })}
            </p>
          )}

          <div className="flex items-center gap-8 mt-3 text-gray-500">
            <button
              onClick={(e) => { e.stopPropagation(); onReplyClick?.(); }}
              className="flex items-center gap-2 group hover:text-blue-400 transition-colors"
              aria-label="Responder"
            >
              <span className="text-xl group-hover:bg-blue-400/10 rounded-full p-1.5 transition">💬</span>
              <span className="text-sm">{tweet.repliesCount ?? 0}</span>
            </button>

            <button
              onClick={handleLike}
              disabled={likingId === tweet.id}
              className={`flex items-center gap-2 group transition-colors disabled:opacity-50 ${
                likedTweets.has(tweet.id) ? 'text-pink-600' : 'hover:text-pink-600'
              }`}
              aria-label={likedTweets.has(tweet.id) ? 'Quitar like' : 'Dar like'}
            >
              <span className={`text-xl rounded-full p-1.5 transition ${likedTweets.has(tweet.id) ? '' : 'group-hover:bg-pink-600/10'}`}>
                {likedTweets.has(tweet.id) ? '❤️' : '🤍'}
              </span>
              <span className="text-sm">{likeCounts[tweet.id] ?? 0}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}


export default function TweetThreadPage() {
  const params = useParams();
  const router = useRouter();
  const { token, user } = useAuthStore();
  const tweetId = params.id as string;

  const [tweet, setTweet] = useState<TweetResponse | null>(null);
  const [replies, setReplies] = useState<TweetResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [replyContent, setReplyContent] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const [likedTweets, setLikedTweets] = useState<Set<string>>(new Set());
  const [likeCounts, setLikeCounts] = useState<Record<string, number>>({});

  const loadThread = useCallback(async () => {
    try {
      const [tweetData, repliesData] = await Promise.all([
        api.tweets.getById(tweetId, token ?? undefined),
        api.tweets.getReplies(tweetId, token ?? undefined),
      ]);
      setTweet(tweetData);
      setReplies(repliesData);
      const allTweets = [tweetData, ...repliesData];
      setLikedTweets(new Set(allTweets.filter(t => t.isLikedByCurrentUser).map(t => t.id)));
      setLikeCounts(allTweets.reduce((acc, t) => ({ ...acc, [t.id]: t.likesCount }), {}));
    } catch {
      router.push('/dashboard');
    } finally {
      setLoading(false);
    }
  }, [tweetId, token, router]);

  useEffect(() => {
    loadThread();
  }, [loadThread]);

  const handleLikeToggle = (id: string, liked: boolean) => {
    setLikedTweets(prev => {
      const next = new Set(prev);
      liked ? next.add(id) : next.delete(id);
      return next;
    });
    setLikeCounts(prev => ({
      ...prev,
      [id]: liked ? (prev[id] ?? 0) + 1 : Math.max(0, (prev[id] ?? 0) - 1),
    }));
  };

  const handleReply = async () => {
    if (!token || !replyContent.trim() || submitting) return;
    setSubmitting(true);
    try {
      const newReply = await api.tweets.create(token, {
        content: replyContent.trim(),
        parentTweetId: tweetId,
      });
      setReplyContent('');
      setReplies(prev => [...prev, newReply]);
      setLikeCounts(prev => ({ ...prev, [newReply.id]: 0 }));
      setTweet(prev => prev ? { ...prev, repliesCount: (prev.repliesCount ?? 0) + 1 } : prev);
    } catch (err: any) {
      alert(`Error al responder: ${err.message}`);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <div className="container mx-auto flex max-w-7xl">
        {/* Left Sidebar */}
        <div className="hidden md:flex md:w-20 lg:w-64 xl:w-72 border-r border-gray-800 sticky top-0 h-screen">
          <div className="w-full">
            <LeftSidebar />
          </div>
        </div>

        {/* Main Content */}
        <div className="flex-1 min-w-0 border-r border-gray-800">
          {/* Header */}
          <div className="sticky top-0 z-10 border-b border-gray-800 bg-gray-900/95 backdrop-blur">
            <div className="flex h-14 items-center gap-4 px-4">
              <button
                onClick={() => router.back()}
                className="text-white hover:bg-gray-800 rounded-full p-2 transition-colors"
                aria-label="Volver"
              >
                ←
              </button>
              <h1 className="text-xl font-bold">Hilo</h1>
            </div>
          </div>

          {loading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
          ) : tweet ? (
            <>
              {/* Tweet raíz */}
              <TweetCard
                tweet={tweet}
                isRoot
                onLikeToggle={handleLikeToggle}
                likedTweets={likedTweets}
                likeCounts={likeCounts}
                onReplyClick={() => document.getElementById('reply-input')?.focus()}
              />

              {/* Composer de respuesta */}
              {user ? (
                <div className="border-b border-gray-800 p-4 flex gap-3">
                  <div className="w-12 h-12 rounded-full bg-blue-500 shrink-0 flex items-center justify-center text-white font-bold">
                    {user.displayName?.charAt(0).toUpperCase()}
                  </div>
                  <div className="flex-1">
                    <textarea
                      id="reply-input"
                      value={replyContent}
                      onChange={e => setReplyContent(e.target.value)}
                      placeholder={`Responder a @${tweet.username}...`}
                      maxLength={280}
                      rows={3}
                      className="w-full bg-transparent text-white placeholder-gray-500 resize-none outline-none text-lg"
                    />
                    <div className="flex items-center justify-between mt-2">
                      <span className={`text-sm ${replyContent.length > 260 ? 'text-red-400' : 'text-gray-500'}`}>
                        {280 - replyContent.length}
                      </span>
                      <button
                        onClick={handleReply}
                        disabled={!replyContent.trim() || submitting}
                        className="bg-blue-500 hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold py-1.5 px-5 rounded-full transition-colors text-sm"
                      >
                        {submitting ? 'Enviando...' : 'Responder'}
                      </button>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="border-b border-gray-800 p-4 text-center">
                  <p className="text-gray-500 text-sm mb-2">Iniciá sesión para responder</p>
                  <Link href="/login" className="text-blue-400 hover:underline text-sm font-medium">
                    Iniciar sesión
                  </Link>
                </div>
              )}

              {/* Replies */}
              {replies.length === 0 ? (
                <div className="p-8 text-center text-gray-500">
                  <div className="text-4xl mb-3">💬</div>
                  <p className="font-bold text-white">Todavía no hay respuestas</p>
                  <p className="text-sm mt-1">¡Sé el primero en responder!</p>
                </div>
              ) : (
                replies.map(reply => (
                  <TweetCard
                    key={reply.id}
                    tweet={reply}
                    onLikeToggle={handleLikeToggle}
                    likedTweets={likedTweets}
                    likeCounts={likeCounts}
                    onReplyClick={() => router.push(`/tweet/${reply.id}`)}
                    onNavigate={(id) => router.push(`/tweet/${id}`)}
                  />
                ))
              )}
            </>
          ) : null}
        </div>

        {/* Right Sidebar */}
        <div className="hidden lg:block lg:w-80 xl:w-96">
          <div className="sticky top-0">
            <RightSidebar />
          </div>
        </div>
      </div>

      {/* Mobile Bottom Navigation */}
      <div className="fixed bottom-0 left-0 right-0 border-t border-gray-800 bg-gray-900 md:hidden">
        <div className="flex items-center justify-around py-3">
          <button onClick={() => router.push('/dashboard')} className="flex flex-col items-center gap-1 text-gray-500">
            <span className="text-2xl">🏠</span>
          </button>
          {user?.username && (
            <button onClick={() => router.push(`/user/${user.username}`)} className="flex flex-col items-center gap-1 text-gray-500">
              <span className="text-2xl">👤</span>
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
