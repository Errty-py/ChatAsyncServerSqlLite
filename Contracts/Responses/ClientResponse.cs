namespace SpaceChatServer.Contracts.Responses;

public class ClientResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[]? Avatar { get; set; }
}