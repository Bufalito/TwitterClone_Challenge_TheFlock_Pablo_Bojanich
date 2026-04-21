using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly DbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(DbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // Find user by username
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return null;

        // Get password hash via reflection (internal setter)
        var passwordHashProperty = typeof(User).GetProperty("PasswordHash");
        var passwordHash = passwordHashProperty?.GetValue(user) as string;

        if (string.IsNullOrEmpty(passwordHash))
            return null;

        // Verify password
        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        // Generate token
        var token = GenerateJwtToken(user.Id.ToString(), user.Username, user.Email);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }

    public string GenerateJwtToken(string userId, string username, string email)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = jwtSettings["Issuer"] ?? "TwitterCloneApi";
        var audience = jwtSettings["Audience"] ?? "TwitterCloneClient";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
