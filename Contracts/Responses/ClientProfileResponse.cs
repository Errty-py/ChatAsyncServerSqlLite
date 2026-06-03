namespace TcpChatServer.Contracts.Responses;

public class ClientProfileResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;

    public byte[]? Avatar { get; set; }
}