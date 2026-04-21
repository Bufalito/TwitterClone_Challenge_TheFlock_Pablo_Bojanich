using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.IntegrationTests;

public class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Add SQLite for testing with the persistent connection
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(_connection));

                services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

                // Ensure database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        // First register a user
        var registerRequest = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPass123!"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "TestPass123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
        Assert.Equal("testuser", loginResponse.Username);
        Assert.Equal("test@example.com", loginResponse.Email);
        Assert.Equal("Test User", loginResponse.DisplayName);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        var loginRequest = new LoginRequest
        {
            Username = "nonexistent",
            Password = "TestPass123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // First register a user
        var registerRequest = new RegisterRequest
        {
            Name = "Test User 2",
            Username = "testuser2",
            Email = "test2@example.com",
            Password = "TestPass123!"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Username = "testuser2",
            Password = "WrongPassword!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/user/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserInfo()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Register and login
        var registerRequest = new RegisterRequest
        {
            Name = "Test User 3",
            Username = "testuser3",
            Email = "test3@example.com",
            Password = "TestPass123!",
            Bio = "Test bio",
            Avatar = "https://example.com/avatar.jpg"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Username = "testuser3",
            Password = "TestPass123!"
        };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Set the token
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginData!.Token);

        // Act
        var response = await client.GetAsync("/api/user/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("testuser3", json);
        Assert.Contains("test3@example.com", json);
        Assert.Contains("Test User 3", json);
        Assert.Contains("Test bio", json);
        Assert.Contains("https://example.com/avatar.jpg", json);
    }

    [Fact]
    public async Task Me_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await client.GetAsync("/api/user/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithShortPassword_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "short"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithShortUsername_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        var loginRequest = new LoginRequest
        {
            Username = "ab",
            Password = "TestPass123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
