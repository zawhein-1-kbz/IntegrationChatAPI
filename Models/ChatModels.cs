namespace IntegrationChatAPI.Models;

public record ChatRequest(string Message, string? ConversationId = null);

public record ChatResponse(
    string Message, 
    string ConversationId, 
    DateTime Timestamp,
    string Model,
    int TokensUsed);

public record ChatError(string Message, string Code);

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o"; // Using GPT-4o as GPT-5 isn't released yet
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 0.7;
}

public class GitHubModelsSettings
{
    public string GitHubToken { get; set; } = string.Empty;
    public string Model { get; set; } = "openai/gpt-4.1-nano";
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 1.0;
    public double TopP { get; set; } = 1.0;
    public string Endpoint { get; set; } = "https://models.github.ai/inference";
}