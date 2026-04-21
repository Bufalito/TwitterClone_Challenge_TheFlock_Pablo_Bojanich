using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly DbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(DbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { error = "User not authenticated." });
            }

            var user = await _context.Set<Domain.Entities.User>()
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null)
            {
                return NotFound(new { error = "User not found." });
            }

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.DisplayName,
                Bio = user.Bio,
                Avatar = user.Avatar,
                user.CreatedAtUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
