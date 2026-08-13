using SuporteRemoto.Domain.Entities;

namespace SuporteRemoto.Application.Interfaces;

public interface IRemoteSessionRepository : IRepository<RemoteSession>
{
    Task<IReadOnlyList<RemoteSession>> ListAtivasAsync(CancellationToken ct = default);
}
