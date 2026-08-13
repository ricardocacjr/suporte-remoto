using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuporteRemoto.Api.Auth;
using SuporteRemoto.Application.Interfaces;
using SuporteRemoto.Domain.Entities;
using SuporteRemoto.Domain.Enums;
using SuporteRemoto.Infrastructure.Identity;
using SuporteRemoto.Shared.Chat;
using SuporteRemoto.Shared.Tickets;

namespace SuporteRemoto.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TicketsController(
    ITicketRepository ticketRepository,
    IChatThreadRepository chatThreadRepository,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> List(CancellationToken ct)
    {
        var tickets = User.IsInRole(Roles.UsuarioFinal)
            ? await ticketRepository.ListBySolicitanteAsync(User.GetUserId(), ct)
            : await ticketRepository.ListAsync(ct);

        var userIds = tickets
            .SelectMany(t => new[] { t.SolicitanteId, t.TecnicoResponsavelId })
            .Where(id => id.HasValue).Select(id => id!.Value)
            .Distinct().ToList();
        var names = await ResolveNamesAsync(userIds);

        return Ok(tickets.Select(t => ToDto(t, names)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var ticket = await ticketRepository.GetWithDetailsAsync(id, ct);
        if (ticket is null || !CanAccess(ticket))
            return NotFound();

        var userIds = new List<Guid> { ticket.SolicitanteId };
        if (ticket.TecnicoResponsavelId is { } tecnicoId) userIds.Add(tecnicoId);
        userIds.AddRange(ticket.Comentarios.Select(c => c.AutorId));
        userIds.AddRange(ticket.Anexos.Select(a => a.EnviadoPorId));
        var names = await ResolveNamesAsync(userIds.Distinct().ToList());

        return Ok(ToDetailDto(ticket, names));
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request, CancellationToken ct)
    {
        var ticket = new Ticket
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            Prioridade = request.Prioridade,
            SolicitanteId = request.SolicitanteId,
        };

        await ticketRepository.AddAsync(ticket, ct);
        await ticketRepository.SaveChangesAsync(ct);

        var names = await ResolveNamesAsync([ticket.SolicitanteId]);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ToDto(ticket, names));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTicketStatusRequest request, CancellationToken ct)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, ct);
        if (ticket is null)
            return NotFound();

        ticket.Status = request.Status;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        ticketRepository.Update(ticket);
        await ticketRepository.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = Roles.Tecnico + "," + Roles.Admin)]
    public async Task<IActionResult> AssignToSelf(Guid id, CancellationToken ct)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, ct);
        if (ticket is null)
            return NotFound();

        ticket.TecnicoResponsavelId = User.GetUserId();
        if (ticket.Status == TicketStatus.Aberto)
            ticket.Status = TicketStatus.EmAndamento;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        ticketRepository.Update(ticket);
        await ticketRepository.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<TicketCommentDto>> AddComment(Guid id, AddCommentRequest request, CancellationToken ct)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, ct);
        if (ticket is null || !CanAccess(ticket))
            return NotFound();

        var comment = new TicketComment
        {
            TicketId = id,
            AutorId = User.GetUserId(),
            Texto = request.Texto,
        };

        await ticketRepository.AddCommentAsync(comment, ct);
        await ticketRepository.SaveChangesAsync(ct);

        var names = await ResolveNamesAsync([comment.AutorId]);
        return Ok(ToCommentDto(comment, names));
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<ActionResult<TicketAttachmentDto>> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, ct);
        if (ticket is null || !CanAccess(ticket))
            return NotFound();

        if (file.Length == 0)
            return BadRequest("Arquivo vazio.");

        var basePath = Path.Combine(
            environment.ContentRootPath,
            configuration["Storage:TicketAttachmentsPath"] ?? "App_Data/attachments",
            id.ToString());
        Directory.CreateDirectory(basePath);

        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(basePath, storedFileName);

        await using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream, ct);

        var attachment = new TicketAttachment
        {
            TicketId = id,
            EnviadoPorId = User.GetUserId(),
            NomeArquivo = file.FileName,
            CaminhoArmazenamento = fullPath,
            TamanhoBytes = file.Length,
            TipoConteudo = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
        };

        await ticketRepository.AddAttachmentAsync(attachment, ct);
        await ticketRepository.SaveChangesAsync(ct);

        var names = await ResolveNamesAsync([attachment.EnviadoPorId]);
        return Ok(ToAttachmentDto(attachment, names));
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken ct)
    {
        var ticket = await ticketRepository.GetWithDetailsAsync(id, ct);
        if (ticket is null || !CanAccess(ticket))
            return NotFound();

        var attachment = ticket.Anexos.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null || !System.IO.File.Exists(attachment.CaminhoArmazenamento))
            return NotFound();

        var stream = System.IO.File.OpenRead(attachment.CaminhoArmazenamento);
        return File(stream, attachment.TipoConteudo, attachment.NomeArquivo);
    }

    [HttpGet("{id:guid}/chat/messages")]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatMessages(Guid id, CancellationToken ct)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, ct);
        if (ticket is null || !CanAccess(ticket))
            return NotFound();

        var thread = await chatThreadRepository.GetByTicketIdAsync(id, ct);
        if (thread is null)
            return Ok(Array.Empty<ChatMessageDto>());

        var names = await ResolveNamesAsync(thread.Mensagens.Select(m => m.RemetenteId).Distinct().ToList());
        return Ok(thread.Mensagens
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto(m.Id, m.RemetenteId, names.GetValueOrDefault(m.RemetenteId, "?"), m.Texto, m.CreatedAt)));
    }

    private bool CanAccess(Ticket ticket) =>
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Tecnico) || ticket.SolicitanteId == User.GetUserId();

    private async Task<Dictionary<Guid, string>> ResolveNamesAsync(IReadOnlyCollection<Guid> ids)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();

        return await userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NomeCompleto);
    }

    private static TicketDto ToDto(Ticket t, IReadOnlyDictionary<Guid, string> names) => new(
        t.Id, t.Titulo, t.Descricao, t.Status, t.Prioridade,
        t.SolicitanteId, names.GetValueOrDefault(t.SolicitanteId, "?"),
        t.TecnicoResponsavelId, t.TecnicoResponsavelId is { } tid ? names.GetValueOrDefault(tid, "?") : null,
        t.CreatedAt);

    private static TicketDetailDto ToDetailDto(Ticket t, IReadOnlyDictionary<Guid, string> names) => new(
        t.Id, t.Titulo, t.Descricao, t.Status, t.Prioridade,
        t.SolicitanteId, names.GetValueOrDefault(t.SolicitanteId, "?"),
        t.TecnicoResponsavelId, t.TecnicoResponsavelId is { } tid ? names.GetValueOrDefault(tid, "?") : null,
        t.CreatedAt, t.ResolvidoEm, t.FechadoEm,
        t.Comentarios.OrderBy(c => c.CreatedAt).Select(c => ToCommentDto(c, names)).ToList(),
        t.Anexos.OrderBy(a => a.CreatedAt).Select(a => ToAttachmentDto(a, names)).ToList());

    private static TicketCommentDto ToCommentDto(TicketComment c, IReadOnlyDictionary<Guid, string> names) => new(
        c.Id, c.AutorId, names.GetValueOrDefault(c.AutorId, "?"), c.Texto, c.CreatedAt);

    private static TicketAttachmentDto ToAttachmentDto(TicketAttachment a, IReadOnlyDictionary<Guid, string> names) => new(
        a.Id, a.NomeArquivo, a.TamanhoBytes, a.TipoConteudo, a.EnviadoPorId, names.GetValueOrDefault(a.EnviadoPorId, "?"), a.CreatedAt);
}
