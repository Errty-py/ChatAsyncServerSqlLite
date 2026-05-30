using TcpChatServer.Contracts.Packets;
using TcpChatServer.Contracts.Requests;
using TcpChatServer.Services;
using TcpChatServer.Core.Networking;
using TcpChatServer.Core.Sessions;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text.Json;
using TcpChatServer.Contracts.Responses;
using TcpChatServer.Abstractions.Interfaces;

namespace TcpChatServer.Handlers;

public class AuthHandler
{
    private readonly AuthService _authService;
    private readonly NetworkHelper _networkHelper;
    private readonly IMessageBroadcaster _broadcaster;
    private readonly ILogger<AuthHandler> _logger;

    public AuthHandler(AuthService authService,
                       NetworkHelper networkHelper,
                       IMessageBroadcaster broadcaster,
                       ILogger<AuthHandler> logger)
    {
        this._authService = authService;
        this._networkHelper = networkHelper;
        this._broadcaster = broadcaster;
        this._logger = logger;
    }

    public async Task RegistrationAsync(ClientSession session, Packet packet)
    {
        _logger.LogInformation("Register request received from session");
        
        RegisterRequest? request = packet.Data.Deserialize<RegisterRequest>();

        if (request == null)
        {
            _logger.LogWarning("Invalid register request format");

            return;
        }

        var (baseResponse, clientResponse) = await _authService.RegisterAsync(request);

        if (!baseResponse.Success)
        {
            _logger.LogWarning("Registration failed for login {Login}: {Message}",
                               request.Login,
                               baseResponse.Message);
            return;
        }

        _logger.LogInformation("User registered successfully: {Login}",
                                   request.Login);

        Packet baseResponsePacket = new Packet
        {
            Type = PacketType.ClientRegistered,
            Data = JsonSerializer.SerializeToElement(baseResponse)
        };
        Packet clientResponsePacket = new Packet
        {
            Type = PacketType.ClientRegistered,
            Data = JsonSerializer.SerializeToElement(clientResponse)
        };

        string baseResponseJson = JsonSerializer.Serialize(baseResponsePacket);
        string clientResponseJson = JsonSerializer.Serialize(clientResponsePacket);

        await _networkHelper.WriteAsync(session.TcpClient.GetStream(), baseResponseJson);
        await _broadcaster.BroadcastAsync(session, clientResponseJson);
    }

    public async Task LoginAsync(ClientSession session, Packet packet)
    {
        _logger.LogInformation("Login attempt received");

        LoginRequest? request = packet.Data.Deserialize<LoginRequest>();

        if (request == null)
        {
            _logger.LogWarning("Invalid login request format");

            return;
        }

        var (baseResponse, clientResponse) = await _authService.LoginAsync(request);

        if (baseResponse.Success && clientResponse is not null)
        {
            session.ClientId = clientResponse.Id;
            session.Name = clientResponse.Name;
            session.IsAuthenticated = true;

            _logger.LogInformation("Login success: {ClientId} ({Name})",
                                   clientResponse.Id,
                                   clientResponse.Name);
        }
        else
        {
            _logger.LogWarning("Login failed for {Login}: {Message}",
                               request.Login,
                               baseResponse.Message);
        }

        Packet responsePacket = new Packet
        {
            Type = PacketType.ClientLogged,
            Data = JsonSerializer.SerializeToElement(new
            {
                BaseResponse = baseResponse,
                ClientResponse = clientResponse
            })
        };

        string responseJson = JsonSerializer.Serialize(responsePacket);
    
        await _networkHelper.WriteAsync(session.TcpClient.GetStream(), responseJson);
    }
}