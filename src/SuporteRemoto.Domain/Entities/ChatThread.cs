using SuporteRemoto.Domain.Common;

namespace SuporteRemoto.Domain.Entities;

public class ChatThread : BaseEntity, ITenantScoped
{
    public Guid? TenantId { get; set; }

    public Guid? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public Guid? RemoteSessionId { get; set; }
    public RemoteSession? RemoteSession { get; set; }

    public bool Encerrada { get; set; }

    public ICollection<ChatMessage> Mensagens { get; set; } = new List<ChatMessage>();
}
