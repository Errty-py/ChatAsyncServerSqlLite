using SpaceChatServer.Core.Models;
using SpaceChatServer.Services;
using SpaceChatServer.Tests.Fakes;

namespace SpaceChatServer.Tests.Services;

public class ClientServiceTests
{
    private static Client MakeClient(string name = "Alice", string login = "alice")
        => Client.Create(Guid.NewGuid(), name, login, "hash", null).Value;

    [Fact]
    public async Task GetAllAsync_ReturnsAllClients()
    {
        var repo = new FakeClientRepository();
        repo.Seed(MakeClient("Alice", "alice"));
        repo.Seed(MakeClient("Bob", "bob"));
        var service = new ClientService(repo);

        var clients = await service.GetAllAsync();

        Assert.Equal(2, clients.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsClient()
    {
        var repo = new FakeClientRepository();
        var client = MakeClient();
        repo.Seed(client);
        var service = new ClientService(repo);

        var (result, error) = await service.GetByIdAsync(client.Id);

        Assert.Null(error);
        Assert.Equal(client.Id, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsError()
    {
        var service = new ClientService(new FakeClientRepository());

        var (result, error) = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
        Assert.Equal("Client not found", error);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesClient()
    {
        var repo = new FakeClientRepository();
        var client = MakeClient();
        repo.Seed(client);
        var service = new ClientService(repo);

        var (result, error) = await service.UpdateAsync(client.Id, "Bob", "bob", null);

        Assert.Null(error);
        Assert.Equal("Bob", result!.Name);
        Assert.Equal("bob", result.Login);
        Assert.Equal(1, repo.UpdateCalls);
    }

    [Fact]
    public async Task UpdateAsync_WhenClientMissing_ReturnsError()
    {
        var service = new ClientService(new FakeClientRepository());

        var (result, error) = await service.UpdateAsync(Guid.NewGuid(), "Bob", "bob", null);

        Assert.Null(result);
        Assert.Equal("Client not found", error);
    }

    [Fact]
    public async Task UpdateAsync_WhenLoginOccupied_ReturnsError()
    {
        var repo = new FakeClientRepository();
        var client = MakeClient("Alice", "alice");
        repo.Seed(client);
        repo.Seed(MakeClient("Bob", "bob"));
        var service = new ClientService(repo);

        var (result, error) = await service.UpdateAsync(client.Id, "Alice", "bob", null);

        Assert.Null(result);
        Assert.Equal("Login is already occupied by another client", error);
        Assert.Equal(0, repo.UpdateCalls);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidData_ReturnsErrorAndDoesNotPersist()
    {
        var repo = new FakeClientRepository();
        var client = MakeClient();
        repo.Seed(client);
        var service = new ClientService(repo);

        var (result, error) = await service.UpdateAsync(client.Id, "", "alice", null);

        Assert.Null(result);
        Assert.NotNull(error);
        Assert.Equal(0, repo.UpdateCalls);
    }

    [Fact]
    public async Task ChangePassword_WhenExists_UpdatesHash()
    {
        var repo = new FakeClientRepository();
        var client = MakeClient();
        var originalHash = client.PasswordHash;
        repo.Seed(client);
        var service = new ClientService(repo);

        var (result, error) = await service.ChangePassword(client.Id, "newpassword");

        Assert.Null(error);
        Assert.NotEqual(originalHash, result!.PasswordHash);
        Assert.Equal(1, repo.UpdateCalls);
    }

    [Fact]
    public async Task ChangePassword_WhenMissing_ReturnsError()
    {
        var service = new ClientService(new FakeClientRepository());

        var (result, error) = await service.ChangePassword(Guid.NewGuid(), "newpassword");

        Assert.Null(result);
        Assert.Equal("Client not found", error);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesClient()
    {
        var repo = new FakeClientRepository();
        var client = MakeClient();
        repo.Seed(client);
        var service = new ClientService(repo);

        var (id, error) = await service.DeleteAsync(client.Id);

        Assert.Null(error);
        Assert.Equal(client.Id, id);
        Assert.Equal(1, repo.DeleteCalls);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsError()
    {
        var repo = new FakeClientRepository();
        var service = new ClientService(repo);

        var (id, error) = await service.DeleteAsync(Guid.NewGuid());

        Assert.Null(id);
        Assert.Equal("Client not found", error);
        Assert.Equal(0, repo.DeleteCalls);
    }
}
