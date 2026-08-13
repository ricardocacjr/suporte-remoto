using Microsoft.EntityFrameworkCore;
using SuporteRemoto.Application.Interfaces;
using SuporteRemoto.Domain.Entities;
using SuporteRemoto.Domain.Enums;

namespace SuporteRemoto.Infrastructure.Persistence.Repositories;

public class RemoteSessionRepository(AppDbContext context) : RepositoryBase<RemoteSession>(context), IRemoteSessionRepository
{
    public async Task<IReadOnlyList<RemoteSession>> ListAtivasAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(s => s.Status == RemoteSessionStatus.AguardandoConexao || s.Status == RemoteSessionStatus.Conectada)
            .ToListAsync(ct);
}
