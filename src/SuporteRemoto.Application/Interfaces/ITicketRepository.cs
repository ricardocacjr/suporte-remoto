using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Application.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IReadOnlyList<Ticket>> ListByTecnicoAsync(Guid tecnicoId, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> ListBySolicitanteAsync(Guid solicitanteId, CancellationToken ct = default);
    Task<Ticket?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task AddCommentAsync(TicketComment comment, CancellationToken ct = default);
    Task AddAttachmentAsync(TicketAttachment attachment, CancellationToken ct = default);
}
