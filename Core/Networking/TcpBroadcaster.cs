using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Core.Sessions;
using Microsoft.Extensions.Logging;

namespace SpaceChatServer.Core.Networking;

public class PacketBroadcaster : IPacketBroadcaster
{
    private readonly SessionManager _sessionManager;
    private readonly NetworkHelper _networkHelper;
    private readonly ILogger<PacketBroadcaster> _logger;

    public PacketBroadcaster(SessionManager sessionManager,
                                 NetworkHelper networkHelper,
                                 ILogger<PacketBroadcaster> logger)
    {
        this._sessionManager = sessionManager;
        this._networkHelper = networkHelper;
        this._logger = logger;
    }

    public async Task BroadcastAsync(ClientSession sender, string data)
    {
        var sessions = _sessionManager.GetAll();

        int sentCount = 0;
        int skippedCount = 0;
        int errorCount = 0;

        if (sessions is null)
            return;

        _logger.LogInformation("Broadcast started from client {ClientId}. Target sessions: {Count}",
                               sender.ClientId,
                               sessions.Count);

        foreach (var session in sessions)
        {
            if (!session.IsAuthenticated || session.ClientId == sender.ClientId)
            {
                skippedCount++;
                continue;
            }

            try
            {
                var stream = session.TcpClient.GetStream();
                
                await _networkHelper.WriteAsync(stream, data);

                sentCount++;
            }
            catch (Exception ex)
            {
                errorCount++;

                _logger.LogError(ex,
                                 "Failed to send packet to client {ClientId}",
                                 session.ClientId);
            }
        }

        _logger.LogInformation("Broadcast finished. Sent: {Sent}, Skipped: {Skipped}, Errors: {Errors}",
                               sentCount,
                               skippedCount,
                               errorCount);
    }
}