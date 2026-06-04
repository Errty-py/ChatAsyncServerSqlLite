using SpaceChatServer.Core.Networking;
using SpaceChatServer.Routing;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Contracts.Packets;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SpaceChatServer.Contracts.Responses;
using SpaceChatServer.Abstractions.Interfaces;

namespace SpaceChatServer.Handlers;

public class ConnectionHandler
{
    private readonly ClientSession _session;
    private readonly SessionManager _sessionManager;
    private readonly PacketRouter _router;
    private readonly NetworkHelper _networkHelper;
    private readonly ITcpBroadcaster _broadcaster;
    private readonly ILogger<ConnectionHandler> _logger;

    public ConnectionHandler(ClientSession session,
                             SessionManager sessionManager,
                             PacketRouter router,
                             NetworkHelper networkHelper,
                             ITcpBroadcaster broadcaster,
                             ILogger<ConnectionHandler> logger)
    {
        this._session = session;
        this._sessionManager = sessionManager;
        this._router = router;
        this._networkHelper = networkHelper;
        this._broadcaster = broadcaster;
        this._logger = logger;
    }

    public async Task HandleAsync()
    {
        try
        {
            var stream = _session.TcpClient.GetStream();

            while (true)
            {
                string? json = await _networkHelper.ReadAsync(stream);

                if (json is null)
                    break;

                Packet? packet = JsonSerializer.Deserialize<Packet>(json);

                if (packet is null)
                {
                    _logger.LogWarning("Failed to deserialize packet from client {ClientId}",
                                       _session.ClientId);

                    continue;
                }

                await _router.RouteAsync(_session, packet);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "Error while processing client {ClientId}",
                             _session.ClientId);
        }
        finally
        {
            bool wasAuthenticated = _session.IsAuthenticated;

            if (wasAuthenticated)
            {
                ClientStatusResponse response = new()
                {
                    ClientId = _session.ClientId,
                    IsOnline = false
                };

                Packet packet = new()
                {
                    Type = PacketType.ClientStatusChanged,
                    Data = JsonSerializer.SerializeToElement(response)
                };

                await _broadcaster.BroadcastAsync(_session, JsonSerializer.Serialize(packet));
            }
        }
    }
}