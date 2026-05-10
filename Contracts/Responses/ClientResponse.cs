namespace ChatAsyncServerSqlLite.Contracts.Responses
{
    public class ClientResponse : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
