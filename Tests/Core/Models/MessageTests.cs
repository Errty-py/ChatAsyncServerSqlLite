using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Tests.Core.Models;

public class MessageTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var id = Guid.NewGuid();
        var fromClientId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var result = Message.Create(id, fromClientId, "hello", createdAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(fromClientId, result.Value.FromClientId);
        Assert.Equal("hello", result.Value.Text);
        Assert.Equal(createdAt, result.Value.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyText_Fails(string text)
    {
        var result = Message.Create(Guid.NewGuid(), Guid.NewGuid(), text, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("Message text cannot be empty.", result.Error);
    }

    [Fact]
    public void Create_WithTooLongText_Fails()
    {
        var text = new string('a', Message.MAX_TEXT_LENGTH + 1);

        var result = Message.Create(Guid.NewGuid(), Guid.NewGuid(), text, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal($"Message text cannot exceed {Message.MAX_TEXT_LENGTH} characters.", result.Error);
    }

    [Fact]
    public void Create_WithTextAtMaxLength_Succeeds()
    {
        var text = new string('a', Message.MAX_TEXT_LENGTH);

        var result = Message.Create(Guid.NewGuid(), Guid.NewGuid(), text, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithEmptyFromClientId_Fails()
    {
        var result = Message.Create(Guid.NewGuid(), Guid.Empty, "hello", DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("FromClientId cannot be empty.", result.Error);
    }
}
