using Amazon;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using ai_service.Services.Interface;
using System.Text;

namespace ai_service.Services.Implementations
{
    public class BedrockAgentService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly AmazonBedrockAgentRuntimeClient _client;

        public BedrockAgentService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            var regionName = _configuration["AWS:Region"] ?? "us-east-1";
            var region = RegionEndpoint.GetBySystemName(regionName);

            var accessKey = _configuration["AWS:AccessKeyId"];
            var secretKey = _configuration["AWS:SecretAccessKey"];

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                _client = new AmazonBedrockAgentRuntimeClient(accessKey, secretKey, region);
            }
            else
            {
                // Fallback to default credentials chain (instance profiles, env variables, shared credentials file)
                _client = new AmazonBedrockAgentRuntimeClient(region);
            }
        }

        public async Task<string> GenerateResponse(string prompt)
        {
            var agentId = _configuration["Bedrock:AgentId"] ?? throw new ArgumentNullException("Bedrock:AgentId configuration is missing");
            var agentAliasId = _configuration["Bedrock:AgentAliasId"] ?? throw new ArgumentNullException("Bedrock:AgentAliasId configuration is missing");
            var sessionId = Guid.NewGuid().ToString();

            var request = new InvokeAgentRequest
            {
                AgentId = agentId,
                AgentAliasId = agentAliasId,
                SessionId = sessionId,
                InputText = prompt
            };

            var response = await _client.InvokeAgentAsync(request);
            var sb = new StringBuilder();

            await foreach (var eventStream in response.Completion)
            {
                if (eventStream is PayloadPart payloadPart)
                {
                    var chunk = Encoding.UTF8.GetString(payloadPart.Bytes.ToArray());
                    sb.Append(chunk);
                }
            }

            return sb.ToString();
        }

        public async IAsyncEnumerable<string> GenerateResponseStream(string prompt)
        {
            var agentId = _configuration["Bedrock:AgentId"] ?? throw new ArgumentNullException("Bedrock:AgentId configuration is missing");
            var agentAliasId = _configuration["Bedrock:AgentAliasId"] ?? throw new ArgumentNullException("Bedrock:AgentAliasId configuration is missing");
            var sessionId = Guid.NewGuid().ToString();

            var request = new InvokeAgentRequest
            {
                AgentId = agentId,
                AgentAliasId = agentAliasId,
                SessionId = sessionId,
                InputText = prompt
            };

            var response = await _client.InvokeAgentAsync(request);

            await foreach (var eventStream in response.Completion)
            {
                if (eventStream is PayloadPart payloadPart)
                {
                    var chunk = Encoding.UTF8.GetString(payloadPart.Bytes.ToArray());
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        yield return chunk;
                    }
                }
            }
        }
    }
}
