using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Contracts.Requests;
using ChatAsyncServerSqlLite.Contracts.Responses;
using ChatAsyncServerSqlLite.Contracts;
using ChatAsyncServerSqlLite.Data.Entities;
using ChatAsyncServerSqlLite.Protocols;
using Microsoft.Extensions.Logging;

namespace ChatAsyncServerSqlLite.Services;

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
        MessageEntity message = new()
        {
            FromClientId = session.ClientId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(message);

        MessageResponse response = new()
        {
            FromClientId = session.ClientId,
            SenderName = session.Name,
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };

        byte[] data = PacketSerializer.Serialize(response);

        _logger.LogInformation("Message from client {ClientId} broadcasted",
                               session.ClientId);

        await _broadcaster.BroadcastAsync(data, session);
    }
}

