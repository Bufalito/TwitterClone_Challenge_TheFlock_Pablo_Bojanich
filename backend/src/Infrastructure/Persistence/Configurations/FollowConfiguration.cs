using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("follows");

        builder.HasKey(follow => new { follow.FollowerId, follow.FollowedId });

        builder.Property(follow => follow.FollowerId)
            .HasColumnName("follower_id");

        builder.Property(follow => follow.FollowedId)
            .HasColumnName("followed_id");

        builder.Property(follow => follow.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.ToTable(tableBuilder =>
            tableBuilder.HasCheckConstraint("ck_follows_no_self_follow", "follower_id <> followed_id"));
    }
}