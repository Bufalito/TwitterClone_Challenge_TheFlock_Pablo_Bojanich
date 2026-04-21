using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.IntegrationTests;

public class FollowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FollowTests(WebApplicationFactory<Program> factory)
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

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResult?.Token ?? throw new Exception("Token is null");
    }

    private async Task<UserProfileResponse> GetUserProfileAsync(string username, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{username}");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        return profile ?? throw new Exception("Profile is null");
    }

    [Fact]
    public async Task Follow_Success()
    {
        var followerToken = await RegisterAndLoginAsync($"follower{Guid.NewGuid():N}");
        var followedUsername = $"followed{Guid.NewGuid():N}";
        var followedToken = await RegisterAndLoginAsync(followedUsername);

        var followedProfile = await GetUserProfileAsync(followedUsername);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", followerToken);
        var response = await _client.PostAsync($"/api/user/{followedProfile.Id}/follow", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify follower count increased
        var updatedProfile = await GetUserProfileAsync(followedUsername);
        Assert.True(updatedProfile.FollowersCount > followedProfile.FollowersCount);
    }

    [Fact]
    public async Task Follow_CannotFollowSelf()
    {
        var token = await RegisterAndLoginAsync($"selffollow{Guid.NewGuid():N}");
        
        // Get current user's ID
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _client.GetAsync("/api/user/me");
        meResponse.EnsureSuccessStatusCode();
        var meData = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        
        // Try to follow self
        var response = await _client.PostAsync($"/api/user/{meData!.Id}/follow", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Follow_PreventsDuplicates()
    {
        var followerToken = await RegisterAndLoginAsync($"follower2{Guid.NewGuid():N}");
        var followedUsername = $"followed2{Guid.NewGuid():N}";
        await RegisterAndLoginAsync(followedUsername);

        var followedProfile = await GetUserProfileAsync(followedUsername);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", followerToken);
        
        // First follow should succeed
        var response1 = await _client.PostAsync($"/api/user/{followedProfile.Id}/follow", null);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Second follow should fail
        var response2 = await _client.PostAsync($"/api/user/{followedProfile.Id}/follow", null);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task Unfollow_Success()
    {
        var followerToken = await RegisterAndLoginAsync($"follower3{Guid.NewGuid():N}");
        var followedUsername = $"followed3{Guid.NewGuid():N}";
        await RegisterAndLoginAsync(followedUsername);

        var followedProfile = await GetUserProfileAsync(followedUsername);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", followerToken);
        
        // First follow
        await _client.PostAsync($"/api/user/{followedProfile.Id}/follow", null);

        // Then unfollow
        var response = await _client.DeleteAsync($"/api/user/{followedProfile.Id}/follow");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify follower count decreased
        var updatedProfile = await GetUserProfileAsync(followedUsername);
        Assert.Equal(followedProfile.FollowersCount, updatedProfile.FollowersCount);
    }

    [Fact]
    public async Task Unfollow_FailsWhenNotFollowing()
    {
        var followerToken = await RegisterAndLoginAsync($"follower4{Guid.NewGuid():N}");
        var followedUsername = $"followed4{Guid.NewGuid():N}";
        await RegisterAndLoginAsync(followedUsername);

        var followedProfile = await GetUserProfileAsync(followedUsername);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", followerToken);
        
        // Try to unfollow without following first
        var response = await _client.DeleteAsync($"/api/user/{followedProfile.Id}/follow");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Follow_RequiresAuthentication()
    {
        var followedUsername = $"followed5{Guid.NewGuid():N}";
        await RegisterAndLoginAsync(followedUsername);
        var followedProfile = await GetUserProfileAsync(followedUsername);

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsync($"/api/user/{followedProfile.Id}/follow", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
