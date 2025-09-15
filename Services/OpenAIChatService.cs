using OpenAI;
using OpenAI.Chat;
using IntegrationChatAPI.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace IntegrationChatAPI.Services;

public class OpenAIChatService : IChatService
{
    private readonly OpenAIClient _openAIClient;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIChatService> _logger;
    
    // In-memory conversation storage (for production, use a database)
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();

    public OpenAIChatService(IOptions<OpenAISettings> settings, ILogger<OpenAIChatService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }
        
        _openAIClient = new OpenAIClient(_settings.ApiKey);
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
    {
        try
        {
            var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();
            
            // Get or create conversation history
            var messages = _conversations.GetOrAdd(conversationId, _ => new List<ChatMessage>());
            
            // Add user message to conversation
            messages.Add(ChatMessage.CreateUserMessage(request.Message));
            
            // Create chat completion request
            var chatCompletionOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = _settings.MaxTokens,
                Temperature = (float)_settings.Temperature
            };

            var completion = await _openAIClient.GetChatClient(_settings.Model)
                .CompleteChatAsync(messages, chatCompletionOptions);

            var responseMessage = completion.Value.Content[0].Text;
            var tokensUsed = completion.Value.Usage?.TotalTokenCount ?? 0;
            
            // Add assistant response to conversation
            messages.Add(ChatMessage.CreateAssistantMessage(responseMessage));
            
            _logger.LogInformation("Chat completion successful for conversation {ConversationId}", conversationId);
            
            return new ChatResponse(
                responseMessage,
                conversationId,
                DateTime.UtcNow,
                _settings.Model,
                tokensUsed
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing chat request");
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            // Simple health check - attempt a basic chat completion with minimal tokens
            var healthCheckMessages = new List<ChatMessage>
            {
                ChatMessage.CreateUserMessage("Hello")
            };
            
            var chatCompletionOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = 5,
                Temperature = 0.1f
            };

            var completion = await _openAIClient.GetChatClient(_settings.Model)
                .CompleteChatAsync(healthCheckMessages, chatCompletionOptions);

            return !string.IsNullOrEmpty(completion.Value.Content[0].Text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed");
            return false;
        }
    }

    public Task<ChatResponse> SendTestMessageAsync(ChatRequest request)
    {
        throw new NotImplementedException();
    }
}