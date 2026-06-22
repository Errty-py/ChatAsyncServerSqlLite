namespace SpaceChatServer.Data.Entities;

public class MessageEntity
{
    public Guid Id { get; set; }
    public Guid FromClientId { get; set; }
    public ClientEntity FromClient { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
