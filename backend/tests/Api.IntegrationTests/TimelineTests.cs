using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.IntegrationTests;

public class TimelineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TimelineTests(WebApplicationFactory<Program> factory)
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

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResult?.Token ?? throw new Exception("Token is null");
    }

    private async Task<TweetResponse> CreateTweetAsync(string token, string content)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/tweets", new CreateTweetRequest(content));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TweetResponse>() ?? throw new Exception("Tweet is null");
    }

    private async Task FollowUserAsync(string followerToken, Guid followedId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", followerToken);
        await _client.PostAsync($"/api/user/{followedId}/follow", null);
    }

    [Fact]
    public async Task GetTimeline_RequiresAuthentication()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/tweets/timeline");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeline_CombinesOwnAndFollowedTweets()
    {
        var user1Username = $"timelineuser1{Guid.NewGuid():N}";
        var user2Username = $"timelineuser2{Guid.NewGuid():N}";
        
        var user1Token = await RegisterAndLoginAsync(user1Username);
        var user2Token = await RegisterAndLoginAsync(user2Username);

        // User1 creates tweet
        await CreateTweetAsync(user1Token, "My own tweet");

        // User2 creates tweet
        await CreateTweetAsync(user2Token, "Friend's tweet");

        // Get user2 profile
        var profileResponse = await _client.GetAsync($"/api/user/{user2Username}");
        var user2Profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileResponse>();

        // User1 follows user2
        await FollowUserAsync(user1Token, user2Profile!.Id);

        // User1 gets timeline
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
        var response = await _client.GetAsync("/api/tweets/timeline");

        response.EnsureSuccessStatusCode();
        var tweets = await response.Content.ReadFromJsonAsync<List<TweetResponse>>();

        Assert.NotNull(tweets);
        Assert.Equal(2, tweets.Count);
        Assert.Contains(tweets, t => t.Content == "My own tweet");
        Assert.Contains(tweets, t => t.Content == "Friend's tweet");
    }

    [Fact]
    public async Task GetTimeline_SupportsPagination()
    {
        var userToken = await RegisterAndLoginAsync($"timelineuser3{Guid.NewGuid():N}");
        
        // Create 25 tweets
        for (int i = 1; i <= 25; i++)
        {
            await CreateTweetAsync(userToken, $"Tweet {i}");
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        
        // Get first page
        var page1Response = await _client.GetAsync("/api/tweets/timeline?page=1&pageSize=10");
        page1Response.EnsureSuccessStatusCode();
        var page1Tweets = await page1Response.Content.ReadFromJsonAsync<List<TweetResponse>>();

        Assert.NotNull(page1Tweets);
        Assert.Equal(10, page1Tweets.Count);

        // Get second page
        var page2Response = await _client.GetAsync("/api/tweets/timeline?page=2&pageSize=10");
        page2Response.EnsureSuccessStatusCode();
        var page2Tweets = await page2Response.Content.ReadFromJsonAsync<List<TweetResponse>>();

        Assert.NotNull(page2Tweets);
        Assert.Equal(10, page2Tweets.Count);

        // Tweets should be different
        Assert.NotEqual(page1Tweets[0].Id, page2Tweets[0].Id);
    }
}
