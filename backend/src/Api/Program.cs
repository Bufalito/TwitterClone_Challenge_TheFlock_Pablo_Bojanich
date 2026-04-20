using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

// Only add Infrastructure if not in Testing environment
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddInfrastructure(builder.Configuration);
}

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

app.UseCors();

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
