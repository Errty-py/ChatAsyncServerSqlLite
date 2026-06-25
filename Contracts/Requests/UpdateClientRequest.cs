namespace SpaceChatServer.Contracts.Requests;

public class UpdateClientRequest
{
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public byte[]? Avatar { get; set; }
}