using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using SpaceChatServer.Core.Sessions;

namespace SpaceChatServer.Tests.Core.Sessions;

public class SessionManagerTests
{
    private static SessionManager CreateManager()
        => new(NullLogger<SessionManager>.Instance);

    private static ClientSession CreateSession(Guid? clientId = null)
        => new()
        {
            TcpClient = new TcpClient(),
            ClientId = clientId ?? Guid.NewGuid(),
        };

    [Fact]
    public void GetAll_WhenEmpty_ReturnsNull()
    {
        var manager = CreateManager();

        Assert.Null(manager.GetAll());
    }

    [Fact]
    public void Add_ThenGetAll_ContainsSession()
    {
        var manager = CreateManager();
        var session = CreateSession();

        manager.Add(session);

        var all = manager.GetAll();
        Assert.NotNull(all);
        Assert.Contains(session, all!);
    }

    [Fact]
    public void Add_SameSessionTwice_IsIdempotent()
    {
        var manager = CreateManager();
        var session = CreateSession();

        manager.Add(session);
        manager.Add(session);

        Assert.Single(manager.GetAll()!);
    }

    [Fact]
    public void IsOnline_WhenClientHasSession_ReturnsTrue()
    {
        var manager = CreateManager();
        var clientId = Guid.NewGuid();
        manager.Add(CreateSession(clientId));

        Assert.True(manager.IsOnline(clientId));
    }

    [Fact]
    public void IsOnline_WhenClientHasNoSession_ReturnsFalse()
    {
        var manager = CreateManager();
        manager.Add(CreateSession());

        Assert.False(manager.IsOnline(Guid.NewGuid()));
    }

    [Fact]
    public void Remove_DeletesSession()
    {
        var manager = CreateManager();
        var session = CreateSession();
        manager.Add(session);

        manager.Remove(session.SessionId);

        Assert.Null(manager.GetAll());
    }

    [Fact]
    public void Remove_UnknownSessionId_DoesNotThrow()
    {
        var manager = CreateManager();
        manager.Add(CreateSession());

        manager.Remove(Guid.NewGuid());

        Assert.Single(manager.GetAll()!);
    }
}
