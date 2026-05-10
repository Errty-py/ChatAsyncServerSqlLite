using ChatAsyncServerSqlLite.Data.Entities;

namespace ChatAsyncServerSqlLite.Abstractions.Interfaces
{
    public interface IMessageRepository
    {
        Task AddAsync(MessageEntity message);
        Task<List<MessageEntity>> GetAllAsync();
        public Task RemoveAsync(int id);
    }
}
