using SuporteRemoto.Domain.Common;
using SuporteRemoto.Domain.Enums;

namespace SuporteRemoto.Domain.Entities;

public class RemoteSession : BaseEntity, ITenantScoped
{
    public Guid? TenantId { get; set; }

    public Guid? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public required Guid TecnicoId { get; set; }
    public required Guid AgenteHostId { get; set; }

    public RemoteSessionStatus Status { get; set; } = RemoteSessionStatus.AguardandoConexao;

    public DateTimeOffset? ConectadaEm { get; set; }
    public DateTimeOffset? EncerradaEm { get; set; }

    public ICollection<RemoteSessionLogEntry> Log { get; set; } = new List<RemoteSessionLogEntry>();
}
