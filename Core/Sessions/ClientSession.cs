using System.Net.Sockets;

namespace TcpChatServer.Core.Sessions;

public class ClientSession
{
    public Guid SessionId { get; init; } = Guid.NewGuid();

    public int? ClientId { get; set; }

    public required TcpClient TcpClient { get; init; }

    public string Name { get; set; } = string.Empty;

    public bool IsAuthenticated { get; set; }
}