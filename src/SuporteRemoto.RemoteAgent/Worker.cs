using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace SuporteRemoto.RemoteAgent;

/// <summary>
/// Esqueleto do agente instalado na máquina do usuário final. Nesta etapa só estabelece
/// a conexão de sinalização com a Api e anuncia presença; captura/streaming de tela e
/// injeção de input entram no módulo de acesso remoto propriamente dito.
/// </summary>
public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    private readonly Guid _agentHostId = Guid.NewGuid();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"]
            ?? throw new InvalidOperationException("Configuração 'ApiBaseUrl' não definida em appsettings.json.");

        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(apiBaseUrl), "hubs/remote-session"))
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("ReceiveSignal", payload =>
            logger.LogInformation("Sinal recebido: {Payload}", payload));

        connection.On<string>("PeerJoined", connectionId =>
            logger.LogInformation("Técnico conectado: {ConnectionId}", connectionId));

        await connection.StartAsync(stoppingToken);
        logger.LogInformation("Agente {AgentHostId} conectado ao hub de sessão remota.", _agentHostId);

        await connection.InvokeAsync("JoinSession", _agentHostId.ToString(), stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            await connection.DisposeAsync();
        }
    }
}
