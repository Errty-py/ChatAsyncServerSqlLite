using ChatAsyncServerSqlLite.Protocols;
using ChatAsyncServerSqlLite.Contracts;
using ChatAsyncServerSqlLite.Contracts.Requests;
using ChatAsyncServerSqlLite.Services;
using ChatAsyncServerSqlLite.Core.Networking;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text.Json;
namespace ChatAsyncServerSqlLite.Handlers;

public class AuthHandler
{
    private readonly AuthService _authService;
    private readonly NetworkHelper _networkHelper;
    private readonly ILogger<AuthHandler> _logger;

    public AuthHandler(AuthService authService,
                        NetworkHelper networkHelper,
                        ILogger<AuthHandler> logger)
    {
        this._authService = authService;
        this._networkHelper = networkHelper;
        this._logger = logger;
    }

    public async Task HandleRegisterAsync(ClientSession session, Packet packet)
    {
        _logger.LogInformation("Register request received from session");

        RegisterRequest? request = packet.Data
                                            .Deserialize<RegisterRequest>();

        if (request == null)
        {
            _logger.LogWarning("Invalid register request format");

            return;
        }

        var response = await _authService.RegisterAsync(request);

        if (response.Success)
        {
            _logger.LogInformation(
                "User registered successfully: {Login}",
                request.Login);
        }
        else
        {
            _logger.LogWarning(
                "Registration failed for login {Login}: {Message}",
                request.Login,
                response.Message);
        }

        byte[] data = PacketSerializer.Serialize(response);

        NetworkStream stream = session.TcpClient.GetStream();
        
        await _networkHelper.SendAsync(data, stream);
    }

    public async Task HandleLoginAsync(ClientSession session, Packet packet)
    {
        _logger.LogInformation("Login attempt received");
        
        LoginRequest? request = packet.Data
                                        .Deserialize<LoginRequest>();

        if (request == null)
        {
            _logger.LogWarning("Invalid login request format");

            return;
        }

        var response = await _authService.LoginAsync(request);

        if (response.Success)
        {
            session.ClientId = response.Id;
            session.Name = response.Name;
            session.IsAuthenticated = true;

            _logger.LogInformation(
                "Login success: {ClientId} ({Name})",
                response.Id,
                response.Name);
        }
        else
        {
            _logger.LogWarning(
                "Login failed for {Login}: {Message}",
                request.Login,
                response.Message);
        }

        byte[] data = PacketSerializer.Serialize(response);

        NetworkStream stream = session.TcpClient.GetStream();
    
        await _networkHelper.SendAsync(data, stream);
    }
}