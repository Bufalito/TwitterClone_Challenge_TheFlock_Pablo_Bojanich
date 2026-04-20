namespace Domain.Entities;

public class User
{
    private User()
    {
    }

    public User(string username, string email, string displayName)
    {
        Username = ValidateRequired(username, nameof(username), 50);
        Email = ValidateRequired(email, nameof(email), 256);
        DisplayName = ValidateRequired(displayName, nameof(displayName), 100);
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Username { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public ICollection<Tweet> Tweets { get; } = new List<Tweet>();

    public ICollection<Like> Likes { get; } = new List<Like>();

    public ICollection<Follow> Following { get; } = new List<Follow>();

    public ICollection<Follow> Followers { get; } = new List<Follow>();

    private static string ValidateRequired(string value, string paramName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);

        var trimmedValue = value.Trim();
        if (trimmedValue.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} exceeds max length of {maxLength}.", paramName);
        }

        return trimmedValue;
    }
}