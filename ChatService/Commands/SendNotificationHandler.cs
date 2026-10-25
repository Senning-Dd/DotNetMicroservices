using ChatService.Api.Commands;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Commands;

public class SendNotificationHandler : IRequestHandler<SendNotificationCommand, Unit>
{
    private readonly IHubContext<Hubs.AgentChatHub> chatHubContext;

    public SendNotificationHandler(IHubContext<Hubs.AgentChatHub> chatHubContext)
    {
        this.chatHubContext = chatHubContext;
    }

    public async Task<Unit> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        await chatHubContext.Clients.All.SendAsync("ReceiveMessage", "system", request.Message, cancellationToken);
        return Unit.Value;
    }
}
