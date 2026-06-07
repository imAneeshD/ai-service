using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ai_service.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMediator _mediator;

        public ChatHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SendMessage(string message)
        {
            var stream = _mediator.CreateStream(new Features.Chat.Commands.SendMessageCommand(message));

            await foreach (var chunk in stream)
            {
                await Clients.Caller.SendAsync("ReceiveMessageChunk", chunk);
            }

            await Clients.Caller.SendAsync("ReceiveMessageEnd");
        }
    }
}
