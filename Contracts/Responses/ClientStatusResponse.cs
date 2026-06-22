namespace SpaceChatServer.Contracts.Responses;

public class ClientStatusResponse
{
    public Guid ClientId { get; set; }

    public bool IsOnline { get; set; }
}