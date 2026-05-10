using ChatAsyncServerSqlLite.Contracts.Requests;
using ChatAsyncServerSqlLite.Contracts.Responses;
using ChatAsyncServerSqlLite.Contracts;
using ChatAsyncServerSqlLite.Services;
using System.Text.Json;

namespace ChatAsyncServerSqlLite.Handlers;

public class MessageHandler
{
    private readonly MessageService _messageService;

    public MessageHandler(MessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task HandleAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        _logger.LogInformation("Message packet received from client {ClientId}",
                               session.ClientId);

        MessageRequest? request =
            packet.Data.Deserialize<MessageRequest>();

        if (request == null)
        {
            _logger.LogWarning("Invalid message request from client {ClientId}",
                               session.ClientId);
            
            return;
        }
        
        await _messageService.SendAsync(request, session);
    }
}