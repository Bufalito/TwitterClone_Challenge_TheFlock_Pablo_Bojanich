namespace Domain.Entities;

public class Tweet
{
    public const int MaxContentLength = 280;

    private Tweet()
    {
    }

    public Tweet(Guid userId, string content)
    {
        UserId = userId;
        Content = ValidateContent(content);
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;

    public ICollection<Like> Likes { get; } = new List<Like>();

    private static string ValidateContent(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

        var trimmedContent = content.Trim();
        if (trimmedContent.Length > MaxContentLength)
        {
            throw new ArgumentException($"Tweet content cannot exceed {MaxContentLength} characters.", nameof(content));
        }

        return trimmedContent;
    }
}