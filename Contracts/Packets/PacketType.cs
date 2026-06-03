using System.Text.Json.Serialization;

namespace TcpChatServer.Contracts.Packets;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PacketType
{
    Registration,
    Login,

    GetAllClients,
    GetClientById,
    UpdateClient,
    DeleteClient,

    SendMessage,
    GetAllMessages,
    DeleteMessage,

    ClientLogged,
    ClientRegistered,
    ClientReceived,
    ClientUpdated,
    ClientDeleted,

    MessageReceived,
    MessageHistoryReceived,
    MessageAdded,
    MessageDeleted
}