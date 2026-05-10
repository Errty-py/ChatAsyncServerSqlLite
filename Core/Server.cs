using ChatAsyncServerSqlLite.Core.Networking;
using ChatAsyncServerSqlLite.Handlers;
using ChatAsyncServerSqlLite.Routing;
using ChatAsyncServerSqlLite.Core.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;

namespace ChatAsyncServerSqlLite.Core;

public class Server
{
    private readonly IPEndPoint _iPEndPoint;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Server> _logger;
    private readonly SessionManager _sessionManager;
    private readonly TcpListener _listener;

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
        _listener.Start();
        _logger.LogInformation("Server started on port {Port}", _iPEndPoint.Port);

        while (true)
        {
            try
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
                        _sessionManager.Remove(session);
                        tcpClient.Close();
                        _logger.LogInformation("Client disconnected {ClientId}", session.ClientId);
                    }
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Accept client failed");
            }
        }
    }

    public async Task StopAsync()
    {
        _listener.Stop();

        foreach (ClientSession session in _sessionManager.GetAll())
        {
            session.TcpClient.Close();
        }

        _logger.LogInformation("Server stoped");
        await Task.CompletedTask;
    }
}
