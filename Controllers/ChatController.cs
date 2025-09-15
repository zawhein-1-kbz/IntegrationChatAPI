using Microsoft.AspNetCore.Mvc;
using IntegrationChatAPI.Models;
using IntegrationChatAPI.Services;

namespace IntegrationChatAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a message to the AI chat service and returns the response
    /// </summary>
    /// <param name="request">The chat request containing the message and optional conversation ID</param>
    /// <returns>The AI response</returns>
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

            var response = await _chatService.SendMessageAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new ChatError(ex.Message, "INVALID_REQUEST"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new ChatError("Internal server error occurred", "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Health check endpoint for the chat service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var isHealthy = await _chatService.IsHealthyAsync();
            
            if (isHealthy)
            {
                return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
            }
            
            return StatusCode(503, new { status = "unhealthy", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new { status = "unhealthy", error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
}