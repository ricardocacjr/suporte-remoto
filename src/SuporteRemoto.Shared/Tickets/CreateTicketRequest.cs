using SuporteRemoto.Domain.Enums;

namespace SuporteRemoto.Shared.Tickets;

public record CreateTicketRequest(
    string Titulo,
    string Descricao,
    TicketPriority Prioridade,
    Guid SolicitanteId);
