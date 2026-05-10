using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Core.Sessions;
using Microsoft.Extensions.Logging;

namespace ChatAsyncServerSqlLite.Core.Networking;

public class TcpMessageBroadcaster : IMessageBroadcaster
{
    private readonly SessionManager _sessionManager;
    private readonly NetworkHelper _networkHelper;
    private readonly ILogger<TcpMessageBroadcaster> _logger;

    public TcpMessageBroadcaster(SessionManager sessionManager,
                                 NetworkHelper networkHelper,
                                 ILogger<TcpMessageBroadcaster> logger)
    {
        this._sessionManager = sessionManager;
        this._networkHelper = networkHelper;
        this._logger = logger;
    }

    public async Task BroadcastAsync(byte[] data, ClientSession sender)
    {
        var sessions = _sessionManager.GetAll();

        int sentCount = 0;
        int skippedCount = 0;
        int errorCount = 0;

        _logger.LogInformation(
            "Broadcast started from client {ClientId}. Target sessions: {Count}",
            sender.ClientId,
            sessions.Count);

        foreach (var session in sessions)
        {
            if (!session.IsAuthenticated || 
                session.ClientId == sender.ClientId)
            {
                skippedCount++;
                continue;
            }

            try
            {
                var stream = session.TcpClient.GetStream();
                
                await _networkHelper.SendAsync(data, stream);

                sentCount++;
            }
            catch (Exception ex)
            {
                errorCount++;

                _logger.LogError(
                    ex,
                    "Failed to send message to client {ClientId}",
                    session.ClientId);
            }
        }

        _logger.LogInformation(
            "Broadcast finished. Sent: {Sent}, Skipped: {Skipped}, Errors: {Errors}",
            sentCount,
            skippedCount,
            errorCount);
    }
}