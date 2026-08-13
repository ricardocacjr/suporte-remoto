using System.IO;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace SuporteRemoto.RemoteViewer;

/// <summary>
/// Esqueleto do app do técnico. Nesta etapa só estabelece a conexão de sinalização com a
/// Api; visualização/controle da tela remota entram no módulo de acesso remoto propriamente
/// dito.
/// </summary>
public partial class MainWindow : Window
{
    private HubConnection? _connection;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await ConnectAsync();
        Closing += async (_, _) =>
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        };
    }

    private async Task ConnectAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var apiBaseUrl = configuration["ApiBaseUrl"]
            ?? throw new InvalidOperationException("Configuração 'ApiBaseUrl' não definida em appsettings.json.");

        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(apiBaseUrl), "hubs/remote-session"))
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("PeerJoined", connectionId => AppendLog($"Peer conectado: {connectionId}"));
        _connection.On<string>("ReceiveSignal", payload => AppendLog($"Sinal recebido: {payload}"));

        _connection.Reconnecting += _ => { StatusText.Text = "Reconectando..."; return Task.CompletedTask; };
        _connection.Reconnected += _ => { StatusText.Text = "Conectado"; return Task.CompletedTask; };
        _connection.Closed += _ => { StatusText.Text = "Desconectado"; return Task.CompletedTask; };

        try
        {
            await _connection.StartAsync();
            StatusText.Text = "Conectado";
            AppendLog("Conectado ao hub de sessão remota.");
        }
        catch (Exception ex)
        {
            StatusText.Text = "Falha na conexão";
            AppendLog($"Erro ao conectar: {ex.Message}");
        }
    }

    private void AppendLog(string message) =>
        Dispatcher.Invoke(() => LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"));
}
