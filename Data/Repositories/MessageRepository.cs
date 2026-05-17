using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace TcpChatServer.Data.Repositories;

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
        return await _dbContext.Messages
                               .Include(x => x.FromClient.Name)
                               .AsNoTracking()
                               .ToListAsync();
    }

    public async Task RemoveAsync(int id) 
    {
        await _dbContext.Messages.Where(m => m.Id == id).ExecuteDeleteAsync();
    }
}