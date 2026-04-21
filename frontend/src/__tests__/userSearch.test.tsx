import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useRouter } from 'next/navigation';
import UserSearch from '@/components/UserSearch';
import { api } from '@/lib/api';

// Mock Next.js router
jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

// Mock API
jest.mock('@/lib/api', () => ({
  api: {
    user: {
      search: jest.fn(),
    },
  },
}));

describe('UserSearch', () => {
  const mockPush = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      push: mockPush,
    });
  });

  it('renders search input', () => {
    render(<UserSearch />);
    expect(screen.getByPlaceholderText('Search users...')).toBeInTheDocument();
  });

  it('does not search for queries shorter than 2 characters', async () => {
    render(<UserSearch />);
    const input = screen.getByPlaceholderText('Search users...');

    await userEvent.type(input, 'a');
    
    await waitFor(() => {
      expect(api.user.search).not.toHaveBeenCalled();
    });
  });

  it('searches and displays results', async () => {
    const mockResults = [
      {
        id: '1',
        username: 'johndoe',
        displayName: 'John Doe',
        bio: 'Test bio',
        followersCount: 10,
      },
      {
        id: '2',
        username: 'janedoe',
        displayName: 'Jane Doe',
        bio: null,
        followersCount: 5,
      },
    ];

    (api.user.search as jest.Mock).mockResolvedValue(mockResults);

    render(<UserSearch />);
    const input = screen.getByPlaceholderText('Search users...');

    await userEvent.type(input, 'doe');

    await waitFor(() => {
      expect(api.user.search).toHaveBeenCalledWith('doe');
    });

    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('@johndoe')).toBeInTheDocument();
    expect(screen.getByText('Jane Doe')).toBeInTheDocument();
    expect(screen.getByText('10 followers')).toBeInTheDocument();
  });

  it('navigates to user profile on click', async () => {
    const mockResults = [
      {
        id: '1',
        username: 'johndoe',
        displayName: 'John Doe',
        bio: null,
        followersCount: 0,
      },
    ];

    (api.user.search as jest.Mock).mockResolvedValue(mockResults);

    render(<UserSearch />);
    const input = screen.getByPlaceholderText('Search users...');

    await userEvent.type(input, 'john');

    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });

    const userButton = screen.getByRole('button');
    await userEvent.click(userButton);

    expect(mockPush).toHaveBeenCalledWith('/user/johndoe');
  });

  it('shows no results message when search returns empty', async () => {
    (api.user.search as jest.Mock).mockResolvedValue([]);

    render(<UserSearch />);
    const input = screen.getByPlaceholderText('Search users...');

    await userEvent.type(input, 'xyz');

    await waitFor(() => {
      expect(screen.getByText(/No users found for "xyz"/)).toBeInTheDocument();
    });
  });

  it('shows loading state while searching', async () => {
    (api.user.search as jest.Mock).mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve([]), 100))
    );

    render(<UserSearch />);
    const input = screen.getByPlaceholderText('Search users...');

    await userEvent.type(input, 'test');

    // Check for loading spinner
    await waitFor(() => {
      const spinner = document.querySelector('.animate-spin');
      expect(spinner).toBeInTheDocument();
    });
  });
});
