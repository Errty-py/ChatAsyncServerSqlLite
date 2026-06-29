using CSharpFunctionalExtensions;

namespace SpaceChatServer.Core.Models;

public class Client
{
    public const int MAX_NAME_LENGTH = 50;
    public const int MAX_LOGIN_LENGTH = 25;
    public const int MAX_AVATAR_SIZE = 1024 * 1024 * 8;

    public Guid Id { get; }
    public string Name { get; private set; } = string.Empty;
    public string Login { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public byte[]? Avatar { get; private set; } = null;

    private Client(Guid id, string name, string login, string passwordHash, byte[]? avatar)
    {
        this.Id = id;
        this.Name = name;
        this.Login = login;
        this.PasswordHash = passwordHash;
        this.Avatar = avatar;
    }

    public static Result<Client> Create(Guid id,
                                        string name,
                                        string login,
                                        string passwordHash,
                                        byte[]? avatar)
    {
        var validation = Validate(name,
                                  login,
                                  passwordHash,
                                  avatar);

        if (validation.IsFailure)
            return Result.Failure<Client>(validation.Error);

        return Result.Success(new Client(id, name, login, passwordHash, avatar));
    }

    public Result Update(string name,
                         string login,
                         byte[]? avatar)
    {
        var validation = Validate(name,
                                  login,
                                  avatar);

        if (validation.IsFailure)
            return validation;

        Name = name;
        Login = login;
        Avatar = avatar;

        return Result.Success();
    }

    public Result ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure("Password cannot be empty.");

        PasswordHash = passwordHash;

        return Result.Success();
    }

    private static Result Validate(string name,
                                   string login,
                                   byte[]? avatar)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            return Result.Failure("Name cannot be empty or too long.");

        if (string.IsNullOrWhiteSpace(login) || login.Length > MAX_LOGIN_LENGTH)
            return Result.Failure("Login cannot be empty or too long.");

        if (avatar is not null && avatar.Length > MAX_AVATAR_SIZE)
            return Result.Failure("Avatar too large (max 8MB).");

        return Result.Success();
    }

    private static Result Validate(string name,
                                   string login,
                                   string passwordHash,
                                   byte[]? avatar)
    {
        var validation = Validate(name, login, avatar);

        if (validation.IsFailure)
            return validation;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure("Password cannot be empty.");

        return Result.Success();
    }
}