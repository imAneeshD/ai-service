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
            var apiKey = _configuration["GeminiApiKey"] ?? _configuration["Gemini:API"];
            var model = _configuration["GeminiModel"] ?? "gemini-2.5-flash";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key=" + apiKey;

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

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {errorContent}");
            }

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

        public async IAsyncEnumerable<string> GenerateResponseStream(string prompt)
        {
            var apiKey = _configuration["GeminiApiKey"] ?? _configuration["Gemini:API"];
            var model = _configuration["GeminiModel"] ?? "gemini-2.5-flash";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:streamGenerateContent?key=" + apiKey;

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

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {errorContent}");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var startIndex = line.IndexOf("\"text\":");
                if (startIndex >= 0)
                {
                    var textPart = line.Substring(startIndex);
                    var text = TryParseText(textPart);
                    if (!string.IsNullOrEmpty(text))
                    {
                        yield return text;
                    }
                }
            }
        }

        private static string? TryParseText(string textPart)
        {
            try
            {
                using var doc = JsonDocument.Parse("{" + textPart.TrimEnd(',') + "}");
                return doc.RootElement.GetProperty("text").GetString();
            }
            catch
            {
                return null;
            }
        }
    }
}
