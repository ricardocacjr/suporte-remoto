using SuporteRemoto.Domain.Common;

namespace SuporteRemoto.Domain.Entities;

public class TicketAttachment : BaseEntity
{
    public required Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public required Guid EnviadoPorId { get; set; }
    public required string NomeArquivo { get; set; }
    public required string CaminhoArmazenamento { get; set; }
    public required long TamanhoBytes { get; set; }
    public required string TipoConteudo { get; set; }
}
