using SpaceChatServer.Core.Networking;
using SpaceChatServer.Routing;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Contracts.Packets;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SpaceChatServer.Handlers;

public class ConnectionHandler
{
    private readonly ClientSession _session;
    private readonly PacketRouter _router;
    private readonly NetworkHelper _networkHelper;
    private readonly ILogger<ConnectionHandler> _logger;

    public ConnectionHandler(ClientSession session,
                             PacketRouter router,
                             NetworkHelper networkHelper,
                             ILogger<ConnectionHandler> logger)
    {
        _session = session;
        _router = router;
        _networkHelper = networkHelper;
        _logger = logger;
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
            _session.TcpClient.Dispose();
        }
    }
}