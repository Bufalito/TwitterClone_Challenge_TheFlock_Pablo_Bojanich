import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import TweetList from '@/components/TweetList';
import { useAuthStore } from '@/store/authStore';
import { api } from '@/lib/api';
import { TweetResponse } from '@/lib/api';

jest.mock('@/store/authStore');
jest.mock('@/lib/api');

const mockTweets: TweetResponse[] = [
  {
    id: 'tweet-1',
    userId: 'user-1',
    content: 'Test tweet',
    createdAtUtc: new Date().toISOString(),
    username: 'testuser',
    displayName: 'Test User',
    likesCount: 5,
    isLikedByCurrentUser: false,
  },
];

describe('TweetList - Like Functionality', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should display like count', () => {
    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
      user: { id: 'user-1', username: 'testuser' },
    });

    render(<TweetList tweets={mockTweets} />);

    expect(screen.getByText('5')).toBeInTheDocument();
    expect(screen.getByLabelText('Like tweet')).toBeInTheDocument();
  });

  it('should like a tweet when clicked', async () => {
    const mockLike = jest.fn().mockResolvedValue({ message: 'Tweet liked' });
    (api.tweets.like as jest.Mock) = mockLike;

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
      user: { id: 'user-2', username: 'otheruser' },
    });

    render(<TweetList tweets={mockTweets} />);

    const likeButton = screen.getByLabelText('Like tweet');
    fireEvent.click(likeButton);

    await waitFor(() => {
      expect(mockLike).toHaveBeenCalledWith('test-token', 'tweet-1');
    });

    // Check optimistic update
    expect(screen.getByText('6')).toBeInTheDocument();
    expect(screen.getByLabelText('Unlike tweet')).toBeInTheDocument();
  });

  it('should unlike a tweet when already liked', async () => {
    const mockUnlike = jest.fn().mockResolvedValue({ message: 'Tweet unliked' });
    (api.tweets.unlike as jest.Mock) = mockUnlike;

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
      user: { id: 'user-2', username: 'otheruser' },
    });

    render(<TweetList tweets={mockTweets} />);

    // First like the tweet
    const likeButton = screen.getByLabelText('Like tweet');
    fireEvent.click(likeButton);

    await waitFor(() => {
      expect(screen.getByLabelText('Unlike tweet')).toBeInTheDocument();
    });

    // Then unlike
    const unlikeButton = screen.getByLabelText('Unlike tweet');
    fireEvent.click(unlikeButton);

    await waitFor(() => {
      expect(mockUnlike).toHaveBeenCalledWith('test-token', 'tweet-1');
    });

    // Check optimistic update reverted
    expect(screen.getByText('5')).toBeInTheDocument();
    expect(screen.getByLabelText('Like tweet')).toBeInTheDocument();
  });

  it('should show alert when not logged in', () => {
    const alertSpy = jest.spyOn(window, 'alert').mockImplementation();

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: null,
      user: null,
    });

    render(<TweetList tweets={mockTweets} />);

    const likeButton = screen.getByLabelText('Like tweet');
    fireEvent.click(likeButton);

    expect(alertSpy).toHaveBeenCalledWith('Please login to like tweets');
    alertSpy.mockRestore();
  });

  it('should revert optimistic update on error', async () => {
    const mockLike = jest.fn().mockRejectedValue(new Error('Network error'));
    (api.tweets.like as jest.Mock) = mockLike;

    const alertSpy = jest.spyOn(window, 'alert').mockImplementation();

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
      user: { id: 'user-2', username: 'otheruser' },
    });

    render(<TweetList tweets={mockTweets} />);

    const likeButton = screen.getByLabelText('Like tweet');
    fireEvent.click(likeButton);

    await waitFor(() => {
      expect(mockLike).toHaveBeenCalled();
    });

    // Check that count reverted back after error
    expect(screen.getByText('5')).toBeInTheDocument();
    expect(alertSpy).toHaveBeenCalledWith(
      'Failed to like tweet: Network error'
    );

    alertSpy.mockRestore();
  });

  it('should disable like button when operation in progress', async () => {
    const mockLike = jest.fn(() => new Promise((resolve) => setTimeout(resolve, 100)));
    (api.tweets.like as jest.Mock) = mockLike;

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
      user: { id: 'user-2', username: 'otheruser' },
    });

    render(<TweetList tweets={mockTweets} />);

    const likeButton = screen.getByLabelText('Like tweet');
    fireEvent.click(likeButton);

    // Button should be disabled during operation
    expect(likeButton).toBeDisabled();

    await waitFor(() => {
      expect(likeButton).not.toBeDisabled();
    });
  });

  it('should initialize like state from isLikedByCurrentUser field', () => {
    const likedTweets: TweetResponse[] = [
      {
        id: 'tweet-liked',
        userId: 'user-1',
        content: 'Already liked tweet',
        createdAtUtc: new Date().toISOString(),
        username: 'testuser',
        displayName: 'Test User',
        likesCount: 10,
        isLikedByCurrentUser: true,
      },
    ];

    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'test-token',
      user: { id: 'user-2', username: 'otheruser' },
    });

    render(<TweetList tweets={likedTweets} />);

    // Should show filled heart and unlike label
    expect(screen.getByLabelText('Unlike tweet')).toBeInTheDocument();
    expect(screen.getByText('❤️')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument();
  });
});
