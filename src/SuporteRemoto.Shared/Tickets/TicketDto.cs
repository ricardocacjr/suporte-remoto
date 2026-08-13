using SuporteRemoto.Domain.Enums;

namespace SuporteRemoto.Shared.Tickets;

public record TicketDto(
    Guid Id,
    string Titulo,
    string Descricao,
    TicketStatus Status,
    TicketPriority Prioridade,
    Guid SolicitanteId,
    string SolicitanteNome,
    Guid? TecnicoResponsavelId,
    string? TecnicoResponsavelNome,
    DateTimeOffset CreatedAt);
