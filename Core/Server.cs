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

        
        while (_isRunning)
        {
            TcpClient tcpClient =
                await _listener.AcceptTcpClientAsync();

            _ = HandleClientAsync(tcpClient);
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        ClientSession session = new()
        {
            TcpClient = tcpClient
        };

        _sessionManager.Add(session);

        _logger.LogInformation("Client connected from {Endpoint}",
                               tcpClient.Client.RemoteEndPoint);

        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            ConnectionHandler processor =
                ActivatorUtilities.CreateInstance<ConnectionHandler>(
                    scope.ServiceProvider,
                    session);

            await processor.HandleAsync();
        }
        finally
        {
            _sessionManager.Remove(session.SessionId);

            tcpClient.Close();

            _logger.LogInformation("Client disconnected {ClientId}",
                                   session.ClientId);
        }
    }

    public async Task StopAsync()
    {
        _isRunning = false;

        _listener.Stop();

        var sessions = _sessionManager.GetAll();

        if(sessions is null)
            return;

        foreach (ClientSession session in sessions)
        {
            session.TcpClient.Close();
        }

        _logger.LogInformation("Server stoped");
        
        await Task.CompletedTask;
    }
}
