using SuporteRemoto.Domain.Common;

namespace SuporteRemoto.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public required Guid ChatThreadId { get; set; }
    public ChatThread? ChatThread { get; set; }

    public required Guid RemetenteId { get; set; }
    public required string Texto { get; set; }
    public bool Lida { get; set; }
}
