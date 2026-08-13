using Microsoft.EntityFrameworkCore;
using SuporteRemoto.Application.Interfaces;
using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Infrastructure.Persistence.Repositories;

public class TicketRepository(AppDbContext context) : RepositoryBase<Ticket>(context), ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> ListByTecnicoAsync(Guid tecnicoId, CancellationToken ct = default) =>
        await Set.AsNoTracking().Where(t => t.TecnicoResponsavelId == tecnicoId).ToListAsync(ct);

    public async Task<IReadOnlyList<Ticket>> ListBySolicitanteAsync(Guid solicitanteId, CancellationToken ct = default) =>
        await Set.AsNoTracking().Where(t => t.SolicitanteId == solicitanteId).ToListAsync(ct);

    public async Task<Ticket?> GetWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await Set
            .Include(t => t.Comentarios)
            .Include(t => t.Anexos)
            .Include(t => t.ChatThread!).ThenInclude(c => c.Mensagens)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddCommentAsync(TicketComment comment, CancellationToken ct = default) =>
        await Context.Set<TicketComment>().AddAsync(comment, ct);

    public async Task AddAttachmentAsync(TicketAttachment attachment, CancellationToken ct = default) =>
        await Context.Set<TicketAttachment>().AddAsync(attachment, ct);
}
