namespace TcpChatServer.Contracts.Responses;

public class MessageResponse
{
    public int FromClientId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}