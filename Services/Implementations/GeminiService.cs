using ai_service.Services.Interface;
using System.Text;
using System.Text.Json;

namespace ai_service.Services.Implementations
{
    public class GeminiService(HttpClient httpClient, IConfiguration configuration) : IAIService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string> GenerateResponse(string prompt)
        {
            var apiKey = _configuration["GeminiApiKey"];

            var url = "https://generativelanguage.googleapis.com/v1beta2/models/gemini-1.5-pro:generateContent?key=" + apiKey;

            var requestBody = new
            {
                contents = new[]
               {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            var response = _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseJson);


            return document
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }
    }
}
