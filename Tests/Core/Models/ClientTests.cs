using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Tests.Core.Models;

public class ClientTests
{
    private static Client CreateValid()
        => Client.Create(Guid.NewGuid(), "Alice", "alice", "hash", null).Value;

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var id = Guid.NewGuid();
        var avatar = new byte[] { 1, 2, 3 };

        var result = Client.Create(id, "Alice", "alice", "hash", avatar);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal("Alice", result.Value.Name);
        Assert.Equal("alice", result.Value.Login);
        Assert.Equal("hash", result.Value.PasswordHash);
        Assert.Equal(avatar, result.Value.Avatar);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_Fails(string name)
    {
        var result = Client.Create(Guid.NewGuid(), name, "alice", "hash", null);

        Assert.True(result.IsFailure);
        Assert.Equal("Name cannot be empty or too long.", result.Error);
    }

    [Fact]
    public void Create_WithTooLongName_Fails()
    {
        var name = new string('a', Client.MAX_NAME_LENGTH + 1);

        var result = Client.Create(Guid.NewGuid(), name, "alice", "hash", null);

        Assert.True(result.IsFailure);
        Assert.Equal("Name cannot be empty or too long.", result.Error);
    }

    [Fact]
    public void Create_WithNameAtMaxLength_Succeeds()
    {
        var name = new string('a', Client.MAX_NAME_LENGTH);

        var result = Client.Create(Guid.NewGuid(), name, "alice", "hash", null);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyLogin_Fails(string login)
    {
        var result = Client.Create(Guid.NewGuid(), "Alice", login, "hash", null);

        Assert.True(result.IsFailure);
        Assert.Equal("Login cannot be empty or too long.", result.Error);
    }

    [Fact]
    public void Create_WithTooLongLogin_Fails()
    {
        var login = new string('a', Client.MAX_LOGIN_LENGTH + 1);

        var result = Client.Create(Guid.NewGuid(), "Alice", login, "hash", null);

        Assert.True(result.IsFailure);
        Assert.Equal("Login cannot be empty or too long.", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPassword_Fails(string password)
    {
        var result = Client.Create(Guid.NewGuid(), "Alice", "alice", password, null);

        Assert.True(result.IsFailure);
        Assert.Equal("Password cannot be empty.", result.Error);
    }

    [Fact]
    public void Create_WithTooLargeAvatar_Fails()
    {
        var avatar = new byte[Client.MAX_AVATAR_SIZE + 1];

        var result = Client.Create(Guid.NewGuid(), "Alice", "alice", "hash", avatar);

        Assert.True(result.IsFailure);
        Assert.Equal("Avatar too large (max 8MB).", result.Error);
    }

    [Fact]
    public void Update_WithValidData_MutatesState()
    {
        var client = CreateValid();
        var avatar = new byte[] { 9 };

        var result = client.Update("Bob", "bob", avatar);

        Assert.True(result.IsSuccess);
        Assert.Equal("Bob", client.Name);
        Assert.Equal("bob", client.Login);
        Assert.Equal(avatar, client.Avatar);
    }

    [Fact]
    public void Update_WithInvalidData_DoesNotMutateState()
    {
        var client = CreateValid();

        var result = client.Update("", "bob", null);

        Assert.True(result.IsFailure);
        Assert.Equal("Alice", client.Name);
        Assert.Equal("alice", client.Login);
    }

    [Fact]
    public void ChangePassword_WithValidHash_Succeeds()
    {
        var client = CreateValid();

        var result = client.ChangePassword("newhash");

        Assert.True(result.IsSuccess);
        Assert.Equal("newhash", client.PasswordHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePassword_WithEmptyHash_Fails(string hash)
    {
        var client = CreateValid();

        var result = client.ChangePassword(hash);

        Assert.True(result.IsFailure);
        Assert.Equal("Password cannot be empty.", result.Error);
        Assert.Equal("hash", client.PasswordHash);
    }
}
