using SpaceChatServer.Core.Security;

namespace SpaceChatServer.Tests.Core.Security;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ProducesSaltAndHashSeparatedByDot()
    {
        var hashed = PasswordHasher.Hash("password");

        var parts = hashed.Split('.');
        Assert.Equal(2, parts.Length);
        Assert.NotEmpty(parts[0]);
        Assert.NotEmpty(parts[1]);
    }

    [Fact]
    public void Hash_SamePasswordProducesDifferentHashes_DueToRandomSalt()
    {
        var first = PasswordHasher.Hash("password");
        var second = PasswordHasher.Hash("password");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hashed = PasswordHasher.Hash("s3cret");

        Assert.True(PasswordHasher.Verify("s3cret", hashed));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hashed = PasswordHasher.Hash("s3cret");

        Assert.False(PasswordHasher.Verify("wrong", hashed));
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        var hashed = PasswordHasher.Hash("Secret");

        Assert.False(PasswordHasher.Verify("secret", hashed));
    }
}
