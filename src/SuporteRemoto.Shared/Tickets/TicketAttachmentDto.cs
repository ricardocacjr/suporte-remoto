namespace SuporteRemoto.Shared.Tickets;

public record TicketAttachmentDto(
    Guid Id,
    string NomeArquivo,
    long TamanhoBytes,
    string TipoConteudo,
    Guid EnviadoPorId,
    string EnviadoPorNome,
    DateTimeOffset CreatedAt);
