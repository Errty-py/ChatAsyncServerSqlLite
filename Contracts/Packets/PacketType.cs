using System.Text.Json.Serialization;

namespace SpaceChatServer.Contracts.Packets;

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
    ClientStatusChanged,
    ClientUpdated,
    ClientDeleted,

    ClientList,
    MessageReceived,
    MessageHistoryReceived,
    MessageAdded,
    MessageDeleted
}