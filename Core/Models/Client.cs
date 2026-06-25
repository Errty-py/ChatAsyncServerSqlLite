using CSharpFunctionalExtensions;

namespace SpaceChatServer.Core.Models;

public class Client
{
    public const int MAX_NAME_LENGTH = 50;
    public const int MAX_LOGIN_LENGTH = 25;

    public Guid Id { get; }
    public string Name { get; } = string.Empty;
    public string Login { get; } = string.Empty;
    public string Password { get; } = string.Empty;
    public byte[]? Avatar { get; } = null;

    private Client(Guid id, string name, string login, string password, byte[]? avatar)
    {
        this.Id = id;
        this.Name = name;
        this.Login = login;
        this.Password = password;
        this.Avatar = avatar;
    }

    public static Result<Client> Create(Guid id, string name, string login, string password, byte[]? avatar)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            return Result.Failure<Client>("Name cannot be empty.");

        if (string.IsNullOrWhiteSpace(login) || login.Length > MAX_LOGIN_LENGTH)
            return Result.Failure<Client>("Login cannot be empty.");

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<Client>("Password cannot be empty.");

        if (avatar is not null && avatar.Length > 1024 * 1024 * 8)
            return Result.Failure<Client>("Avatar too large (max 8MB)");

        return Result.Success(new Client(id, name, login, password, avatar));
    }
}