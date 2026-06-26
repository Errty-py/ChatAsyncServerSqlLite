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

        var packet = new Packet
        {
            Type = PacketType.ClientList,
            Data = JsonSerializer.SerializeToElement(responses)
        };

        string json = JsonSerializer.Serialize(packet);
        var stream = session.TcpClient.GetStream();

        await _networkHelper.WriteAsync(stream, json);
    }

    public async Task GetByIdAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;
        
        Guid id = packet.Data.Deserialize<Guid>();
        
        var (client, error) = await _service.GetByIdAsync((Guid)id);
        var stream = session.TcpClient.GetStream();

        if(!string.IsNullOrEmpty(error))
        {
            _logger.LogInformation("The client was not found");
            
            var errorResponse = new BaseResponse
            {
                Success = false,
                Message = error
            };
            var errorResponsePacket = new Packet
            {
                Type = PacketType.ClientReceived,
                Data = JsonSerializer.SerializeToElement(errorResponse)
            };

            string errorResponseJson = JsonSerializer.Serialize(errorResponsePacket);

            await _networkHelper.WriteAsync(stream, errorResponseJson);

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
        var responsePacket = new Packet
        {
            Type = PacketType.ClientReceived,
            Data = JsonSerializer.SerializeToElement(new
            {
                BaseResponse = baseResponse,
                ClientResponse = clientResponse
            })
        };

        string responseJson = JsonSerializer.Serialize(responsePacket);

        await _networkHelper.WriteAsync(stream, responseJson);
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
                                                         request.PasswordHash,
                                                         request.Avatar);
        var stream = session.TcpClient.GetStream();
        
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogInformation("Client update failed: {Error}", 
                                   error);
            
            var errorBaseResponse = new BaseResponse
            {
                Success = false,
                Message = error
            };
            var errorResponsePacket = new Packet
            {
                Type = PacketType.ClientUpdated,
                Data = JsonSerializer.SerializeToElement(errorBaseResponse)
            };

            string errorResponseJson = JsonSerializer.Serialize(errorResponsePacket);    

            await _networkHelper.WriteAsync(stream, errorResponseJson);

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
        var responsePacket = new Packet
        {
            Type = PacketType.ClientUpdated,
            Data = JsonSerializer.SerializeToElement(new
            {
                baseResponse,
                clientProfileResponse
            })
        };
        var broadcastPacket = new Packet
        {
            Type = PacketType.ClientUpdated,
            Data = JsonSerializer.SerializeToElement(new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                Avatar = client.Avatar
            })
        };

        string responseJson = JsonSerializer.Serialize(responsePacket);
        string broadcastJson = JsonSerializer.Serialize(broadcastPacket);

        await _networkHelper.WriteAsync(stream, responseJson);
        await _broadcaster.BroadcastAsync(session, broadcastJson);
    }

    public async Task DeleteAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        var (clientId, error) = await _service.DeleteAsync((Guid)session.ClientId);
        var stream = session.TcpClient.GetStream();

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogInformation("Client delete failed: {Error}", error);

            var errorResponse = new BaseResponse
            {
                Success = false,
                Message = error
            };
            var errorResponsePacket = new Packet
            {
                Type = PacketType.ClientDeleted,
                Data = JsonSerializer.SerializeToElement(errorResponse)
            };

            string errorResponseJson = JsonSerializer.Serialize(errorResponsePacket);

            await _networkHelper.WriteAsync(stream, errorResponseJson);

            return;
        }

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "The client was deleted",
        };
        var responsePacket = new Packet
        {
            Type = PacketType.ClientDeleted,
            Data = JsonSerializer.SerializeToElement(new
            {
                BaseResponse = baseResponse,
                Id = clientId
            })
        };

        string responseJson = JsonSerializer.Serialize(responsePacket);

        await _networkHelper.WriteAsync(stream, responseJson);
        await _broadcaster.BroadcastAsync(session, responseJson);
    }
}