import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { api, LoginData, RegisterData, UserProfile } from '@/lib/api';

interface AuthState {
  token: string | null;
  user: UserProfile | null;
  isLoading: boolean;
  error: string | null;
  
  login: (data: LoginData) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  loadUser: () => Promise<void>;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      user: null,
      isLoading: false,
      error: null,

      login: async (data: LoginData) => {
        set({ isLoading: true, error: null });
        try {
          const response = await api.auth.login(data);
          set({ token: response.token, isLoading: false });
          
          // Load user profile after login
          await get().loadUser();
        } catch (error: any) {
          set({ 
            isLoading: false, 
            error: error.message || 'Login failed' 
          });
          throw error;
        }
      },

      register: async (data: RegisterData) => {
        set({ isLoading: true, error: null });
        try {
          await api.auth.register(data);
          
          // Auto-login after registration
          await get().login({
            username: data.username,
            password: data.password,
          });
        } catch (error: any) {
          set({ 
            isLoading: false, 
            error: error.message || 'Registration failed' 
          });
          throw error;
        }
      },

      logout: () => {
        set({ token: null, user: null, error: null });
      },

      loadUser: async () => {
        const { token } = get();
        if (!token) return;

        set({ isLoading: true, error: null });
        try {
          const user = await api.user.getMe(token);
          set({ user, isLoading: false });
        } catch (error: any) {
          set({ 
            isLoading: false, 
            error: error.message || 'Failed to load user',
            token: null, // Clear invalid token
            user: null,
          });
        }
      },

      clearError: () => set({ error: null }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ 
        token: state.token,
        user: state.user,
      }),
    }
  )
);
