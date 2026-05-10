using ChatAsyncServerSqlLite.Protocols;
using ChatAsyncServerSqlLite.Core.Networking;
using ChatAsyncServerSqlLite.Routing;
using ChatAsyncServerSqlLite.Core.Sessions;
using ChatAsyncServerSqlLite.Contracts.Packets;
using System.Net.Sockets;

namespace ChatAsyncServerSqlLite.Handlers;

public class ClientHandler
{
    private readonly PacketRouter _router;
    private readonly NetworkHelper _networkHelper;
    private readonly ClientSession _session;

    public ClientHandler(ClientSession session,
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
                byte[] data = await _networkHelper.ReadAsync(stream);

                if (data.Length == 0)
                    break;

                Packet? packet = PacketSerializer.Deserialize<Packet>(data);

                if (packet == null)
                    continue;

                await _router.RouteAsync(_session, packet);
            }
        }
        finally
        {
            _session.TcpClient.Close();
        }
    }
}