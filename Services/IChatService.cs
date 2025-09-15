using IntegrationChatAPI.Models;

namespace IntegrationChatAPI.Services;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request);
    Task<bool> IsHealthyAsync();
    Task<ChatResponse> SendTestMessageAsync(ChatRequest request);
}