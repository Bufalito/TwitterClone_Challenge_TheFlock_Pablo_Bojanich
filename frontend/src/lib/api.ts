const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:8080';

export interface RegisterData {
  name: string;
  username: string;
  email: string;
  password: string;
  bio?: string;
  avatar?: string;
}

export interface LoginData {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  email: string;
  displayName: string;
}

export interface UserProfile {
  id: string;
  username: string;
  email: string;
  displayName: string;
  bio?: string;
  avatar?: string;
  followersCount?: number;
  followingCount?: number;
  tweetsCount?: number;
  createdAtUtc: string;
  isFollowedByCurrentUser: boolean;
}

export interface UserSearchResult {
  id: string;
  username: string;
  displayName: string;
  bio?: string;
  avatar?: string;
  followersCount: number;
}

export interface TweetResponse {
  id: string;
  userId: string;
  content: string;
  createdAtUtc: string;
  username: string;
  displayName: string;
  likesCount: number;
  isLikedByCurrentUser: boolean;
}

export interface CreateTweetRequest {
  content: string;
}

export interface TrendingHashtag {
  hashtag: string;
  count: number;
}

class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}

async function fetchApi<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  const url = `${API_URL}${endpoint}`;
  
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ error: 'Unknown error' }));
    throw new ApiError(
      response.status,
      errorData.error || `HTTP ${response.status}`
    );
  }

  return response.json();
}

export const api = {
  auth: {
    register: (data: RegisterData) =>
      fetchApi<LoginResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    login: (data: LoginData) =>
      fetchApi<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
  },

  user: {
    getMe: (token: string) =>
      fetchApi<UserProfile>('/api/user/me', {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),

    getByUsername: (username: string, token?: string) =>
      fetchApi<UserProfile>(`/api/user/${username}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
      }),

    search: (query: string) =>
      fetchApi<UserSearchResult[]>(`/api/user/search?q=${encodeURIComponent(query)}`),

    follow: (token: string, userId: string) =>
      fetchApi<{ message: string }>(`/api/user/${userId}/follow`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),

    unfollow: (token: string, userId: string) =>
      fetchApi<{ message: string }>(`/api/user/${userId}/follow`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),

    getSuggestions: (limit: number = 3, token?: string) =>
      fetchApi<UserSearchResult[]>(`/api/user/suggestions?limit=${limit}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
      }),
  },

  tweets: {
    create: (token: string, data: CreateTweetRequest) =>
      fetchApi<TweetResponse>('/api/tweets', {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(data),
      }),

    delete: (token: string, tweetId: string) =>
      fetchApi<void>(`/api/tweets/${tweetId}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),

    getRecent: (count: number = 20, token?: string) =>
      fetchApi<TweetResponse[]>(`/api/tweets?count=${count}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
      }),

    getByUser: (username: string, token?: string) =>
      fetchApi<TweetResponse[]>(`/api/tweets/user/${username}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
      }),

    getTimeline: (token: string, page: number = 1, pageSize: number = 20) =>
      fetchApi<TweetResponse[]>(`/api/tweets/timeline?page=${page}&pageSize=${pageSize}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),

    like: (token: string, tweetId: string) =>
      fetchApi<{ message: string }>(`/api/tweets/${tweetId}/like`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),

    unlike: (token: string, tweetId: string) =>
      fetchApi<{ message: string }>(`/api/tweets/${tweetId}/like`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),

    getTrending: (limit: number = 5) =>
      fetchApi<TrendingHashtag[]>(`/api/tweets/trending?limit=${limit}`),
  },
};

export { ApiError };
