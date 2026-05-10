using ChatAsyncServerSqlLite.Handlers;
using ChatAsyncServerSqlLite.Contracts;

namespace ChatAsyncServerSqlLite.Routing
{
    public class PacketRouter
    {
        private readonly AuthHandler _authHandler;
        private readonly MessageHandler _messageHandler;

        public PacketRouter(AuthHandler authHandler, MessageHandler messageHandler)
        {
            _authHandler = authHandler;
            _messageHandler = messageHandler;
        }

        public async Task RouteAsync(ClientSession session, Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.Register:
                    await _authHandler.HandleRegisterAsync(
                        session,
                        packet
                    );
                    break;

                case PacketType.Login:
                    await _authHandler.HandleLoginAsync(
                        session,
                        packet
                    );
                    break;

                case PacketType.Message:
                    await _messageHandler.HandleAsync(
                        session,
                        packet
                    );
                    break;
            }
        }
    }
}
