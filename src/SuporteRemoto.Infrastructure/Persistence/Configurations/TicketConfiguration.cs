using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.Property(t => t.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Descricao).HasMaxLength(4000).IsRequired();

        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.Status);

        builder.HasMany(t => t.Comentarios)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Anexos)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ChatThread)
            .WithOne(c => c.Ticket)
            .HasForeignKey<ChatThread>(c => c.TicketId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
