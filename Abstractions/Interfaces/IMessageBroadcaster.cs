using TcpChatServer.Core.Sessions;

namespace TcpChatServer.Abstractions.Interfaces;

public interface IMessageBroadcaster
{
    Task BroadcastAsync(ClientSession sender, string data);
}