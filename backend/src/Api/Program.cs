using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

// Only add Infrastructure if not in Testing environment
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddInfrastructure(builder.Configuration);
}

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Ask if user wants to seed the database
if (!app.Environment.IsProduction())
{
    Console.WriteLine("\n╔════════════════════════════════════════════╗");
    Console.WriteLine("║     TwitterClone Database Setup            ║");
    Console.WriteLine("╚════════════════════════════════════════════╝");
    Console.WriteLine("\nDo you want to seed the database with sample data?");
    Console.WriteLine("This will create 12 test users, tweets, follows, and likes.");
    Console.WriteLine("\n⚠️  Warning: Skip this if your database already has data.");
    Console.WriteLine("\nType 'yes' to seed, or press Enter to skip: ");
    
    var response = Console.ReadLine()?.Trim().ToLower();
    
    if (response == "yes" || response == "y")
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.ApplicationDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.IPasswordHasher<Domain.Entities.User>>();
            
            var seeder = new Infrastructure.Persistence.DatabaseSeeder(context, passwordHasher);
            await seeder.SeedAsync();
        }
        
        Console.WriteLine("\n✅ Database seeded successfully!");
        Console.WriteLine("Press any key to start the application...");
        Console.ReadKey();
        Console.Clear();
    }
    else
    {
        Console.WriteLine("\n⏭️  Skipping database seed.");
    }
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", (Infrastructure.Persistence.ApplicationDbContext db) =>
{
    try
    {
        db.Database.CanConnect();
        return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "unhealthy", error = ex.Message, timestamp = DateTime.UtcNow },
            statusCode: 503);
    }
});

app.Run();

// Make Program accessible to tests
public partial class Program { }
