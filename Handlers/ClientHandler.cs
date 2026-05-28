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

namespace TcpChatServer.Handlers;

public class ClientHandler
{
    private readonly ClientService _service;
    private readonly NetworkHelper _networkHelper;
    private readonly Logger<ClientHandler> _logger;

    public ClientHandler(ClientService service,
                         NetworkHelper networkHelper,
                         Logger<ClientHandler> logger)
    {
        this._service = service;
        this._networkHelper = networkHelper;
        this._logger = logger;
    }
    
    public async Task GetAllAsync(ClientSession session)
    {
        if (!session.IsAuthenticated)
            return;

        List<ClientResponse> responses = await _service.GetAllAsync();

        string data = JsonSerializer.Serialize(responses);

        NetworkStream stream = session.TcpClient.GetStream();

        await _networkHelper.WriteAsync(stream, data);
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
        
        ClientResponse response = await _service.GetByIdAsync((int)id);

        if(response.Success)
            _logger.LogInformation("The client was successfully received");
        else
            _logger.LogWarning("Receipt failed");

        string data = JsonSerializer.Serialize(response);

        NetworkStream stream = session.TcpClient.GetStream();

        await _networkHelper.WriteAsync(stream, data);
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
        

        BaseResponse response = await _service.DeleteAsync((int)id);

        if(response.Success)
            _logger.LogInformation("The client was successfully removed.");
        
        else
            _logger.LogWarning("Removal failed");

        string data = JsonSerializer.Serialize(response);

        NetworkStream stream = session.TcpClient.GetStream();

        await _networkHelper.WriteAsync(stream, data);
    }
}