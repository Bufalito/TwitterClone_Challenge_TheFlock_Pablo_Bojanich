using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.IntegrationTests;

public class TweetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TweetTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLogin()
    {
        var registerRequest = new
        {
            name = "Tweet Test User",
            username = $"tweetuser{Guid.NewGuid():N}",
            email = $"tweet{Guid.NewGuid():N}@test.com",
            password = "TestPass123!"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            username = registerRequest.username,
            password = registerRequest.password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return loginResult!.Token;
    }

    [Fact]
    public async Task CreateTweet_WithValidContent_ReturnsCreated()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new { content = "This is a test tweet!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tweets", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var tweet = await response.Content.ReadFromJsonAsync<TweetResponse>();
        Assert.NotNull(tweet);
        Assert.Equal("This is a test tweet!", tweet.Content);
        Assert.Equal(0, tweet.LikesCount);
    }

    [Fact]
    public async Task CreateTweet_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var request = new { content = "This should fail" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tweets", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTweet_WithEmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new { content = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tweets", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTweet_WithContentTooLong_ReturnsBadRequest()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var longContent = new string('x', 281); // 281 characters
        var request = new { content = longContent };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tweets", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTweet_With280Characters_ReturnsCreated()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new string('x', 280); // Exactly 280 characters
        var request = new { content };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tweets", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTweet_AsOwner_ReturnsNoContent()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new { content = "Tweet to delete" };
        var createResponse = await _client.PostAsJsonAsync("/api/tweets", createRequest);
        var tweet = await createResponse.Content.ReadFromJsonAsync<TweetResponse>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tweets/{tweet!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTweet_AsNonOwner_ReturnsForbidden()
    {
        // Arrange - User 1 creates tweet
        var token1 = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new { content = "User 1 tweet" };
        var createResponse = await _client.PostAsJsonAsync("/api/tweets", createRequest);
        var tweet = await createResponse.Content.ReadFromJsonAsync<TweetResponse>();

        // User 2 tries to delete
        var token2 = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tweets/{tweet!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTweet_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/tweets/{randomId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTweet_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/tweets/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRecentTweets_ReturnsAllTweets()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/tweets", new { content = "Tweet 1" });
        await _client.PostAsJsonAsync("/api/tweets", new { content = "Tweet 2" });
        await _client.PostAsJsonAsync("/api/tweets", new { content = "Tweet 3" });

        // Act
        var response = await _client.GetAsync("/api/tweets");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tweets = await response.Content.ReadFromJsonAsync<List<TweetResponse>>();
        Assert.NotNull(tweets);
        Assert.True(tweets.Count >= 3);
    }

    [Fact]
    public async Task GetRecentTweets_OrderedByNewest()
    {
        // Arrange
        var token = await RegisterAndLogin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tweet1Response = await _client.PostAsJsonAsync("/api/tweets", new { content = "First tweet" });
        var tweet1 = await tweet1Response.Content.ReadFromJsonAsync<TweetResponse>();

        await Task.Delay(100); // Ensure different timestamps

        var tweet2Response = await _client.PostAsJsonAsync("/api/tweets", new { content = "Second tweet" });
        var tweet2 = await tweet2Response.Content.ReadFromJsonAsync<TweetResponse>();

        // Act
        var response = await _client.GetAsync("/api/tweets");
        var tweets = await response.Content.ReadFromJsonAsync<List<TweetResponse>>();

        // Assert
        var userTweets = tweets!.Where(t => t.Id == tweet1!.Id || t.Id == tweet2!.Id).ToList();
        Assert.Equal(2, userTweets.Count);
        Assert.Equal(tweet2!.Id, userTweets[0].Id); // Newest first
    }
}
