using TcpChatServer.Core.Sessions;

namespace TcpChatServer.Abstractions.Interfaces;

public interface IMessageBroadcaster
{
    Task BroadcastAsync(string data, ClientSession sender);
}