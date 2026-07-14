using SpaceChatServer.Core.Models;
using SpaceChatServer.Core.Security;
using SpaceChatServer.Services;
using SpaceChatServer.Tests.Fakes;

namespace SpaceChatServer.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithNewLogin_CreatesClient()
    {
        var repo = new FakeClientRepository();
        var service = new AuthService(repo);

        var (client, error) = await service.RegisterAsync("Alice", "alice", "password");

        Assert.Null(error);
        Assert.NotNull(client);
        Assert.Equal("alice", client!.Login);
        Assert.Equal(1, repo.CreateCalls);
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        var repo = new FakeClientRepository();
        var service = new AuthService(repo);

        var (client, _) = await service.RegisterAsync("Alice", "alice", "password");

        Assert.NotEqual("password", client!.PasswordHash);
        Assert.True(PasswordHasher.Verify("password", client.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidData_ReturnsErrorAndDoesNotPersist()
    {
        var repo = new FakeClientRepository();
        var service = new AuthService(repo);

        var (client, error) = await service.RegisterAsync("", "alice", "password");

        Assert.Null(client);
        Assert.NotNull(error);
        Assert.Equal(0, repo.CreateCalls);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingLogin_ReturnsError()
    {
        var repo = new FakeClientRepository();
        repo.Seed(Client.Create(Guid.NewGuid(), "Existing", "alice", "hash", null).Value);
        var service = new AuthService(repo);

        var (client, error) = await service.RegisterAsync("Alice", "alice", "password");

        Assert.Null(client);
        Assert.Equal("Client with this login already exists", error);
        Assert.Equal(0, repo.CreateCalls);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsClient()
    {
        var repo = new FakeClientRepository();
        var hash = PasswordHasher.Hash("password");
        var existing = Client.Create(Guid.NewGuid(), "Alice", "alice", hash, null).Value;
        repo.Seed(existing);
        var service = new AuthService(repo);

        var (client, error) = await service.LoginAsync("alice", "password");

        Assert.Null(error);
        Assert.NotNull(client);
        Assert.Equal(existing.Id, client!.Id);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownLogin_ReturnsError()
    {
        var repo = new FakeClientRepository();
        var service = new AuthService(repo);

        var (client, error) = await service.LoginAsync("nobody", "password");

        Assert.Null(client);
        Assert.Equal("Client not found", error);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsError()
    {
        var repo = new FakeClientRepository();
        var hash = PasswordHasher.Hash("password");
        repo.Seed(Client.Create(Guid.NewGuid(), "Alice", "alice", hash, null).Value);
        var service = new AuthService(repo);

        var (client, error) = await service.LoginAsync("alice", "wrong");

        Assert.Null(client);
        Assert.Equal("Invalid password", error);
    }
}
