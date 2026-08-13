using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Application.Interfaces;

public interface IChatThreadRepository : IRepository<ChatThread>
{
    Task<ChatThread?> GetByTicketIdAsync(Guid ticketId, CancellationToken ct = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken ct = default);
}
