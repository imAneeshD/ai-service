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
            var response = await _mediator.Send(new Features.Chat.Commands.SendMessageCommand(message));

            await Clients.Caller.SendAsync("ReceiveMessage", response);
        }
    }
}
