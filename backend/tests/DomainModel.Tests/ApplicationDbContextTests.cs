using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DomainModel.Tests;

public class ApplicationDbContextTests
{
    [Fact]
    public async Task ShouldEnforceUniqueUsernameAndEmail()
    {
        await using var database = await TestDatabase.CreateAsync();

        database.Context.Users.Add(new User("alice", "alice@test.com", "Alice"));
        await database.Context.SaveChangesAsync();

        database.Context.Users.Add(new User("alice", "alice-2@test.com", "Alice 2"));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());

        database.Context.ChangeTracker.Clear();
        database.Context.Users.Add(new User("alice-2", "alice@test.com", "Alice 3"));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task ShouldEnforceUniqueLikePerUserAndTweet()
    {
        await using var database = await TestDatabase.CreateAsync();

        var user = new User("alice", "alice@test.com", "Alice");
        var tweet = new Tweet(user.Id, "hello world");

        database.Context.AddRange(user, tweet);
        database.Context.Likes.Add(new Like(user.Id, tweet.Id));
        await database.Context.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();
        database.Context.Likes.Add(new Like(user.Id, tweet.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task ShouldEnforceUniqueFollowAndDatabaseNoSelfFollow()
    {
        await using var database = await TestDatabase.CreateAsync();

        var alice = new User("alice", "alice@test.com", "Alice");
        var bob = new User("bob", "bob@test.com", "Bob");

        database.Context.AddRange(alice, bob);
        database.Context.Follows.Add(new Follow(alice.Id, bob.Id));
        await database.Context.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();
        database.Context.Follows.Add(new Follow(alice.Id, bob.Id));
        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());

        database.Context.ChangeTracker.Clear();
        await Assert.ThrowsAsync<SqliteException>(() => database.Context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO follows (follower_id, followed_id, created_at_utc) VALUES ({bob.Id}, {bob.Id}, {DateTime.UtcNow})"));
    }

    [Fact]
    public async Task ShouldConfigureBasicRelationships()
    {
        await using var database = await TestDatabase.CreateAsync();

        var alice = new User("alice", "alice@test.com", "Alice");
        var bob = new User("bob", "bob@test.com", "Bob");
        var tweet = new Tweet(alice.Id, "hello world");
        var like = new Like(bob.Id, tweet.Id);
        var follow = new Follow(bob.Id, alice.Id);

        database.Context.AddRange(alice, bob, tweet, like, follow);
        await database.Context.SaveChangesAsync();

        var savedTweet = await database.Context.Tweets
            .Include(current => current.User)
            .Include(current => current.Likes)
            .SingleAsync();

        var savedFollow = await database.Context.Follows
            .Include(current => current.Follower)
            .Include(current => current.Followed)
            .SingleAsync();

        Assert.Equal(alice.Id, savedTweet.User.Id);
        Assert.Single(savedTweet.Likes);
        Assert.Equal(bob.Id, savedFollow.Follower.Id);
        Assert.Equal(alice.Id, savedFollow.Followed.Id);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private TestDatabase(SqliteConnection connection, ApplicationDbContext context)
        {
            Connection = connection;
            Context = context;
        }

        public SqliteConnection Connection { get; }

        public ApplicationDbContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}