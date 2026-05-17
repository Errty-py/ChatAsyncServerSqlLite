using TcpChatServer.Core.Networking;
using TcpChatServer.Handlers;
using TcpChatServer.Routing;
using TcpChatServer.Core.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;

namespace TcpChatServer.Core;

public class Server
{
    private readonly IPEndPoint _iPEndPoint;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Server> _logger;
    private readonly SessionManager _sessionManager;
    private readonly TcpListener _listener;
    private bool _isRunning;

    public Server(IPEndPoint iPEndPoint,
                  SessionManager sessionManager,
                  IServiceScopeFactory scopeFactory,
                  ILogger<Server> logger)
    {
        this._iPEndPoint = iPEndPoint;
        this._listener = new TcpListener(_iPEndPoint);

        this._sessionManager = sessionManager;
        this._scopeFactory = scopeFactory;
        this._logger = logger;
    }

    public async Task StartAsync()
    {
        _isRunning = true;

        _listener.Start();
        
        _logger.LogInformation("Server started on port {Port}", _iPEndPoint.Port);

        
        try
        {
            while(_isRunning)
            {
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();

                ClientSession session = new ClientSession()
                {
                    TcpClient = tcpClient
                };

                _sessionManager.Add(session);
                
                _logger.LogInformation("Client connected from {Endpoint}",
                                        tcpClient.Client.RemoteEndPoint);

                _ = Task.Run(async () =>
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();

                    var router = scope.ServiceProvider.GetRequiredService<PacketRouter>();
                    var networkHelper = scope.ServiceProvider.GetRequiredService<NetworkHelper>();

                    var handler = new ClientHandler(session, router, networkHelper);

                    try
                    {
                        await handler.HandleAsync();
                    }
                    finally
                    {
                        _sessionManager.Remove(session.SessionId);
                        tcpClient.Close();
                        _logger.LogInformation("Client disconnected {ClientId}", session.ClientId);
                    }
                });
            }
        }
        catch (SocketException)
        {
            if (_isRunning)
            {
                _logger.LogError("Unexpected socket error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Server ERROR");
        }
    }

    public async Task StopAsync()
    {
        _isRunning = false;

        _listener.Stop();

        foreach (ClientSession session in _sessionManager.GetAll())
        {
            session.TcpClient.Close();
        }

        _logger.LogInformation("Server stoped");
        await Task.CompletedTask;
    }
}
