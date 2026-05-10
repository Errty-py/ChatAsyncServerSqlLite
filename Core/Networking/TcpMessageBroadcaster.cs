using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Contracts;

namespace ChatAsyncServerSqlLite.Core.Networking;

public class MessageBroadcaster : IMessageBroadcaster
{
    private readonly SessionManager _sessionManager;

    public MessageBroadcaster(SessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public async Task BroadcastAsync(byte[] data, ClientSession sender)
    {
        foreach (var session in _sessionManager.GetAll())
        {
            if (!session.IsAuthenticated || session.ClientId == sender.ClientId)
                continue;

            try
            {
                await session.TcpClient.GetStream().WriteAsync(data);
            }
            catch
            {
                _logger.LogError
                (
                    ex, 
                    "Error while sending message to client {ClientId}",
                    session.ClientId
                );
            }
        }
    }
}