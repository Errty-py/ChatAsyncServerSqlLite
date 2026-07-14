using SpaceChatServer.Core.Models;
using SpaceChatServer.Services;
using SpaceChatServer.Tests.Fakes;

namespace SpaceChatServer.Tests.Services;

public class MessageServiceTests
{
    [Fact]
    public async Task AddAsync_WithValidText_PersistsMessage()
    {
        var repo = new FakeMessageRepository();
        var service = new MessageService(repo);
        var fromClientId = Guid.NewGuid();

        var (message, error) = await service.AddAsync(fromClientId, "hello");

        Assert.Null(error);
        Assert.NotNull(message);
        Assert.Equal("hello", message!.Text);
        Assert.Equal(fromClientId, message.FromClientId);
        Assert.Equal(1, repo.AddCalls);
    }

    [Fact]
    public async Task AddAsync_WithEmptyText_ReturnsErrorAndDoesNotPersist()
    {
        var repo = new FakeMessageRepository();
        var service = new MessageService(repo);

        var (message, error) = await service.AddAsync(Guid.NewGuid(), "");

        Assert.Null(message);
        Assert.NotNull(error);
        Assert.Equal(0, repo.AddCalls);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMessages()
    {
        var repo = new FakeMessageRepository();
        repo.Seed(Message.Create(Guid.NewGuid(), Guid.NewGuid(), "a", DateTime.UtcNow).Value);
        repo.Seed(Message.Create(Guid.NewGuid(), Guid.NewGuid(), "b", DateTime.UtcNow).Value);
        var service = new MessageService(repo);

        var messages = await service.GetAllAsync();

        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public async Task DeleteAsync_ByOwner_RemovesMessage()
    {
        var repo = new FakeMessageRepository();
        var fromClientId = Guid.NewGuid();
        var message = Message.Create(Guid.NewGuid(), fromClientId, "hi", DateTime.UtcNow).Value;
        repo.Seed(message);
        var service = new MessageService(repo);

        var (id, error) = await service.DeleteAsync(fromClientId, message.Id);

        Assert.Null(error);
        Assert.Equal(message.Id, id);
        Assert.Equal(1, repo.DeleteCalls);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsError()
    {
        var repo = new FakeMessageRepository();
        var service = new MessageService(repo);

        var (id, error) = await service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(Guid.Empty, id);
        Assert.Equal("Message not found", error);
        Assert.Equal(0, repo.DeleteCalls);
    }

    [Fact]
    public async Task DeleteAsync_ByNonOwner_ReturnsErrorAndDoesNotDelete()
    {
        var repo = new FakeMessageRepository();
        var ownerId = Guid.NewGuid();
        var message = Message.Create(Guid.NewGuid(), ownerId, "hi", DateTime.UtcNow).Value;
        repo.Seed(message);
        var service = new MessageService(repo);

        var (id, error) = await service.DeleteAsync(Guid.NewGuid(), message.Id);

        Assert.Equal(Guid.Empty, id);
        Assert.Equal("You cannot delete other people's messages", error);
        Assert.Equal(0, repo.DeleteCalls);
    }
}
