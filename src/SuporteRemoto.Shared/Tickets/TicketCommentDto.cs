namespace SuporteRemoto.Shared.Tickets;

public record TicketCommentDto(
    Guid Id,
    Guid AutorId,
    string AutorNome,
    string Texto,
    DateTimeOffset CreatedAt);
