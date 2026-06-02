using TcpChatServer.Core.Sessions;

namespace TcpChatServer.Abstractions.Interfaces;

public interface ITcpBroadcaster
{
    Task BroadcastAsync(ClientSession sender, string data);
}