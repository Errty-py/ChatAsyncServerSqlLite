using ChatAsyncServerSqlLite.Contracts.Requests;
using ChatAsyncServerSqlLite.Contracts.Packets;
using ChatAsyncServerSqlLite.Core.Sessions;
using ChatAsyncServerSqlLite.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ChatAsyncServerSqlLite.Contracts.Responses;
using System.Net.Sockets;
using ChatAsyncServerSqlLite.Core.Networking;

namespace ChatAsyncServerSqlLite.Handlers;

public class MessageHandler
{
    private readonly MessageService _messageService;
    private readonly NetworkHelper _networkHelper;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(MessageService messageService,
                          NetworkHelper networkHelper, 
                          ILogger<MessageHandler> logger)
    {
        this._messageService = messageService;
        this._networkHelper = networkHelper;
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

    public async Task SendAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        _logger.LogInformation("Message packet received from client {ClientId}",
                               session.ClientId);
                               
        MessageRequest? request = packet.Data.Deserialize<MessageRequest>();

        if (request is null)
        {
            _logger.LogWarning(
                "Invalid SendMessageRequest");

            return;
        }

        await _messageService.SendAsync(
            request,
            session);
    }

    public async Task GetAllAsync(ClientSession session)
    {
        List<MessageResponse> response = await _messageService.GetAllAsync(session);

        string data = JsonSerializer.Serialize(response);

        NetworkStream stream = session.TcpClient.GetStream();
    
        await _networkHelper.WriteAsync(stream, data);
    }
}