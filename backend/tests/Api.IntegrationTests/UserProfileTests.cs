using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.IntegrationTests;

public class UserProfileTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserProfileTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByUsername_ExistingUser_ReturnsProfile()
    {
        // Arrange - Register a user first
        var registerRequest = new
        {
            name = "Profile Test User",
            username = "profiletest",
            email = "profile@test.com",
            password = "TestPass123!",
            bio = "Test bio for profile"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act
        var response = await _client.GetAsync("/api/user/profiletest");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("profiletest", profile.Username);
        Assert.Equal("Profile Test User", profile.DisplayName);
        Assert.Equal("profile@test.com", profile.Email);
        Assert.Equal("Test bio for profile", profile.Bio);
        Assert.Equal(0, profile.FollowersCount);
        Assert.Equal(0, profile.FollowingCount);
        Assert.Equal(0, profile.TweetsCount);
    }

    [Fact]
    public async Task GetByUsername_NonExistingUser_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/user/nonexistentuser");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithMatchingQuery_ReturnsUsers()
    {
        // Arrange - Register multiple users
        var users = new[]
        {
            new { name = "John Doe", username = "johndoe", email = "john@test.com", password = "Pass123!" },
            new { name = "Jane Doe", username = "janedoe", email = "jane@test.com", password = "Pass123!" },
            new { name = "Bob Smith", username = "bobsmith", email = "bob@test.com", password = "Pass123!" }
        };

        foreach (var user in users)
        {
            await _client.PostAsJsonAsync("/api/auth/register", user);
        }

        // Act
        var response = await _client.GetAsync("/api/user/search?q=doe");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<UserSearchResult>>();
        Assert.NotNull(results);
        Assert.True(results.Count >= 2);
        Assert.Contains(results, r => r.Username == "johndoe");
        Assert.Contains(results, r => r.Username == "janedoe");
    }

    [Fact]
    public async Task Search_WithoutQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/user/search");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/user/search?q=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithNoMatches_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/user/search?q=veryrandomstringthatwontmatch12345");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<UserSearchResult>>();
        Assert.NotNull(results);
        Assert.Empty(results);
    }
}
