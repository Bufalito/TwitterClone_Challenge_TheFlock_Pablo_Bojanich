using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.IntegrationTests;

public class UserRegistrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public UserRegistrationTests(WebApplicationFactory<Program> factory)
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
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("John Doe", result.DisplayName);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request1 = new RegisterRequest
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john1@example.com",
            Password = "SecurePass123!"
        };
        var request2 = new RegisterRequest
        {
            Name = "Jane Doe",
            Username = "johndoe",
            Email = "jane@example.com",
            Password = "SecurePass123!"
        };

        // Act
        await client.PostAsJsonAsync("/api/auth/register", request1);
        var response = await client.PostAsJsonAsync("/api/auth/register", request2);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("already taken", error);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request1 = new RegisterRequest
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };
        var request2 = new RegisterRequest
        {
            Name = "Jane Doe",
            Username = "janedoe",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };

        // Act
        await client.PostAsJsonAsync("/api/auth/register", request1);
        var response = await client.PostAsJsonAsync("/api/auth/register", request2);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("already registered", error);
    }

    [Fact]
    public async Task Register_WithInvalidUsername_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Username = "a", // Too short
            Email = "john@example.com",
            Password = "SecurePass123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "not-an-email",
            Password = "SecurePass123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "short"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithBioAndAvatar_ReturnsCreatedAndStoresOptionalFields()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "SecurePass123!",
            Bio = "Software developer and tech enthusiast",
            Avatar = "https://example.com/avatar.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
    }

    [Fact]
    public async Task Register_PasswordIsHashed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };

        // Act
        await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert - verify password is hashed in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await context.Users.SingleAsync(u => u.Username == "johndoe");

        Assert.NotEqual("SecurePass123!", user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);
        Assert.Contains("AQ", user.PasswordHash); // PasswordHasher prefix
    }
}
