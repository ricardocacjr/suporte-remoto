using System.Net.Http.Headers;
using System.Net.Http.Json;
using SuporteRemoto.Shared.Auth;
using SuporteRemoto.Shared.Chat;
using SuporteRemoto.Shared.Tickets;

namespace SuporteRemoto.Web.Services;

public class SuporteRemotoApiClient(HttpClient http, AuthState authState)
{
    public Uri? ApiBaseAddress => http.BaseAddress;

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> RegisterAsync(string nomeCompleto, string email, string password, string role)
    {
        var response = await http.PostAsJsonAsync("api/auth/register", new RegisterRequest(nomeCompleto, email, password, role));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<IReadOnlyList<TicketDto>> GetTicketsAsync()
    {
        using var request = AuthorizedRequest(HttpMethod.Get, "api/tickets");
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TicketDto>>() ?? [];
    }

    public async Task<TicketDto?> CreateTicketAsync(CreateTicketRequest ticket)
    {
        using var request = AuthorizedRequest(HttpMethod.Post, "api/tickets");
        request.Content = JsonContent.Create(ticket);
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketDto>();
    }

    public async Task<TicketDetailDto?> GetTicketDetailAsync(Guid id)
    {
        using var request = AuthorizedRequest(HttpMethod.Get, $"api/tickets/{id}");
        var response = await http.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketDetailDto>();
    }

    public async Task AssignToSelfAsync(Guid ticketId)
    {
        using var request = AuthorizedRequest(HttpMethod.Post, $"api/tickets/{ticketId}/assign");
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TicketCommentDto?> AddCommentAsync(Guid ticketId, string texto)
    {
        using var request = AuthorizedRequest(HttpMethod.Post, $"api/tickets/{ticketId}/comments");
        request.Content = JsonContent.Create(new AddCommentRequest(texto));
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketCommentDto>();
    }

    public async Task<TicketAttachmentDto?> UploadAttachmentAsync(Guid ticketId, Stream fileStream, string fileName, string contentType)
    {
        using var request = AuthorizedRequest(HttpMethod.Post, $"api/tickets/{ticketId}/attachments");
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        request.Content = content;

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketAttachmentDto>();
    }

    public string GetAttachmentDownloadUrl(Guid ticketId, Guid attachmentId) =>
        new Uri(http.BaseAddress!, $"api/tickets/{ticketId}/attachments/{attachmentId}?access_token={Uri.EscapeDataString(authState.Token ?? string.Empty)}").ToString();

    public async Task<IReadOnlyList<ChatMessageDto>> GetChatMessagesAsync(Guid ticketId)
    {
        using var request = AuthorizedRequest(HttpMethod.Get, $"api/tickets/{ticketId}/chat/messages");
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ChatMessageDto>>() ?? [];
    }

    private HttpRequestMessage AuthorizedRequest(HttpMethod method, string uri)
    {
        var request = new HttpRequestMessage(method, uri);
        if (authState.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
        return request;
    }
}
