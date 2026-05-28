using TcpChatServer.Handlers;
using TcpChatServer.Contracts.Packets;
using TcpChatServer.Core.Sessions;
using Microsoft.Extensions.Logging;

namespace TcpChatServer.Routing;

public class PacketRouter
{
    private readonly AuthHandler _authHandler;
    private readonly ClientHandler _clientHandler;
    private readonly MessageHandler _messageHandler;
    private readonly ILogger<PacketRouter> _logger;

    public PacketRouter(AuthHandler authHandler,
                        ClientHandler clientHandler,
                        MessageHandler messageHandler,
                        ILogger<PacketRouter> logger)
    {
        this._authHandler = authHandler;
        this._clientHandler = clientHandler;
        this._messageHandler = messageHandler;
        this._logger = logger;
    }

    public async Task RouteAsync(ClientSession session, Packet packet)
    {
        _logger.LogInformation("Packet received: {Type}",
                               packet.Type);

        switch (packet.Type)
        {
            case PacketType.Registration:
                _logger.LogInformation("Routing to AuthHandler.Registration");
                
                await _authHandler.RegistrationAsync(session,
                                                     packet);
                
                break;

            case PacketType.Login:
                _logger.LogInformation("Routing to AuthHandler.Login");
                
                await _authHandler.LoginAsync(session,
                                              packet);
                
                break;

            

            case PacketType.GetClientById:
                _logger.LogInformation("Routing to ClientHandler.GetById");

                await _clientHandler.GetByIdAsync(session, packet);

                break;

            case PacketType.DeleteClientById:
                _logger.LogInformation("Routing to ClientHandler.DeleteById");

                await _clientHandler.DeleteAsync(session, packet);
                
                break;

            case PacketType.SendMessage:
                _logger.LogInformation("Routing to MessageHandler.Send");
                
                await _messageHandler.SendAsync(session,
                                                packet);
                
                break;

            case PacketType.GetAllMessages:
                _logger.LogInformation("Routing to MessageHandler.GetAll");
                
                await _messageHandler.GetAllAsync(session);

                break;

            default:
                _logger.LogWarning("Unknown packet type: {Type}",
                                   packet.Type);
        
                break;
        }
    }
}