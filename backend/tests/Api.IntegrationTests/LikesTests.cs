using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.IntegrationTests;

public class LikesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LikesTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginAsync(string username)
    {
        var registerRequest = new RegisterRequest
        {
            Name = $"Test User {username}",
            Username = username,
            Email = $"{username}@test.com",
            Password = "TestPass123!"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        
        // Ignore if already registered (409 Conflict)
        if (registerResponse.StatusCode != HttpStatusCode.Created && registerResponse.StatusCode != HttpStatusCode.Conflict)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed with {registerResponse.StatusCode}: {error}");
        }

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = "TestPass123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        if (!loginResponse.IsSuccessStatusCode)
        {
            var error = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Login failed with {loginResponse.StatusCode}: {error}");
        }

        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return loginData!.Token;
    }

    [Fact]
    public async Task LikeTweet_Success()
    {
        // Arrange - Create user and tweet
        var token = await RegisterAndLoginAsync($"likeuser{Guid.NewGuid():N}");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tweetRequest = new { content = "Test tweet for liking" };
        var tweetResponse = await _client.PostAsJsonAsync("/api/tweets", tweetRequest);
        var tweet = await tweetResponse.Content.ReadFromJsonAsync<TweetResponse>();

        // Act - Like the tweet
        var likeResponse = await _client.PostAsync($"/api/tweets/{tweet!.Id}/like", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, likeResponse.StatusCode);

        // Verify like count increased
        var tweetsResponse = await _client.GetAsync("/api/tweets?count=10");
        var tweets = await tweetsResponse.Content.ReadFromJsonAsync<List<TweetResponse>>();
        var likedTweet = tweets!.First(t => t.Id == tweet.Id);
        Assert.Equal(1, likedTweet.LikesCount);
    }

    [Fact]
    public async Task LikeTweet_Duplicate_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLoginAsync($"duplikeuser{Guid.NewGuid():N}");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tweetRequest = new { content = "Test tweet" };
        var tweetResponse = await _client.PostAsJsonAsync("/api/tweets", tweetRequest);
        var tweet = await tweetResponse.Content.ReadFromJsonAsync<TweetResponse>();

        // Like once
        await _client.PostAsync($"/api/tweets/{tweet!.Id}/like", null);

        // Act - Try to like again
        var duplicateLikeResponse = await _client.PostAsync($"/api/tweets/{tweet.Id}/like", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, duplicateLikeResponse.StatusCode);
    }

    [Fact]
    public async Task UnlikeTweet_Success()
    {
        // Arrange - Create user, tweet and like it
        var token = await RegisterAndLoginAsync($"unlikeuser{Guid.NewGuid():N}");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tweetRequest = new { content = "Test tweet for unliking" };
        var tweetResponse = await _client.PostAsJsonAsync("/api/tweets", tweetRequest);
        var tweet = await tweetResponse.Content.ReadFromJsonAsync<TweetResponse>();

        await _client.PostAsync($"/api/tweets/{tweet!.Id}/like", null);

        // Act - Unlike the tweet
        var unlikeResponse = await _client.DeleteAsync($"/api/tweets/{tweet.Id}/like");

        // Assert
        Assert.Equal(HttpStatusCode.OK, unlikeResponse.StatusCode);

        // Verify like count decreased
        var tweetsResponse = await _client.GetAsync("/api/tweets?count=10");
        var tweets = await tweetsResponse.Content.ReadFromJsonAsync<List<TweetResponse>>();
        var unlikedTweet = tweets!.First(t => t.Id == tweet.Id);
        Assert.Equal(0, unlikedTweet.LikesCount);
    }

    [Fact]
    public async Task UnlikeTweet_NotLiked_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLoginAsync($"notlikeduser{Guid.NewGuid():N}");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tweetRequest = new { content = "Test tweet" };
        var tweetResponse = await _client.PostAsJsonAsync("/api/tweets", tweetRequest);
        var tweet = await tweetResponse.Content.ReadFromJsonAsync<TweetResponse>();

        // Act - Try to unlike without liking first
        var unlikeResponse = await _client.DeleteAsync($"/api/tweets/{tweet!.Id}/like");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, unlikeResponse.StatusCode);
    }

    [Fact]
    public async Task LikeTweet_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange - Create a tweet from authenticated user
        var token = await RegisterAndLoginAsync($"authuser{Guid.NewGuid():N}");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tweetRequest = new { content = "Test tweet" };
        var tweetResponse = await _client.PostAsJsonAsync("/api/tweets", tweetRequest);
        var tweet = await tweetResponse.Content.ReadFromJsonAsync<TweetResponse>();

        // Remove auth header
        _client.DefaultRequestHeaders.Authorization = null;

        // Act - Try to like without authentication
        var likeResponse = await _client.PostAsync($"/api/tweets/{tweet!.Id}/like", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, likeResponse.StatusCode);
    }

    [Fact]
    public async Task LikeTweet_NonExistent_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLoginAsync($"nonexistuser{Guid.NewGuid():N}");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var nonExistentTweetId = Guid.NewGuid();

        // Act
        var likeResponse = await _client.PostAsync($"/api/tweets/{nonExistentTweetId}/like", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, likeResponse.StatusCode);
    }
}
