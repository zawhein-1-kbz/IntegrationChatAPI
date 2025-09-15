using Microsoft.AspNetCore.Mvc;
using IntegrationChatAPI.Models;
using IntegrationChatAPI.Services;

namespace IntegrationChatAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GitHubModelsController : ControllerBase
{
    private readonly GitHubModelsChatService _gitHubModelsChatService;
    private readonly ILogger<GitHubModelsController> _logger;

    public GitHubModelsController(GitHubModelsChatService gitHubModelsChatService, ILogger<GitHubModelsController> logger)
    {
        _gitHubModelsChatService = gitHubModelsChatService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a message to the GitHub Models AI chat service and returns the response
    /// </summary>
    /// <param name="request">The chat request containing the message and optional conversation ID</param>
    /// <returns>The AI response from GitHub Models</returns>
    [HttpPost("message")]
    [ProducesResponseType(typeof(ChatResponse), 200)]
    [ProducesResponseType(typeof(ChatError), 400)]
    [ProducesResponseType(typeof(ChatError), 500)]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatError("Message cannot be empty", "INVALID_MESSAGE"));
            }

            var response = await _gitHubModelsChatService.SendMessageAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new ChatError(ex.Message, "INVALID_REQUEST"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GitHub Models chat message");
            return StatusCode(500, new ChatError("Internal server error occurred", "INTERNAL_ERROR"));
        }
    }

    [HttpPost("testMessage")]
    [ProducesResponseType(typeof(ChatResponse), 200)]
    [ProducesResponseType(typeof(ChatError), 400)]
    [ProducesResponseType(typeof(ChatError), 500)]
    public async Task<IActionResult> TestMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatError("Message cannot be empty", "INVALID_MESSAGE"));
            }
            var response = await _gitHubModelsChatService.SendTestMessageAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new ChatError(ex.Message, "INVALID_REQUEST"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GitHub Models chat message");
            return StatusCode(500, new ChatError("Internal server error occurred", "INTERNAL_ERROR"));
        }
    }
    /// <summary>
    /// Health check endpoint for the GitHub Models chat service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var isHealthy = await _gitHubModelsChatService.IsHealthyAsync();
            
            if (isHealthy)
            {
                return Ok(new { status = "healthy", service = "GitHub Models", timestamp = DateTime.UtcNow });
            }
            
            return StatusCode(503, new { status = "unhealthy", service = "GitHub Models", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Models health check failed");
            return StatusCode(503, new { status = "unhealthy", service = "GitHub Models", error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
}