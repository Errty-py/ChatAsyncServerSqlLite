using TcpChatServer.Core.Networking;
using TcpChatServer.Routing;
using TcpChatServer.Core.Sessions;
using TcpChatServer.Contracts.Packets;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TcpChatServer.Handlers;

public class ConnectionHandler
{
    private readonly ClientSession _session;
    private readonly PacketRouter _router;
    private readonly NetworkHelper _networkHelper;

    public ConnectionHandler(ClientSession session,
                             PacketRouter router,
                             NetworkHelper networkHelper)
    {
        this._session = session;
        this._router = router;
        this._networkHelper = networkHelper;
    }

    public async Task HandleAsync()
    {
        try
        {
            NetworkStream stream = _session.TcpClient.GetStream();

            while (true)
            {
                string? json = await _networkHelper.ReadAsync(stream);

                if (json is null)
                    break;

                Packet? packet = JsonSerializer.Deserialize<Packet>(json);

                if (packet is null)
                    continue;

                await _router.RouteAsync(_session, packet);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            _session.TcpClient.Dispose();
        }
    }
}