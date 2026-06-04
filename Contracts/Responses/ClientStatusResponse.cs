namespace SpaceChatServer.Contracts.Responses;

public class ClientStatusResponse
{
    public int ClientId { get; set; }

    public bool IsOnline { get; set; }
}