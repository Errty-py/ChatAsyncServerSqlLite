using TcpChatServer.Data.Configurations;
using TcpChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace TcpChatServer.Data;

public class AppDbContext : DbContext
{
    public DbSet<ClientEntity> Clients { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}