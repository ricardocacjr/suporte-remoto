using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Infrastructure.Persistence.Configurations;

public class RemoteSessionConfiguration : IEntityTypeConfiguration<RemoteSession>
{
    public void Configure(EntityTypeBuilder<RemoteSession> builder)
    {
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.Status);

        builder.HasMany(s => s.Log)
            .WithOne(l => l.RemoteSession)
            .HasForeignKey(l => l.RemoteSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Ticket)
            .WithMany()
            .HasForeignKey(s => s.TicketId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
