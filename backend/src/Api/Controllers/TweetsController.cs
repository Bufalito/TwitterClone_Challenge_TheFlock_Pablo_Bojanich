using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TweetsController : ControllerBase
{
    private readonly ITweetService _tweetService;
    private readonly ILogger<TweetsController> _logger;

    public TweetsController(ITweetService tweetService, ILogger<TweetsController> logger)
    {
        _tweetService = tweetService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateTweetRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { error = "User not authenticated." });
            }

            var tweet = await _tweetService.CreateAsync(userGuid, request);
            return CreatedAtAction(nameof(GetRecent), new { id = tweet.Id }, tweet);
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
            _logger.LogError(ex, "Error creating tweet");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { error = "User not authenticated." });
            }

            await _tweetService.DeleteAsync(userGuid, id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tweet {TweetId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 20)
    {
        try
        {
            if (count < 1 || count > 100)
            {
                return BadRequest(new { error = "Count must be between 1 and 100." });
            }

            var tweets = await _tweetService.GetRecentAsync(count);
            return Ok(tweets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent tweets");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpGet("user/{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUser(string username)
    {
        try
        {
            var tweets = await _tweetService.GetByUserAsync(username);
            return Ok(tweets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tweets for user {Username}", username);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
