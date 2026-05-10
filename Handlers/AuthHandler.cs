using ChatAsyncServerSqlLite.Protocols;
using ChatAsyncServerSqlLite.Contracts;
using ChatAsyncServerSqlLite.Contracts.Requests;
using ChatAsyncServerSqlLite.Services;
using System.Text.Json;

namespace ChatAsyncServerSqlLite.Handlers
{
    public class AuthHandler
    {
        private readonly AuthService _authService;

        public AuthHandler(AuthService authService)
        {
            _authService = authService;
        }

        public async Task HandleRegisterAsync(ClientSession session,
                                              Packet packet)
        {
            RegisterRequest? request = packet.Data
                                             .Deserialize<RegisterRequest>();

            if (request == null)
                return;

            var response =
                await _authService.RegisterAsync(request);

            byte[] data = PacketSerializer.Serialize(response);

            await session.TcpClient
                .GetStream()
                .WriteAsync(data);
        }

        public async Task HandleLoginAsync(
            ClientSession session,
            Packet packet)
        {
            LoginRequest? request = packet.Data
                                          .Deserialize<LoginRequest>();

            if (request == null)
                return;

            var response =
                await _authService.LoginAsync(request);

            if (response.Success)
            {
                session.ClientId = response.Id;
                session.Name = response.Name;
                session.IsAuthenticated = true;
            }

            byte[] data = PacketSerializer.Serialize(response);

            await session.TcpClient
                .GetStream()
                .WriteAsync(data);
        }
    }
}
