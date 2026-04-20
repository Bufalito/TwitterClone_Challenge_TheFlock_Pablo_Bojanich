namespace Domain.Entities;

public class Like
{
    private Like()
    {
    }

    public Like(Guid userId, Guid tweetId)
    {
        UserId = userId;
        TweetId = tweetId;
    }

    public Guid UserId { get; private set; }

    public Guid TweetId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;

    public Tweet Tweet { get; private set; } = null!;
}