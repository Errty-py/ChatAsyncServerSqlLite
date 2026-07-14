using SpaceChatServer.Contracts.Requests;
using SpaceChatServer.Contracts.Packets;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using SpaceChatServer.Contracts.Responses;
using SpaceChatServer.Core.Networking;
using SpaceChatServer.Abstractions.Interfaces;

namespace SpaceChatServer.Handlers;

public class MessageHandler
{
    private readonly MessageService _service;
    private readonly NetworkHelper _networkHelper;
    private readonly IPacketBroadcaster _broadcaster;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(MessageService messageService,
                          NetworkHelper networkHelper,
                          IPacketBroadcaster broadcaster,
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

        var (message, error) = await _service.AddAsync(session.ClientId, request.Text);

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Message send failed for client {ClientId}: {Error}",
                               session.ClientId,
                               error);

            await _networkHelper.SendErrorAsync(session, PacketType.MessageAdded, error);

            return;
        }

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Message sent successfully"
        };
        var messageResponse = new MessageResponse
        {
            Id = message!.Id,
            FromClientId = message.FromClientId,
            SenderName = session.ClientName,
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };
        var messageResponsePacket = Packet.Create(PacketType.MessageReceived, messageResponse);

        string messageResponseJson = JsonSerializer.Serialize(messageResponsePacket);

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.MessageAdded, new
        {
            baseResponse,
            messageResponse
        }));
        await _broadcaster.BroadcastAsync(session, messageResponseJson);

        _logger.LogInformation("Message {MessageId} created by client {ClientId}",
                               messageResponse!.Id,
                               session.ClientId);

        _logger.LogInformation("Message {MessageId} broadcasted to connected clients",
                               messageResponse.Id);
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

        var messages = await _service.GetAllAsync();

        List<MessageResponse> responses =
            messages.Select(message => new MessageResponse
            {
                Id = message.Id,
                FromClientId = message.FromClientId,
                SenderName = session.ClientName,
                Text = message.Text,
                CreatedAt = message.CreatedAt
            })
            .ToList();

        _logger.LogInformation("{Count} messages loaded for client {ClientId}",
                               messages.Count,
                               session.ClientId);

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.MessageHistoryReceived, responses));

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

        var (id, error) = await _service.DeleteAsync(session.ClientId, messageId.Value);

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Message deletion failed for client {ClientId}: {Error}",
                               session.ClientId,
                               error);

            await _networkHelper.SendErrorAsync(session, PacketType.MessageDeleted, error);

            return;
        }

        _logger.LogInformation("Message {MessageId} deleted by client {ClientId}",
                               messageId,
                               session.ClientId);

        string deletedJson = await _networkHelper.SendAsync(session, Packet.Create(PacketType.MessageDeleted, messageId));

        await _broadcaster.BroadcastAsync(session, deletedJson);

        _logger.LogInformation("MessageDeleted response sent to client {ClientId} for message {MessageId}",
                               session.ClientId,
                               messageId);
        _logger.LogInformation("MessageDeleted event broadcasted for message {MessageId}",
                               messageId);
    }
}
