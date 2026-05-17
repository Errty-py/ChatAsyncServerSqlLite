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
    private readonly ILogger<MessageService> _logger;
    
    public MessageService(IMessageRepository repository, 
                          IMessageBroadcaster broadcaster,
                          ILogger<MessageService> logger)
    {
        this._repository = repository;
        this._broadcaster = broadcaster;
        this._logger = logger;
    }

    public async Task SendAsync(MessageRequest request, ClientSession session)
    {
        if (!session.IsAuthenticated || session.ClientId is null)
        {
            _logger.LogWarning(
                "Unauthenticated session {SessionId} tried to send message",
                session.SessionId);

            return;
        }
        
        MessageEntity message = new MessageEntity()
        {
            FromClientId = session.ClientId.Value,
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

        _logger.LogInformation("Message from client {ClientId} broadcasted",
                               session.ClientId);

        await _broadcaster.BroadcastAsync(data, session);
    }

    public async Task<List<MessageResponse>> GetAllAsync(ClientSession session)
    {
        if (!session.IsAuthenticated || session.ClientId is null)
        {
            _logger.LogWarning(
                "Unauthenticated session {SessionId} tried to get messages",
                session.SessionId);

            return [];
        }

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

        _logger.LogInformation(
            "Client {ClientId} received {Count} messages",
            session.ClientId,
            responses.Count);

        return responses;
    }
}