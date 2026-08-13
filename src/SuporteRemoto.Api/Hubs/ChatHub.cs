using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SuporteRemoto.Api.Auth;
using SuporteRemoto.Application.Interfaces;
using SuporteRemoto.Domain.Entities;
using SuporteRemoto.Infrastructure.Identity;
using SuporteRemoto.Shared.Chat;

namespace SuporteRemoto.Api.Hubs;

/// <summary>
/// Chat embutido em cada ticket: uma conversa (<see cref="ChatThread"/>) por ticket, criada sob
/// demanda na primeira entrada/mensagem. Mensagens são persistidas antes do broadcast.
/// </summary>
[Authorize]
public class ChatHub(
    ITicketRepository ticketRepository,
    IChatThreadRepository chatThreadRepository,
    UserManager<ApplicationUser> userManager,
    ILogger<ChatHub> logger) : Hub
{
    public async Task JoinTicketChat(Guid ticketId)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId, Context.ConnectionAborted);
        if (ticket is null || !CanAccess(ticket))
            throw new HubException("Ticket não encontrado ou acesso negado.");

        await GetOrCreateThreadAsync(ticketId, Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId.ToString());
        logger.LogInformation("Conexão {ConnectionId} entrou no chat do ticket {TicketId}", Context.ConnectionId, ticketId);
    }

    public async Task SendMessage(Guid ticketId, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return;

        var ticket = await ticketRepository.GetByIdAsync(ticketId, Context.ConnectionAborted);
        if (ticket is null || !CanAccess(ticket))
            throw new HubException("Ticket não encontrado ou acesso negado.");

        var thread = await GetOrCreateThreadAsync(ticketId, Context.ConnectionAborted);
        var remetenteId = Context.User!.GetUserId();

        var message = new ChatMessage
        {
            ChatThreadId = thread.Id,
            RemetenteId = remetenteId,
            Texto = texto,
        };

        await chatThreadRepository.AddMessageAsync(message, Context.ConnectionAborted);
        await chatThreadRepository.SaveChangesAsync(Context.ConnectionAborted);

        var remetente = await userManager.FindByIdAsync(remetenteId.ToString());
        var dto = new ChatMessageDto(message.Id, remetenteId, remetente?.NomeCompleto ?? "?", message.Texto, message.CreatedAt);

        await Clients.Group(ticketId.ToString()).SendAsync("ReceiveMessage", dto);
    }

    private async Task<ChatThread> GetOrCreateThreadAsync(Guid ticketId, CancellationToken ct)
    {
        var thread = await chatThreadRepository.GetByTicketIdAsync(ticketId, ct);
        if (thread is not null)
            return thread;

        thread = new ChatThread { TicketId = ticketId };
        await chatThreadRepository.AddAsync(thread, ct);
        await chatThreadRepository.SaveChangesAsync(ct);
        return thread;
    }

    private bool CanAccess(Ticket ticket)
    {
        var user = Context.User!;
        return user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Tecnico) || ticket.SolicitanteId == user.GetUserId();
    }
}
