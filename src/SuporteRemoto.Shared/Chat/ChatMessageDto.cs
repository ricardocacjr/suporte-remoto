namespace SuporteRemoto.Shared.Chat;

public record ChatMessageDto(
    Guid Id,
    Guid RemetenteId,
    string RemetenteNome,
    string Texto,
    DateTimeOffset CreatedAt);
