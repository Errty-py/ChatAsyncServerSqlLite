using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatAsyncServerSqlLite.Data.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _dbContext;

    public MessageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MessageEntity message)
    {
        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<MessageEntity>> GetAllAsync()
    {
        return await _dbContext.Messages.AsNoTracking().ToListAsync();
    }

    public async Task RemoveAsync(int id) 
    {
        await _dbContext.Messages.Where(m => m.Id == id).ExecuteDeleteAsync();
    }
}