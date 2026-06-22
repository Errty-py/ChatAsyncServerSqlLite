using SpaceChatServer.Contracts.Requests;
using SpaceChatServer.Contracts.Packets;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using SpaceChatServer.Contracts.Responses;
using System.Net.Sockets;
using SpaceChatServer.Core.Networking;
using Microsoft.EntityFrameworkCore.Query;
using SpaceChatServer.Abstractions.Interfaces;

namespace SpaceChatServer.Handlers;

public class MessageHandler
{
    private readonly MessageService _service;
    private readonly NetworkHelper _networkHelper;
    private readonly ITcpBroadcaster _broadcaster;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(MessageService messageService,
                          NetworkHelper networkHelper,
                          ITcpBroadcaster broadcaster, 
                          ILogger<MessageHandler> logger)
    {
        this._service = messageService;
        this._networkHelper = networkHelper;
        this._broadcaster = broadcaster;
        this._logger = logger;
    }
    
    public async Task SendAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
        {
            _logger.LogWarning("Unauthenticated client attempted to send message");

            return;
        }
        
        _logger.LogInformation("Message send request received from client {ClientId}",
                               session.ClientId);
                               
        MessageRequest? request = packet.Data.Deserialize<MessageRequest>();

        if (request is null)
        {
            _logger.LogWarning("Invalid MessageRequest received from client {ClientId}",
                               session.ClientId);
            return;
        }

        var (baseResponse, messageResponse) = await _service.AddAsync(session, request);
        
        NetworkStream stream = session.TcpClient.GetStream();

        Packet baseResponsePacket = new Packet()
        {
            Type = PacketType.MessageAdded,
            Data = JsonSerializer.SerializeToElement(baseResponse)
        };

        string baseResponseJson = JsonSerializer.Serialize(baseResponsePacket);

        await _networkHelper.WriteAsync(stream, baseResponseJson);

        if(!baseResponse.Success)
        {
            _logger.LogWarning("Message send failed for client {ClientId}: {Message}",
                               session.ClientId,
                               baseResponse.Message);                
            return;
        }

        _logger.LogInformation("Message {MessageId} created by client {ClientId}",
                               messageResponse!.Id,
                               session.ClientId);

        Packet messageResponsePacket = new Packet()
        {
            Type = PacketType.MessageReceived,
            Data = JsonSerializer.SerializeToElement(messageResponse)
        };

        string messageResponseJson = JsonSerializer.Serialize(messageResponsePacket);

        _logger.LogInformation("Message {MessageId} broadcasted to connected clients",
                               messageResponse.Id);

        await _broadcaster.BroadcastAsync(session, messageResponseJson);
    }

    public async Task GetAllAsync(ClientSession session)
    {
        if (!session.IsAuthenticated)
        {
            _logger.LogWarning("An unauthenticated client attempted to retrieve message history");

            return;
        }

        _logger.LogInformation("Message history requested by client {ClientId}",
                               session.ClientId);


        List<MessageResponse> response = await _service.GetAllAsync();

        _logger.LogInformation("{Count} messages loaded for client {ClientId}",
                               response.Count,
                               session.ClientId);

        Packet packet = new Packet()
        {
            Type = PacketType.MessageHistoryReceived,
            Data = JsonSerializer.SerializeToElement(response)
        };

        string data = JsonSerializer.Serialize(packet);

        NetworkStream stream = session.TcpClient.GetStream();
    
        await _networkHelper.WriteAsync(stream, data);

        _logger.LogInformation("Message history sent to client {ClientId}",
                               session.ClientId);
    }

    public async Task DeleteAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
        {
            _logger.LogWarning("Unauthenticated client attempted to delete message");

            return;
        }

        _logger.LogInformation("Delete message request received from client {ClientId}",
                               session.ClientId);

        Guid? messageId = packet.Data.Deserialize<Guid>();

        if (messageId is null)
        {
            _logger.LogWarning("Invalid message ID format");
            return;
        }

        _logger.LogInformation("Client {ClientId} requested deletion of message {MessageId}",
                        session.ClientId,
                        messageId);

        BaseResponse response = await _service.DeleteAsync(session, messageId.Value);

        string data = JsonSerializer.Serialize(response);

        NetworkStream stream = session.TcpClient.GetStream();

        await _networkHelper.WriteAsync(stream, data);

        if (!response.Success)
        {
            _logger.LogWarning(
                "Message deletion failed for client {ClientId}: {Reason}",
                session.ClientId,
                response.Message);

            return;
        }

        _logger.LogInformation("Message {MessageId} deleted by client {ClientId}",
                               messageId,
                               session.ClientId);

        Packet deletedPacket = new Packet
        {
            Type = PacketType.MessageDeleted,
            Data = JsonSerializer.SerializeToElement(messageId)
        };

        string deletedJson = JsonSerializer.Serialize(deletedPacket);

        _logger.LogInformation("MessageDeleted event broadcasted for message {MessageId}",
                               messageId);

        await _broadcaster.BroadcastAsync(session, deletedJson);       
    }
}