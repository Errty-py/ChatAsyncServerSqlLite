namespace ChatAsyncServerSqlLite.Contracts.Requests
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
