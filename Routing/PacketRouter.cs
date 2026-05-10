using ChatAsyncServerSqlLite.Handlers;
using ChatAsyncServerSqlLite.Contracts;
using Microsoft.Extensions.Logging;

namespace ChatAsyncServerSqlLite.Routing;

public class PacketRouter
{
    private readonly AuthHandler _authHandler;
    private readonly MessageHandler _messageHandler;
    private readonly ILogger<PacketRouter> _logger;

    public PacketRouter(AuthHandler authHandler,
                        MessageHandler messageHandler,
                        ILogger<PacketRouter> logger)
    {
        this._authHandler = authHandler;
        this._messageHandler = messageHandler;
        this._logger = logger;
    }

    public async Task RouteAsync(ClientSession session, Packet packet)
    {
        _logger.LogInformation(
                "Packet received: {Type}",
                packet.Type);

        switch (packet.Type)
        {
            case PacketType.Register:
                _logger.LogInformation("Routing to AuthHandler.Register");
                
                await _authHandler.HandleRegisterAsync(
                    session,
                    packet
                );
                break;

            case PacketType.Login:
                _logger.LogInformation("Routing to AuthHandler.Login");
                
                await _authHandler.HandleLoginAsync(
                    session,
                    packet
                );
                break;

            case PacketType.Message:
                _logger.LogInformation("Routing to MessageHandler");
                
                await _messageHandler.HandleAsync(
                    session,
                    packet
                );
                break;

            default:
                _logger.LogWarning(
                    "Unknown packet type: {Type}",
                    packet.Type);
        
                break;
        }
    }
}