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
    private readonly NetworkHelper _networkHelper;
    private readonly ITcpBroadcaster _broadcaster;
    private readonly ILogger<ClientHandler> _logger;

    public ClientHandler(ClientService service,
                         NetworkHelper networkHelper,
                         ITcpBroadcaster broadcaster,
                         ILogger<ClientHandler> logger)
    {
        this._service = service;
        this._networkHelper = networkHelper;
        this._broadcaster = broadcaster;
        this._logger = logger;
    }
    
    public async Task GetAllAsync(ClientSession session)
    {
        if (!session.IsAuthenticated)
            return;

        List<ClientResponse> responses = await _service.GetAllAsync();

        string data = JsonSerializer.Serialize(responses);

        await _networkHelper.WriteAsync(session.TcpClient.GetStream(), data);
    }

    public async Task GetByIdAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;
        
        int? id = packet.Data.Deserialize<int>();
    
        if (id is null)
        {
            _logger.LogWarning("Invalid register request format");
            return;
        }
        
        var (baseResponse, clientResponse) = await _service.GetByIdAsync((int)id);

        if(!baseResponse.Success)
        {
            _logger.LogWarning("Receipt failed");
            
            return;
        }

        _logger.LogInformation("The client was successfully received");

        Packet responsePacket = new Packet
        {
            Type = PacketType.ClientReceived,
            Data = JsonSerializer.SerializeToElement(new
            {
                BaseResponse = baseResponse,
                ClientResponse = clientResponse
            })
        };

        string responseJson = JsonSerializer.Serialize(responsePacket);

        await _networkHelper.WriteAsync(session.TcpClient.GetStream(), responseJson);
    }

    public async Task UpdateAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        UpdateClientRequest? request = packet.Data.Deserialize<UpdateClientRequest>();

        if (request is null)
        {
            _logger.LogWarning("Invalid UpdateClient request");
            return;
        }

        var (baseResponse, profile) = await _service.UpdateAsync(session.ClientId, request);

        var stream = session.TcpClient.GetStream();

        Packet responsePacket = new()
        {
            Type = PacketType.ClientUpdated,
            Data = JsonSerializer.SerializeToElement(new
            {
                baseResponse,
                profile
            })
        };

        await _networkHelper.WriteAsync(stream, JsonSerializer.Serialize(responsePacket));

        if (!baseResponse.Success || profile is null)
            return;

        Packet broadcastPacket = new()
        {
            Type = PacketType.ClientUpdated,
            Data = JsonSerializer.SerializeToElement(new ClientResponse
            {
                Id = profile.Id,
                Name = profile.Name,
                Avatar = profile.Avatar
            })
        };

        string broadcastJson = JsonSerializer.Serialize(broadcastPacket);

        await _broadcaster.BroadcastAsync(session, broadcastJson);
    }

    public async Task DeleteAsync(ClientSession session, Packet packet)
    {
        if (!session.IsAuthenticated)
            return;

        int? id = packet.Data.Deserialize<int>();

        if (id is null)
        {
            _logger.LogWarning("Invalid delete client request format");
            return;
        }
        if (session.ClientId != id)
        {
            _logger.LogWarning("A client:{ClientId} cannot make changes to another client with id:{id}",
                               session.ClientId,
                               id); 
            return;
        }

        var (response, _) = await _service.DeleteAsync((int)id);

        if(!response.Success)
        {
            _logger.LogWarning("Removal failed");
            
            return;
        }

        Packet responsePacket = new Packet
        {
            Type = PacketType.ClientDeleted,
            Data = JsonSerializer.SerializeToElement(new
            {
                Response = response,
                Id = id
            })
        };

        string responseJson = JsonSerializer.Serialize(responsePacket);

        await _networkHelper.WriteAsync(session.TcpClient.GetStream(), responseJson);
        await _broadcaster.BroadcastAsync(session, responseJson);
    }
}