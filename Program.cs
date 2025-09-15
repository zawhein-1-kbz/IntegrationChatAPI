using IntegrationChatAPI.Models;
using IntegrationChatAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure OpenAI settings
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

// Configure GitHub Models settings
builder.Services.Configure<GitHubModelsSettings>(
    builder.Configuration.GetSection("GitHubModels"));

// Register chat services
builder.Services.AddScoped<IChatService, OpenAIChatService>();
builder.Services.AddScoped<GitHubModelsChatService>();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Integration Chat API", 
        Version = "v1",
        Description = "A production-ready API for chat integration with OpenAI GPT models and GitHub Models"
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Integration Chat API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
