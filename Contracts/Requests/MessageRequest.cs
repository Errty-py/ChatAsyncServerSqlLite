namespace ChatAsyncServerSqlLite.Contracts.Requests
{
    public class MessageRequest
    {
        public int FromClientId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
