using ai_service.Services.Interface;
using MediatR;

namespace ai_service.Features.Chat.Commands
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, string>
    {
        private readonly IAIService _aiService;

        public SendMessageCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<string> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            return await _aiService.GenerateResponse(request.Message);
        }
    }
}
