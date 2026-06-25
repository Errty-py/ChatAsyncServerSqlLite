using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> entity)
    {
        entity.HasKey(m => m.Id);

        entity.Property(m => m.Text)
              .HasMaxLength(Message.MAX_TEXT_LENGTH)
              .IsRequired();

        entity.Property(m => m.CreatedAt)
              .IsRequired();

        entity.Property(m => m.FromClientId)
              .IsRequired();

        entity.HasOne(m => m.FromClient)
              .WithMany()
              .HasForeignKey(m => m.FromClientId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}