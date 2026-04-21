using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class TweetConfiguration : IEntityTypeConfiguration<Tweet>
{
    public void Configure(EntityTypeBuilder<Tweet> builder)
    {
        builder.ToTable("tweets");

        builder.HasKey(tweet => tweet.Id);

        builder.Property(tweet => tweet.Id)
            .HasColumnName("id");

        builder.Property(tweet => tweet.UserId)
            .HasColumnName("user_id");

        builder.Property(tweet => tweet.Content)
            .HasColumnName("content")
            .HasMaxLength(Tweet.MaxContentLength)
            .IsRequired();

        builder.Property(tweet => tweet.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(tweet => tweet.ParentTweetId)
            .HasColumnName("parent_tweet_id")
            .IsRequired(false);

        builder.ToTable(tableBuilder =>
            tableBuilder.HasCheckConstraint("ck_tweets_content_length", "length(content) <= 280"));

        builder.HasMany(tweet => tweet.Likes)
            .WithOne(like => like.Tweet)
            .HasForeignKey(like => like.TweetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tweet => tweet.Replies)
            .WithOne(reply => reply.ParentTweet)
            .HasForeignKey(reply => reply.ParentTweetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}