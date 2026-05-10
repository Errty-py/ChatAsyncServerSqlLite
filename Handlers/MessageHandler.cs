using ChatAsyncServerSqlLite.Contracts.Requests;
using ChatAsyncServerSqlLite.Contracts.Packets;
using ChatAsyncServerSqlLite.Core.Sessions;
using ChatAsyncServerSqlLite.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ChatAsyncServerSqlLite.Handlers;

public class MessageHandler
{
    private readonly MessageService _messageService;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(MessageService messageService, 
                          ILogger<MessageHandler> logger)
    {
        this._messageService = messageService;
        this._logger = logger;
    }

    public async Task HandleAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        _logger.LogInformation("Message packet received from client {ClientId}",
                               session.ClientId);

        MessageRequest? request = packet.Data.Deserialize<MessageRequest>();

        if (request == null)
        {
            _logger.LogWarning("Invalid message request from client {ClientId}",
                               session.ClientId);
            
            return;
        }
        
        await _messageService.SendAsync(request, session);
    }
}