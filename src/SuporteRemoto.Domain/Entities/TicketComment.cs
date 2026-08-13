using SuporteRemoto.Domain.Common;

namespace SuporteRemoto.Domain.Entities;

public class TicketComment : BaseEntity
{
    public required Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public required Guid AutorId { get; set; }
    public required string Texto { get; set; }
}
