namespace SpaceChatServer.Contracts.Responses;

public class ClientResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public byte[]? Avatar { get; set; }
}