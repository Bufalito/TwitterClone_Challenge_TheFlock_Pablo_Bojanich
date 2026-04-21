import { renderHook, act, waitFor } from '@testing-library/react';
import { useAuthStore } from '@/store/authStore';
import { api } from '@/lib/api';

// Mock the API
jest.mock('@/lib/api', () => ({
  api: {
    auth: {
      login: jest.fn(),
      register: jest.fn(),
    },
    user: {
      getMe: jest.fn(),
    },
  },
  ApiError: class ApiError extends Error {
    constructor(public status: number, message: string) {
      super(message);
    }
  },
}));

describe('Auth Store', () => {
  beforeEach(() => {
    // Reset store state before each test
    const { getState } = useAuthStore;
    act(() => {
      useAuthStore.setState({
        token: null,
        user: null,
        isLoading: false,
        error: null,
      });
    });
    jest.clearAllMocks();
  });

  describe('login', () => {
    it('should successfully login and load user', async () => {
      const mockLoginResponse = {
        token: 'test-token',
        username: 'testuser',
        email: 'test@example.com',
        displayName: 'Test User',
      };

      const mockUserProfile = {
        id: '123',
        username: 'testuser',
        email: 'test@example.com',
        displayName: 'Test User',
        createdAtUtc: '2026-04-20T00:00:00Z',
      };

      (api.auth.login as jest.Mock).mockResolvedValue(mockLoginResponse);
      (api.user.getMe as jest.Mock).mockResolvedValue(mockUserProfile);

      const { result } = renderHook(() => useAuthStore());

      await act(async () => {
        await result.current.login({
          username: 'testuser',
          password: 'password123',
        });
      });

      await waitFor(() => {
        expect(result.current.token).toBe('test-token');
        expect(result.current.user).toEqual(mockUserProfile);
        expect(result.current.error).toBeNull();
        expect(result.current.isLoading).toBe(false);
      });

      expect(api.auth.login).toHaveBeenCalledWith({
        username: 'testuser',
        password: 'password123',
      });
      expect(api.user.getMe).toHaveBeenCalledWith('test-token');
    });

    it('should handle login failure', async () => {
      const mockError = new Error('Invalid credentials');
      (api.auth.login as jest.Mock).mockRejectedValue(mockError);

      const { result } = renderHook(() => useAuthStore());

      await act(async () => {
        try {
          await result.current.login({
            username: 'testuser',
            password: 'wrongpassword',
          });
        } catch (error) {
          // Expected to throw
        }
      });

      await waitFor(() => {
        expect(result.current.token).toBeNull();
        expect(result.current.user).toBeNull();
        expect(result.current.error).toBe('Invalid credentials');
        expect(result.current.isLoading).toBe(false);
      });
    });
  });

  describe('register', () => {
    it('should successfully register and auto-login', async () => {
      const mockRegisterResponse = {
        token: 'test-token',
        username: 'newuser',
        email: 'new@example.com',
        displayName: 'New User',
      };

      const mockLoginResponse = {
        token: 'test-token',
        username: 'newuser',
        email: 'new@example.com',
        displayName: 'New User',
      };

      const mockUserProfile = {
        id: '456',
        username: 'newuser',
        email: 'new@example.com',
        displayName: 'New User',
        createdAtUtc: '2026-04-20T00:00:00Z',
      };

      (api.auth.register as jest.Mock).mockResolvedValue(mockRegisterResponse);
      (api.auth.login as jest.Mock).mockResolvedValue(mockLoginResponse);
      (api.user.getMe as jest.Mock).mockResolvedValue(mockUserProfile);

      const { result } = renderHook(() => useAuthStore());

      await act(async () => {
        await result.current.register({
          name: 'New User',
          username: 'newuser',
          email: 'new@example.com',
          password: 'password123',
        });
      });

      await waitFor(() => {
        expect(result.current.token).toBe('test-token');
        expect(result.current.user).toEqual(mockUserProfile);
      });

      expect(api.auth.register).toHaveBeenCalled();
      expect(api.auth.login).toHaveBeenCalledWith({
        username: 'newuser',
        password: 'password123',
      });
    });
  });

  describe('logout', () => {
    it('should clear token and user', () => {
      const { result } = renderHook(() => useAuthStore());

      // Set initial state
      act(() => {
        useAuthStore.setState({
          token: 'test-token',
          user: {
            id: '123',
            username: 'testuser',
            email: 'test@example.com',
            displayName: 'Test User',
            createdAtUtc: '2026-04-20T00:00:00Z',
          },
        });
      });

      act(() => {
        result.current.logout();
      });

      expect(result.current.token).toBeNull();
      expect(result.current.user).toBeNull();
      expect(result.current.error).toBeNull();
    });
  });

  describe('clearError', () => {
    it('should clear error state', () => {
      const { result } = renderHook(() => useAuthStore());

      // Set error
      act(() => {
        useAuthStore.setState({ error: 'Test error' });
      });

      expect(result.current.error).toBe('Test error');

      act(() => {
        result.current.clearError();
      });

      expect(result.current.error).toBeNull();
    });
  });
});
