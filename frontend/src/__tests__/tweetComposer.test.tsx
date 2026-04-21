import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import TweetComposer from '@/components/TweetComposer';
import { api } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';

// Mock API
jest.mock('@/lib/api', () => ({
  api: {
    tweets: {
      create: jest.fn(),
    },
  },
}));

// Mock auth store
jest.mock('@/store/authStore', () => ({
  useAuthStore: jest.fn(),
}));

describe('TweetComposer', () => {
  const mockOnTweetCreated = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: 'mock-token',
    });
  });

  it('renders when user is authenticated', () => {
    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    expect(screen.getByPlaceholderText("What's happening?")).toBeInTheDocument();
  });

  it('does not render when user is not authenticated', () => {
    (useAuthStore as unknown as jest.Mock).mockReturnValue({
      token: null,
    });

    const { container } = render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    expect(container.firstChild).toBeNull();
  });

  it('shows character count', () => {
    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    expect(screen.getByText('280 characters left')).toBeInTheDocument();
  });

  it('updates character count as user types', async () => {
    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    const textarea = screen.getByPlaceholderText("What's happening?");

    await userEvent.type(textarea, 'Hello');

    expect(screen.getByText('275 characters left')).toBeInTheDocument();
  });

  it('disables submit button when content is empty', () => {
    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    const button = screen.getByRole('button', { name: /tweet/i });

    expect(button).toBeDisabled();
  });

  it('enables submit button when content is valid', async () => {
    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    const textarea = screen.getByPlaceholderText("What's happening?");
    const button = screen.getByRole('button', { name: /tweet/i });

    await userEvent.type(textarea, 'Hello world!');

    expect(button).not.toBeDisabled();
  });

  it('creates tweet successfully', async () => {
    const mockTweet = {
      id: '1',
      userId: 'user-1',
      content: 'Test tweet',
      createdAtUtc: new Date().toISOString(),
      username: 'testuser',
      displayName: 'Test User',
      likesCount: 0,
    };

    (api.tweets.create as jest.Mock).mockResolvedValue(mockTweet);

    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    const textarea = screen.getByPlaceholderText("What's happening?");
    const button = screen.getByRole('button', { name: /tweet/i });

    await userEvent.type(textarea, 'Test tweet');
    await userEvent.click(button);

    await waitFor(() => {
      expect(api.tweets.create).toHaveBeenCalledWith('mock-token', {
        content: 'Test tweet',
      });
      expect(mockOnTweetCreated).toHaveBeenCalledWith(mockTweet);
    });
  });

  it('shows error when tweet creation fails', async () => {
    (api.tweets.create as jest.Mock).mockRejectedValue(new Error('Failed to post'));

    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    const textarea = screen.getByPlaceholderText("What's happening?");
    const button = screen.getByRole('button', { name: /tweet/i });

    await userEvent.type(textarea, 'Test tweet');
    await userEvent.click(button);

    await waitFor(() => {
      expect(screen.getByText('Failed to post')).toBeInTheDocument();
    });
  });

  it('clears content after successful tweet', async () => {
    const mockTweet = {
      id: '1',
      userId: 'user-1',
      content: 'Test tweet',
      createdAtUtc: new Date().toISOString(),
      username: 'testuser',
      displayName: 'Test User',
      likesCount: 0,
    };

    (api.tweets.create as jest.Mock).mockResolvedValue(mockTweet);

    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    const textarea = screen.getByPlaceholderText("What's happening?") as HTMLTextAreaElement;
    const button = screen.getByRole('button', { name: /tweet/i });

    await userEvent.type(textarea, 'Test tweet');
    await userEvent.click(button);

    await waitFor(() => {
      expect(textarea.value).toBe('');
    });
  });

  it('shows warning when approaching character limit', async () => {
    render(<TweetComposer onTweetCreated={mockOnTweetCreated} />);
    const textarea = screen.getByPlaceholderText("What's happening?");

    const longText = 'a'.repeat(270);
    await userEvent.type(textarea, longText);

    const charCount = screen.getByText('10 characters left');
    expect(charCount).toHaveClass('text-red-400');
  });
});
