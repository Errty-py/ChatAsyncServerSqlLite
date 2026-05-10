using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAsyncServerSqlLite.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatAsyncServerSqlLite.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<ClientEntity>
{
    public void Configure(EntityTypeBuilder<ClientEntity> entity)
    {
        entity.HasKey(c => c.Id);

        entity.Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

        entity.Property(c => c.Login)
                .HasMaxLength(50)
                .IsRequired();

        entity.HasIndex(c => c.Login)
                .IsUnique();

        entity.Property(c => c.PasswordHash)
                .IsRequired();
    }
}