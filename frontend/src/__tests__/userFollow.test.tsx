import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { useRouter, useParams } from 'next/navigation';
import UserProfilePage from '@/app/user/[username]/page';
import { api } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
  useParams: jest.fn(),
}));

jest.mock('@/lib/api', () => ({
  api: {
    user: {
      getByUsername: jest.fn(),
      follow: jest.fn(),
      unfollow: jest.fn(),
    },
    tweets: {
      getByUser: jest.fn(),
    },
  },
}));

jest.mock('@/store/authStore', () => ({
  useAuthStore: jest.fn(),
}));

jest.mock('@/components/TweetList', () => {
  return function TweetList() {
    return <div>Mock TweetList</div>;
  };
});

describe('UserProfilePage - Follow Functionality', () => {
  const mockPush = jest.fn();
  const mockBack = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      push: mockPush,
      back: mockBack,
    });
    
    (useParams as jest.Mock).mockReturnValue({ username: 'testuser' });
  });

  it('should display follow button when not following', async () => {
    const mockProfile = {
      id: 'user-1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      followersCount: 10,
      followingCount: 5,
      tweetsCount: 20,
      createdAtUtc: new Date().toISOString(),
      isFollowedByCurrentUser: false,
    };

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
    });

    (api.user.getByUsername as jest.Mock).mockResolvedValue(mockProfile);
    (api.tweets.getByUser as jest.Mock).mockResolvedValue([]);

    render(<UserProfilePage />);

    await waitFor(() => {
      expect(screen.getByText('Follow')).toBeInTheDocument();
    });
  });

  it('should display following button when already following', async () => {
    const mockProfile = {
      id: 'user-1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      followersCount: 10,
      followingCount: 5,
      tweetsCount: 20,
      createdAtUtc: new Date().toISOString(),
      isFollowedByCurrentUser: true,
    };

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
    });

    (api.user.getByUsername as jest.Mock).mockResolvedValue(mockProfile);
    (api.tweets.getByUser as jest.Mock).mockResolvedValue([]);

    render(<UserProfilePage />);

    await waitFor(() => {
      const followButton = screen.getByRole('button', { name: /following/i });
      expect(followButton).toBeInTheDocument();
    });
  });

  it('should not display follow button when not authenticated', async () => {
    const mockProfile = {
      id: 'user-1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      followersCount: 10,
      followingCount: 5,
      tweetsCount: 20,
      createdAtUtc: new Date().toISOString(),
      isFollowedByCurrentUser: false,
    };

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: null,
    });

    (api.user.getByUsername as jest.Mock).mockResolvedValue(mockProfile);
    (api.tweets.getByUser as jest.Mock).mockResolvedValue([]);

    render(<UserProfilePage />);

    await waitFor(() => {
      expect(screen.getByText('Test User')).toBeInTheDocument();
    });

    expect(screen.queryByRole('button', { name: /follow$/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /following/i })).not.toBeInTheDocument();
  });

  it('should follow user when follow button clicked', async () => {
    const mockProfile = {
      id: 'user-1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      followersCount: 10,
      followingCount: 5,
      tweetsCount: 20,
      createdAtUtc: new Date().toISOString(),
      isFollowedByCurrentUser: false,
    };

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
    });

    (api.user.getByUsername as jest.Mock).mockResolvedValue(mockProfile);
    (api.tweets.getByUser as jest.Mock).mockResolvedValue([]);
    (api.user.follow as jest.Mock).mockResolvedValue({ message: 'Success' });

    render(<UserProfilePage />);

    await waitFor(() => {
      const followButton = screen.getByRole('button', { name: /follow$/i });
      expect(followButton).toBeInTheDocument();
    });

    const followButton = screen.getByRole('button', { name: /follow$/i });
    fireEvent.click(followButton);

    await waitFor(() => {
      expect(api.user.follow).toHaveBeenCalledWith('test-token', 'user-1');
      const followingButton = screen.getByRole('button', { name: /following/i });
      expect(followingButton).toBeInTheDocument();
    });

    // Check followers count increased
    expect(screen.getByText('11')).toBeInTheDocument();
  });

  it('should unfollow user when following button clicked', async () => {
    const mockProfile = {
      id: 'user-1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      followersCount: 10,
      followingCount: 5,
      tweetsCount: 20,
      createdAtUtc: new Date().toISOString(),
      isFollowedByCurrentUser: true,
    };

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
    });

    (api.user.getByUsername as jest.Mock).mockResolvedValue(mockProfile);
    (api.tweets.getByUser as jest.Mock).mockResolvedValue([]);
    (api.user.unfollow as jest.Mock).mockResolvedValue({ message: 'Success' });

    render(<UserProfilePage />);

    await waitFor(() => {
      const followingButton = screen.getByRole('button', { name: /following/i });
      expect(followingButton).toBeInTheDocument();
    });

    const followingButton = screen.getByRole('button', { name: /following/i });
    fireEvent.click(followingButton);

    await waitFor(() => {
      expect(api.user.unfollow).toHaveBeenCalledWith('test-token', 'user-1');
      const followButton = screen.getByRole('button', { name: /follow$/i });
      expect(followButton).toBeInTheDocument();
    });

    // Check followers count decreased
    expect(screen.getByText('9')).toBeInTheDocument();
  });

  it('should disable button during follow operation', async () => {
    const mockProfile = {
      id: 'user-1',
      username: 'testuser',
      displayName: 'Test User',
      email: 'test@example.com',
      followersCount: 10,
      followingCount: 5,
      tweetsCount: 20,
      createdAtUtc: new Date().toISOString(),
      isFollowedByCurrentUser: false,
    };

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
    });

    (api.user.getByUsername as jest.Mock).mockResolvedValue(mockProfile);
    (api.tweets.getByUser as jest.Mock).mockResolvedValue([]);
    
    let resolveFollow: any;
    (api.user.follow as jest.Mock).mockImplementation(() => {
      return new Promise((resolve) => {
        resolveFollow = resolve;
      });
    });

    render(<UserProfilePage />);

    await waitFor(() => {
      const followButton = screen.getByRole('button', { name: /follow$/i });
      expect(followButton).toBeInTheDocument();
    });

    const followButton = screen.getByRole('button', { name: /follow$/i }) as HTMLButtonElement;
    fireEvent.click(followButton);

    await waitFor(() => {
      const loadingButton = screen.getByRole('button', { name: /loading/i }) as HTMLButtonElement;
      expect(loadingButton).toBeInTheDocument();
      expect(loadingButton.disabled).toBe(true);
    });

    resolveFollow({ message: 'Success' });

    await waitFor(() => {
      const followingButton = screen.getByRole('button', { name: /following/i });
      expect(followingButton).toBeInTheDocument();
    });
  });
});
