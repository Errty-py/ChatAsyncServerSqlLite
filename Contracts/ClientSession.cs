using System.Net.Sockets;

namespace ChatAsyncServerSqlLite.Contracts;

public class ClientSession
{
    public int ClientId { get; set; }
    public required TcpClient TcpClient { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
}