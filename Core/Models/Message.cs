using CSharpFunctionalExtensions;

namespace SpaceChatServer.Core.Models;

public class Message
{
    public const int MAX_TEXT_LENGTH = 250;

    public Guid Id { get; }
    public Guid FromClientId { get; }
    public string Text { get; } = string.Empty;
    public DateTime CreatedAt { get; }

    private Message(Guid id, Guid fromClientId, string text, DateTime createdAt)
    {
        this.Id = id;
        this.FromClientId = fromClientId;
        this.Text = text;
        this.CreatedAt = createdAt;
    }

    public static Result<Message> Create(Guid id, Guid fromClientId, string text, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure<Message>("Message text cannot be empty.");

        if (text.Length > MAX_TEXT_LENGTH)
            return Result.Failure<Message>($"Message text cannot exceed {MAX_TEXT_LENGTH} characters.");

        if (fromClientId == Guid.Empty)
                return Result.Failure<Message>("FromClientId cannot be empty.");

        return Result.Success(new Message(id, fromClientId, text, createdAt));
    }
}