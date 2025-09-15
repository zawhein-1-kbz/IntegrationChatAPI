using OpenAI;
using OpenAI.Chat;
using IntegrationChatAPI.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.ClientModel;

namespace IntegrationChatAPI.Services;

public class GitHubModelsChatService : IChatService
{
    private readonly ChatClient _chatClient;
    private readonly GitHubModelsSettings _settings;
    private readonly ILogger<GitHubModelsChatService> _logger;
    
    // In-memory conversation storage (for production, use a database)
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();

    public GitHubModelsChatService(IOptions<GitHubModelsSettings> settings, ILogger<GitHubModelsChatService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        
        if (string.IsNullOrEmpty(_settings.GitHubToken))
        {
            throw new InvalidOperationException("GitHub token is not configured");
        }
        
        var endpoint = new Uri(_settings.Endpoint);
        var credential = new ApiKeyCredential(_settings.GitHubToken);
        
        var openAIOptions = new OpenAIClientOptions()
        {
            Endpoint = endpoint
        };

        _chatClient = new ChatClient(_settings.Model, credential, openAIOptions);
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
    {
        try
        {
            var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();
            
            // Get or create conversation history
            var messages = _conversations.GetOrAdd(conversationId, _ => new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant.")
            });
            
            // Add user message to conversation
            messages.Add(new UserChatMessage(request.Message));
            
            // Create chat completion request options
            var requestOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = _settings.MaxTokens,
                Temperature = (float)_settings.Temperature,
                TopP = (float)_settings.TopP
            };

            var response = await _chatClient.CompleteChatAsync(messages, requestOptions);

            var responseMessage = response.Value.Content[0].Text;
            var tokensUsed = response.Value.Usage?.TotalTokenCount ?? 0;
            
            // Add assistant response to conversation
            messages.Add(new AssistantChatMessage(responseMessage));
            
            _logger.LogInformation("GitHub Models chat completion successful for conversation {ConversationId}", conversationId);
            
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
            _logger.LogError(ex, "Error occurred while processing GitHub Models chat request");
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
                new SystemChatMessage("You are a helpful assistant."),
                new UserChatMessage("Hello")
            };
            
            var requestOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = 5,
                Temperature = 0.1f,
                TopP = 1.0f
            };

            var response = await _chatClient.CompleteChatAsync(healthCheckMessages, requestOptions);

            return !string.IsNullOrEmpty(response.Value.Content[0].Text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GitHub Models health check failed");
            return false;
        }
    }

    public async Task<ChatResponse> SendTestMessageAsync(ChatRequest request)
    {
        // 
        var endpoint = new Uri("https://models.github.ai/inference");
        var credential = "***";// System.Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        var model = "openai/gpt-4.1-nano";

        var openAIOptions = new OpenAIClientOptions()
        {
            Endpoint = endpoint
        };

        var client = new ChatClient(model, new ApiKeyCredential(credential), openAIOptions);

        List<ChatMessage> messages = new List<ChatMessage>()
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage(request.Message),
        };

        var requestOptions = new ChatCompletionOptions()
        {
            Temperature = 1.0f,
            TopP = 1.0f,
        };

        var response = client.CompleteChat(messages, requestOptions);
        var responseMessage = response.Value.Content[0].Text;
        var tokensUsed = response.Value.Usage?.TotalTokenCount ?? 0;
        messages.Add(new AssistantChatMessage(responseMessage));

        return new ChatResponse(
                responseMessage,
                request.ConversationId,
                DateTime.UtcNow,
                _settings.Model,
                tokensUsed
            );
    }
}