using CSharpFunctionalExtensions;

namespace SpaceChatServer.Core.Models;

public class Client
{
    public const int MAX_NAME_LENGTH = 100;
    public const int MAX_LOGIN_LENGTH = 50;

    public Guid Id { get; }
    public string Name { get; } = string.Empty;
    public string Login { get; } = string.Empty;
    public string PasswordHash { get; } = string.Empty;
    public byte[]? Avatar { get; } = null;

    private Client(Guid id, string name, string login, string passwordHash, byte[]? avatar)
    {
        this.Id = id;
        this.Name = name;
        this.Login = login;
        this.PasswordHash = passwordHash;
        this.Avatar = avatar;
    }

    public Result<Client> Create(Guid id, string name, string login, string passwordHash, byte[]? avatar)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            return Result.Failure<Client>("Name cannot be empty.");

        if (string.IsNullOrWhiteSpace(login) || login.Length > MAX_LOGIN_LENGTH)
            return Result.Failure<Client>("Login cannot be empty.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<Client>("Password hash cannot be empty.");

        return Result.Success(new Client(id, name, login, passwordHash, avatar));
    }
}