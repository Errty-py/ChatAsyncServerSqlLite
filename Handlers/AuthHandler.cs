using SpaceChatServer.Contracts.Packets;
using SpaceChatServer.Contracts.Requests;
using SpaceChatServer.Services;
using SpaceChatServer.Core.Networking;
using SpaceChatServer.Core.Sessions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using SpaceChatServer.Contracts.Responses;
using SpaceChatServer.Abstractions.Interfaces;

namespace SpaceChatServer.Handlers;

public class AuthHandler
{
    private readonly AuthService _authService;
    private readonly NetworkHelper _networkHelper;
    private readonly ITcpBroadcaster _broadcaster;
    private readonly ILogger<AuthHandler> _logger;

    public AuthHandler(AuthService authService,
                       NetworkHelper networkHelper,
                       ITcpBroadcaster broadcaster,
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

        Packet baseResponsePacket = new Packet
        {
            Type = PacketType.ClientRegistered,
            Data = JsonSerializer.SerializeToElement(baseResponse)
        };

        string baseResponseJson = JsonSerializer.Serialize(baseResponsePacket);

        await _networkHelper.WriteAsync(session.TcpClient.GetStream(), baseResponseJson);

        if (!baseResponse.Success)
        {
            _logger.LogWarning("Registration failed for login {Login}: {Message}",
                               request.Login,
                               baseResponse.Message);
            return;
        }

        _logger.LogInformation("User registered successfully: {Login}",
                                   request.Login);

        Packet clientResponsePacket = new Packet
        {
            Type = PacketType.ClientRegistered,
            Data = JsonSerializer.SerializeToElement(clientResponse)
        };
        
        string clientResponseJson = JsonSerializer.Serialize(clientResponsePacket);
        
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
        
        if (!baseResponse.Success && clientResponse is null)
        {    
            _logger.LogWarning("Login failed for {Login}: {Message}",
                               request.Login,
                               baseResponse.Message);

            return;
        }
        
        _logger.LogInformation("Login success: {ClientId} ({Name})",
                               clientResponse.Id,
                               clientResponse.Name);

        session.ClientId = clientResponse.Id;
        session.Name = clientResponse.Name;
        session.IsAuthenticated = true;

        ClientStatusResponse statusResponse = new()
        {
            ClientId = session.ClientId,
            IsOnline = true
        };
        
        Packet broadcastPacket = new()
        {
            Type = PacketType.ClientStatusChanged,
            Data = JsonSerializer.SerializeToElement(statusResponse)
        };

        await _broadcaster.BroadcastAsync(session, JsonSerializer.Serialize(broadcastPacket));
    }
}