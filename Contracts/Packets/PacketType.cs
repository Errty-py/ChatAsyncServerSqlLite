using System.Text.Json.Serialization;

namespace TcpChatServer.Contracts.Packets;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PacketType
{
    Registration,
    Login,

    SendMessage,
    GetAllMessages
}