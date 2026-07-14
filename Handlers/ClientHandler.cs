using SpaceChatServer.Core.Networking;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Contracts.Packets;
using System.Text.Json;
using SpaceChatServer.Contracts.Requests;
using Microsoft.Extensions.Logging;
using SpaceChatServer.Contracts.Responses;
using SpaceChatServer.Services;
using SpaceChatServer.Abstractions.Interfaces;

namespace SpaceChatServer.Handlers;

public class ClientHandler
{
    private readonly ClientService _service;
    private readonly SessionManager _sessionManager;
    private readonly NetworkHelper _networkHelper;
    private readonly IPacketBroadcaster _broadcaster;
    private readonly ILogger<ClientHandler> _logger;

    public ClientHandler(ClientService service,
                         SessionManager sessionManager,
                         NetworkHelper networkHelper,
                         IPacketBroadcaster broadcaster,
                         ILogger<ClientHandler> logger)
    {
        this._service = service;
        this._sessionManager = sessionManager;
        this._networkHelper = networkHelper;
        this._broadcaster = broadcaster;
        this._logger = logger;
    }
    
    public async Task GetAllAsync(ClientSession session)
    {
        if (!session.IsAuthenticated)
            return;

        var clients = await _service.GetAllAsync();
        
        List<ClientResponse> responses = 
            clients.Select(client => new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                Avatar = client.Avatar,
                IsOnline = _sessionManager.IsOnline(client.Id)
            })
            .ToList();

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.ClientList, responses));
    }

    public async Task GetByIdAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;
        
        Guid id = packet.Data.Deserialize<Guid>();
        
        var (client, error) = await _service.GetByIdAsync((Guid)id);

        if(!string.IsNullOrEmpty(error))
        {
            _logger.LogInformation("The client was not found");

            await _networkHelper.SendErrorAsync(session, PacketType.ClientReceived, error);

            return;
        }

        _logger.LogInformation("The client was successfully received");

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "The client was successfully received"
        };
        var clientResponse = new ClientResponse
        {
            Id = client!.Id,
            Name = client.Name,
            IsOnline = _sessionManager.IsOnline(client.Id),
            Avatar = client.Avatar
        };

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.ClientReceived, new
        {
            BaseResponse = baseResponse,
            ClientResponse = clientResponse
        }));
    }

    public async Task UpdateAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        var request = packet.Data.Deserialize<UpdateClientRequest>();

        if (request is null)
        {
            _logger.LogWarning("Invalid UpdateClient request");
               
            return;
        }

        var (client, error) = await _service.UpdateAsync(session.ClientId,
                                                         request.Name,
                                                         request.Login,
                                                         request.Avatar);

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogInformation("Client update failed: {Error}", 
                                   error);

            await _networkHelper.SendErrorAsync(session, PacketType.ClientUpdated, error);

            return;
        }

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Client updated"
        };
        var clientProfileResponse = new ClientProfileResponse
        {
            Id = client!.Id,
            Name = client.Name,
            Login = client.Login,
            Avatar = client.Avatar
        };
        var broadcastPacket = Packet.Create(PacketType.ClientUpdated, new ClientResponse
        {
            Id = client.Id,
            Name = client.Name,
            Avatar = client.Avatar
        });

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.ClientUpdated, new
        {
            baseResponse,
            clientProfileResponse
        }));
        await _broadcaster.BroadcastAsync(session, JsonSerializer.Serialize(broadcastPacket));
    }

    public async Task ChangePassword(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        var request = packet.Data.Deserialize<ChangePasswordRequest>();

        if (request is null)
        {
            _logger.LogWarning("Invalid ChangePassword request");
               
            return;
        }

        var (client, error) = await _service.ChangePassword(session.ClientId, request.Password);

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogInformation("User password change failed: {Error}", 
                                   error);

            await _networkHelper.SendErrorAsync(session, PacketType.ClientPasswordChanged, error);

            return;
        }

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Password changed successfully"
        };

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.ClientPasswordChanged, baseResponse));
    }

    public async Task DeleteAsync(ClientSession session)
    {
        if (!session.IsAuthenticated)
            return;

        var (clientId, error) = await _service.DeleteAsync((Guid)session.ClientId);

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogInformation("Client delete failed: {Error}", error);

            await _networkHelper.SendErrorAsync(session, PacketType.ClientDeleted, error);

            return;
        }

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "The client was deleted",
        };

        string responseJson = await _networkHelper.SendAsync(session, Packet.Create(PacketType.ClientDeleted, new
        {
            BaseResponse = baseResponse,
            Id = clientId
        }));

        await _broadcaster.BroadcastAsync(session, responseJson);
    }
}