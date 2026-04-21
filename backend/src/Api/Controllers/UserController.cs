using Application.Interfaces;
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
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(DbContext context, IUserService userService, ILogger<UserController> logger)
    {
        _context = context;
        _userService = userService;
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

    [HttpGet("{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUsername(string username)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = null;
            
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                currentUserId = userGuid;
            }

            var profile = await _userService.GetByUsernameAsync(username, currentUserId);

            if (profile == null)
            {
                return NotFound(new { error = $"User '{username}' not found." });
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for {Username}", username);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { error = "Search query 'q' is required." });
            }

            var results = await _userService.SearchAsync(q);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query {Query}", q);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpPost("{id}/follow")]
    public async Task<IActionResult> Follow(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var followerGuid))
            {
                return Unauthorized(new { error = "User not authenticated." });
            }

            await _userService.FollowAsync(followerGuid, id);
            return Ok(new { message = "Successfully followed user." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error following user {UserId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id}/follow")]
    public async Task<IActionResult> Unfollow(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var followerGuid))
            {
                return Unauthorized(new { error = "User not authenticated." });
            }

            await _userService.UnfollowAsync(followerGuid, id);
            return Ok(new { message = "Successfully unfollowed user." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfollowing user {UserId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
