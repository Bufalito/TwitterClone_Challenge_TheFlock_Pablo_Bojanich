using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("likes");

        builder.HasKey(like => new { like.UserId, like.TweetId });

        builder.Property(like => like.UserId)
            .HasColumnName("user_id");

        builder.Property(like => like.TweetId)
            .HasColumnName("tweet_id");

        builder.Property(like => like.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
    }
}