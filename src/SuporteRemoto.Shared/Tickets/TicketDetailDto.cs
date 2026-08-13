using SuporteRemoto.Domain.Enums;

namespace SuporteRemoto.Shared.Tickets;

public record TicketDetailDto(
    Guid Id,
    string Titulo,
    string Descricao,
    TicketStatus Status,
    TicketPriority Prioridade,
    Guid SolicitanteId,
    string SolicitanteNome,
    Guid? TecnicoResponsavelId,
    string? TecnicoResponsavelNome,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvidoEm,
    DateTimeOffset? FechadoEm,
    IReadOnlyList<TicketCommentDto> Comentarios,
    IReadOnlyList<TicketAttachmentDto> Anexos);
