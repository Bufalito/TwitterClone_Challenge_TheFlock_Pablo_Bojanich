import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useRouter } from 'next/navigation';
import TweetList from '@/components/TweetList';
import { api, TweetResponse } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';

// Mock dependencies
jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

jest.mock('@/lib/api', () => ({
  api: {
    tweets: {
      delete: jest.fn(),
    },
  },
}));

jest.mock('@/store/authStore', () => ({
  useAuthStore: jest.fn(),
}));

describe('TweetList', () => {
  const mockTweets: TweetResponse[] = [
    {
      id: '1',
      userId: 'user-1',
      content: 'First tweet',
      createdAtUtc: new Date().toISOString(),
      username: 'testuser',
      displayName: 'Test User',
      likesCount: 5,
      isLikedByCurrentUser: false,
    },
    {
      id: '2',
      userId: 'user-2',
      content: 'Second tweet',
      createdAtUtc: new Date(Date.now() - 3600000).toISOString(), // 1 hour ago
      username: 'anotheruser',
      displayName: 'Another User',
      likesCount: 3,
      isLikedByCurrentUser: false,
    },
  ];

  const mockOnTweetDeleted = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      push: jest.fn(),
    });
    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'mock-token',
      user: { id: 'user-1', username: 'testuser' },
    });
  });

  it('renders list of tweets', () => {
    render(<TweetList tweets={mockTweets} onTweetDeleted={mockOnTweetDeleted} />);

    expect(screen.getByText('First tweet')).toBeInTheDocument();
    expect(screen.getByText('Second tweet')).toBeInTheDocument();
    expect(screen.getByText('Test User')).toBeInTheDocument();
    expect(screen.getByText('@testuser')).toBeInTheDocument();
  });

  it('shows empty state when no tweets', () => {
    render(<TweetList tweets={[]} onTweetDeleted={mockOnTweetDeleted} />);

    expect(screen.getByText('No tweets yet.')).toBeInTheDocument();
    expect(screen.getByText('Be the first to tweet!')).toBeInTheDocument();
  });

  it('displays like counts', () => {
    render(<TweetList tweets={mockTweets} onTweetDeleted={mockOnTweetDeleted} />);

    expect(screen.getByText('5')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('shows delete button only for own tweets', () => {
    render(<TweetList tweets={mockTweets} onTweetDeleted={mockOnTweetDeleted} />);

    const deleteButtons = screen.getAllByLabelText('Delete tweet');
    expect(deleteButtons).toHaveLength(1);
  });

  it('does not show delete button when not authenticated', () => {
    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: null,
      user: null,
    });

    render(<TweetList tweets={mockTweets} onTweetDeleted={mockOnTweetDeleted} />);

    expect(screen.queryByLabelText('Delete tweet')).not.toBeInTheDocument();
  });

  it('deletes tweet when confirmed', async () => {
    // Mock window.confirm
    global.confirm = jest.fn(() => true);

    (api.tweets.delete as jest.Mock).mockResolvedValue(undefined);

    render(<TweetList tweets={mockTweets} onTweetDeleted={mockOnTweetDeleted} />);

    const deleteButton = screen.getByLabelText('Delete tweet');
    await userEvent.click(deleteButton);

    await waitFor(() => {
      expect(api.tweets.delete).toHaveBeenCalledWith('mock-token', '1');
      expect(mockOnTweetDeleted).toHaveBeenCalledWith('1');
    });
  });

  it('does not delete tweet when cancelled', async () => {
    global.confirm = jest.fn(() => false);

    render(<TweetList tweets={mockTweets} onTweetDeleted={mockOnTweetDeleted} />);

    const deleteButton = screen.getByLabelText('Delete tweet');
    await userEvent.click(deleteButton);

    expect(api.tweets.delete).not.toHaveBeenCalled();
    expect(mockOnTweetDeleted).not.toHaveBeenCalled();
  });

  it('formats relative time correctly', () => {
    render(<TweetList tweets={mockTweets} onTweetDeleted={mockOnTweetDeleted} />);

    // The second tweet is 1 hour ago
    expect(screen.getByText('1h ago')).toBeInTheDocument();
  });
});
