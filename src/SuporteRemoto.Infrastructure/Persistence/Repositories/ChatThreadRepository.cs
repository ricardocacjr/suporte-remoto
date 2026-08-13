using Microsoft.EntityFrameworkCore;
using SuporteRemoto.Application.Interfaces;
using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Infrastructure.Persistence.Repositories;

public class ChatThreadRepository(AppDbContext context) : RepositoryBase<ChatThread>(context), IChatThreadRepository
{
    public async Task<ChatThread?> GetByTicketIdAsync(Guid ticketId, CancellationToken ct = default) =>
        await Set.Include(c => c.Mensagens).FirstOrDefaultAsync(c => c.TicketId == ticketId, ct);

    public async Task AddMessageAsync(ChatMessage message, CancellationToken ct = default) =>
        await Context.Set<ChatMessage>().AddAsync(message, ct);
}
