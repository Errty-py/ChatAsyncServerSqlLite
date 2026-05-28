using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Contracts.Requests;
using TcpChatServer.Contracts.Responses;
using TcpChatServer.Core.Sessions;
using TcpChatServer.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace TcpChatServer.Services;

public class MessageService
{
    private readonly IMessageRepository _repository;
    private readonly IMessageBroadcaster _broadcaster;
    
    public MessageService(IMessageRepository repository, 
                          IMessageBroadcaster broadcaster)
    {
        this._repository = repository;
        this._broadcaster = broadcaster;
    }

    public async Task SendAsync(MessageRequest request, ClientSession session)
    {
        if (!session.IsAuthenticated)
            return;
        
        MessageEntity message = new MessageEntity()
        {
            FromClientId = session.ClientId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(message);

        MessageResponse response = new MessageResponse()
        {
            FromClientId = message.FromClientId,
            SenderName = session.Name,
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };

        string data = JsonSerializer.Serialize(response);

        await _broadcaster.BroadcastAsync(data, session);
    }

    public async Task<List<MessageResponse>> GetAllAsync(ClientSession session)
    {
        if (!session.IsAuthenticated)
            return [];

        List<MessageEntity> messages = await _repository.GetAllAsync();

        List<MessageResponse> responses =
            messages.Select(message => new MessageResponse
            {
                FromClientId = message.FromClientId,
                SenderName = message.FromClient.Login,
                Text = message.Text,
                CreatedAt = message.CreatedAt
            })
            .ToList();

        return responses;
    }
}