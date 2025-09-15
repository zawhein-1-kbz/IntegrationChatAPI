# IntegrationChatAPI

A production-ready ASP.NET Core Web API that integrates with OpenAI's GPT models to provide chat functionality for other systems.

## Features

- **OpenAI Integration**: Uses the official OpenAI .NET SDK to connect to GPT models
- **Conversation Management**: Supports conversation history with unique conversation IDs
- **Health Monitoring**: Built-in health checks for service monitoring
- **Production Ready**: Comprehensive error handling, logging, and configuration
- **RESTful API**: Clean REST endpoints for easy integration
- **Swagger Documentation**: Interactive API documentation available in development

## Configuration

### Required Settings

Configure your OpenAI API key in `appsettings.json` or environment variables:

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here",
    "Model": "gpt-4o",
    "MaxTokens": 1000,
    "Temperature": 0.7
  }
}
```

### Environment Variables

You can also set the API key via environment variable:
```bash
export OpenAI__ApiKey="your-openai-api-key-here"
```

## API Endpoints

### Chat Message
Send a message to the AI and receive a response.

**POST** `/api/chat/message`

Request Body:
```json
{
    "message": "Your message here",
    "conversationId": "optional-conversation-id"
}
```

Response:
```json
{
    "message": "AI response here",
    "conversationId": "conversation-id",
    "timestamp": "2024-01-15T10:30:00Z",
    "model": "gpt-4o",
    "tokensUsed": 45
}
```

### Health Check
Check if the chat service is operational.

**GET** `/api/chat/health`

Response (200 OK):
```json
{
    "status": "healthy",
    "timestamp": "2024-01-15T10:30:00Z"
}
```

### Application Health
Standard health check endpoint for load balancers.

**GET** `/health`

## Running the Application

1. **Prerequisites**: .NET 8.0 SDK installed
2. **Configure**: Set your OpenAI API key in configuration
3. **Run**: `dotnet run`
4. **Access**: Navigate to `https://localhost:5001` for Swagger documentation

## Development

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Run in development mode with hot reload
dotnet watch run
```

## Integration Examples

### cURL Example
```bash
curl -X POST https://your-api-domain/api/chat/message \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, how can you help me today?"}'
```

### JavaScript Example
```javascript
const response = await fetch('/api/chat/message', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        message: 'Hello, how can you help me today?',
        conversationId: 'user-session-123'
    })
});

const chatResponse = await response.json();
console.log(chatResponse.message);
```

## Production Deployment

1. Set the OpenAI API key securely (environment variables, Azure Key Vault, etc.)
2. Configure logging providers for your environment
3. Set up health check monitoring
4. Configure HTTPS certificates
5. Consider rate limiting and authentication as needed

## Security Notes

- Never commit API keys to source control
- Use secure configuration providers in production
- Implement authentication and authorization as needed for your use case
- Consider rate limiting to prevent abuse 
