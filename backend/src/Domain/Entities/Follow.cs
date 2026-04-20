namespace Domain.Entities;

public class Follow
{
    private Follow()
    {
    }

    public Follow(Guid followerId, Guid followedId)
    {
        if (followerId == followedId)
        {
            throw new ArgumentException("A user cannot follow themselves.", nameof(followedId));
        }

        FollowerId = followerId;
        FollowedId = followedId;
    }

    public Guid FollowerId { get; private set; }

    public Guid FollowedId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public User Follower { get; private set; } = null!;

    public User Followed { get; private set; } = null!;
}