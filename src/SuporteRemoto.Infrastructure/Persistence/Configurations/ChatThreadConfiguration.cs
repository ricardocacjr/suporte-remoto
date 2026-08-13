using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Infrastructure.Persistence.Configurations;

public class ChatThreadConfiguration : IEntityTypeConfiguration<ChatThread>
{
    public void Configure(EntityTypeBuilder<ChatThread> builder)
    {
        builder.HasIndex(c => c.TenantId);

        builder.HasMany(c => c.Mensagens)
            .WithOne(m => m.ChatThread)
            .HasForeignKey(m => m.ChatThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.RemoteSession)
            .WithMany()
            .HasForeignKey(c => c.RemoteSessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
