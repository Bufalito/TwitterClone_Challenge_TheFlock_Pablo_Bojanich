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
}

export interface UserSearchResult {
  id: string;
  username: string;
  displayName: string;
  bio?: string;
  avatar?: string;
  followersCount: number;
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

    getByUsername: (username: string) =>
      fetchApi<UserProfile>(`/api/user/${username}`),

    search: (query: string) =>
      fetchApi<UserSearchResult[]>(`/api/user/search?q=${encodeURIComponent(query)}`),
  },
};

export { ApiError };
