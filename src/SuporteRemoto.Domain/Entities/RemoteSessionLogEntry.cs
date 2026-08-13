using SuporteRemoto.Domain.Common;

namespace SuporteRemoto.Domain.Entities;

public class RemoteSessionLogEntry : BaseEntity
{
    public required Guid RemoteSessionId { get; set; }
    public RemoteSession? RemoteSession { get; set; }

    public required string Evento { get; set; }
}
