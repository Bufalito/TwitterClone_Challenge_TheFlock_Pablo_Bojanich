using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id");

        builder.Property(user => user.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(user => user.Username)
            .IsUnique();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasMany(user => user.Tweets)
            .WithOne(tweet => tweet.User)
            .HasForeignKey(tweet => tweet.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Likes)
            .WithOne(like => like.User)
            .HasForeignKey(like => like.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Following)
            .WithOne(follow => follow.Follower)
            .HasForeignKey(follow => follow.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.Followers)
            .WithOne(follow => follow.Followed)
            .HasForeignKey(follow => follow.FollowedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}