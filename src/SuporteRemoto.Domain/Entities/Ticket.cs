using SuporteRemoto.Domain.Common;
using SuporteRemoto.Domain.Enums;

namespace SuporteRemoto.Domain.Entities;

public class Ticket : BaseEntity, ITenantScoped
{
    public Guid? TenantId { get; set; }

    public required string Titulo { get; set; }
    public required string Descricao { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Aberto;
    public TicketPriority Prioridade { get; set; } = TicketPriority.Normal;

    public required Guid SolicitanteId { get; set; }
    public Guid? TecnicoResponsavelId { get; set; }

    public DateTimeOffset? ResolvidoEm { get; set; }
    public DateTimeOffset? FechadoEm { get; set; }

    public ICollection<TicketComment> Comentarios { get; set; } = new List<TicketComment>();
    public ICollection<TicketAttachment> Anexos { get; set; } = new List<TicketAttachment>();
    public ChatThread? ChatThread { get; set; }
}
