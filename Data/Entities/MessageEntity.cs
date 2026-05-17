namespace ChatAsyncServerSqlLite.Data.Entities;

public class MessageEntity
{
    public int Id { get; set; }
    public int FromClientId { get; set; }
    public ClientEntity FromClient { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
