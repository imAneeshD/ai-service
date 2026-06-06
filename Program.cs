using ai_service.Hubs;
using ai_service.Services.Implementations;
using ai_service.Services.Interface;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSignalR();

// Enable CORS for frontend UI connection
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials();
    });
});

var apiProvider = builder.Configuration["AIServiceProvider"] ?? "Gemini";
if (apiProvider.Equals("Bedrock", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IAIService, BedrockAgentService>();
}
else
{
    builder.Services.AddHttpClient<IAIService, GeminiService>();
}

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "AI Chat API";
        options.Theme = ScalarTheme.BluePlanet;
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseWebSockets();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();