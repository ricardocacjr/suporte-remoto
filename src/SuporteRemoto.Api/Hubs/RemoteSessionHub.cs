using Microsoft.AspNetCore.SignalR;

namespace SuporteRemoto.Api.Hubs;

/// <summary>
/// Sinalização de sessões de acesso remoto. Nesta etapa é só o esqueleto: agente e técnico
/// conseguem se anunciar e trocar mensagens de sinalização (sem captura/streaming de tela
/// ainda, que é o módulo de maior risco e será tratado em uma sessão de planejamento própria).
/// </summary>
public class RemoteSessionHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"[RemoteSessionHub] Conectado: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public async Task JoinSession(string remoteSessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, remoteSessionId);
        await Clients.OthersInGroup(remoteSessionId).SendAsync("PeerJoined", Context.ConnectionId);
    }

    public async Task SendSignal(string remoteSessionId, string payload)
    {
        await Clients.OthersInGroup(remoteSessionId).SendAsync("ReceiveSignal", payload);
    }
}
