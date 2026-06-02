using TcpChatServer.Core.Networking;
using TcpChatServer.Routing;
using TcpChatServer.Core.Sessions;
using TcpChatServer.Contracts.Packets;
using System.Net.Sockets;
using System.Text.Json;
using TcpChatServer.Contracts.Requests;
using Microsoft.Extensions.Logging;
using TcpChatServer.Contracts.Responses;
using TcpChatServer.Services;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.AspNetCore.Mvc;
using TcpChatServer.Abstractions.Interfaces;

namespace TcpChatServer.Handlers;

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

        if(baseResponse.Success)
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