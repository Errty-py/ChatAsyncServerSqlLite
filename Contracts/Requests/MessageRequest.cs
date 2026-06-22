namespace SpaceChatServer.Contracts.Requests;

public class MessageRequest
{
    public Guid FromClientId { get; set; }
    public string Text { get; set; } = string.Empty;
}