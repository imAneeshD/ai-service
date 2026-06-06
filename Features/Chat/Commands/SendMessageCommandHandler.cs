using ai_service.Services.Interface;
using MediatR;

namespace ai_service.Features.Chat.Commands
{
    public class SendMessageCommandHandler : IStreamRequestHandler<SendMessageCommand, string>
    {
        private readonly IAIService _aiService;

        public SendMessageCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public IAsyncEnumerable<string> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            return _aiService.GenerateResponseStream(request.Message);
        }
    }
}
