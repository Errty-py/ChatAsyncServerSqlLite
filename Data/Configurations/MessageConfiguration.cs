using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAsyncServerSqlLite.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace ChatAsyncServerSqlLite.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<MessageEntity>
    {
        public void Configure(EntityTypeBuilder<MessageEntity> entity)
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Text)
                  .IsRequired();

            entity.Property(m => m.CreatedAt)
                  .IsRequired();

            entity.Property(m => m.FromClientId)
                  .IsRequired();

            entity.HasOne<ClientEntity>()
                  .WithMany()
                  .HasForeignKey(m => m.FromClientId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}