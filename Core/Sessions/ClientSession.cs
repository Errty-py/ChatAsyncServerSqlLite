using System.Net.Sockets;

namespace SpaceChatServer.Core.Sessions;

public class ClientSession
{
    public Guid SessionId { get; init; } = Guid.NewGuid();

    public Guid ClientId { get; set; }

    public required TcpClient TcpClient { get; init; }

    public string ClientName { get; set; } = string.Empty;

    public bool IsAuthenticated { get; set; }
}