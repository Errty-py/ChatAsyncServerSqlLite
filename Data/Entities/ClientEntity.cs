namespace ChatAsyncServerSqlLite.Data.Entities;

public class ClientEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}