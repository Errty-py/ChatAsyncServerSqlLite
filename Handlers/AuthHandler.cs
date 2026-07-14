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
    private readonly IPacketBroadcaster _broadcaster;
    private readonly ILogger<AuthHandler> _logger;
    
    public AuthHandler(AuthService authService,
                       NetworkHelper networkHelper,
                       IPacketBroadcaster broadcaster,
                       ILogger<AuthHandler> logger)
    {
        this._authService = authService;
        this._networkHelper = networkHelper;
        this._broadcaster = broadcaster;
        this._logger = logger;
    }

    public async Task RegistrationAsync(ClientSession session, Packet packet)
    {   
        RegisterRequest? request = packet.Data.Deserialize<RegisterRequest>();

        if (request == null)
        {
            _logger.LogWarning("Invalid register request format");

            return;
        }

        var (client, error) = await _authService.RegisterAsync(request.Name,
                                                               request.Login,
                                                               request.Password);

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Registration failed for login {Login}: {Error}",
                               request.Login,
                               error);

            await _networkHelper.SendErrorAsync(session, PacketType.ClientRegistered, error);

            return;
        }

        _logger.LogInformation("User registered successfully: {Login}",
                               request.Login);

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Registered"
        };
        var clientResponse = new ClientResponse
        {
            Id = client!.Id,
            Name = client.Name
        };

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.ClientRegistered, baseResponse));

        string clientResponseJson = JsonSerializer.Serialize(Packet.Create(PacketType.ClientRegistered, clientResponse));

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

        var (client, error) = await _authService.LoginAsync(request.Login,
                                                            request.Password);

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Login failed for {Login}: {Error}",
                               request.Login,
                               error);

            await _networkHelper.SendErrorAsync(session, PacketType.ClientLogged, error);

            return;
        }

        var baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Logged in"
        };
        var clientResponse = new ClientResponse
        {
            Id = client!.Id,
            Name = client.Name,
            IsOnline = true,
            Avatar = client.Avatar
        };

        await _networkHelper.SendAsync(session, Packet.Create(PacketType.ClientLogged, new
        {
            BaseResponse = baseResponse,
            ClientResponse = clientResponse
        }));

        _logger.LogInformation("Login success: {ClientId} ({Name})",
                               clientResponse!.Id,
                               clientResponse.Name);

        session.ClientId = clientResponse.Id;
        session.ClientName = clientResponse.Name;
        session.IsAuthenticated = true;

        var statusResponse = new ClientStatusResponse
        {
            ClientId = session.ClientId,
            IsOnline = true
        };
        var broadcastPacket = Packet.Create(PacketType.ClientStatusChanged, statusResponse);

        await _broadcaster.BroadcastAsync(session, JsonSerializer.Serialize(broadcastPacket));
    }
}