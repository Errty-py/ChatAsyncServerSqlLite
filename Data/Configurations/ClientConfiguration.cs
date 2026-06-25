using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<ClientEntity>
{
    public void Configure(EntityTypeBuilder<ClientEntity> entity)
    {
        entity.HasKey(c => c.Id);

        entity.Property(c => c.Name)
              .HasMaxLength(Client.MAX_NAME_LENGTH)
              .IsRequired();

        entity.Property(c => c.Login)
              .HasMaxLength(Client.MAX_LOGIN_LENGTH)
              .IsRequired();

        entity.HasIndex(c => c.Login)
              .IsUnique();

        entity.Property(c => c.PasswordHash)
              .IsRequired();

        entity.Property(c => c.Avatar)
              .IsRequired(false);
    }
}