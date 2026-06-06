using MediatR;

namespace ai_service.Features.Chat.Commands
{
        public record SendMessageCommand(string Message) : IRequest<string>;
}
