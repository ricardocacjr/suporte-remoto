namespace SuporteRemoto.Shared.Auth;

/// <summary>
/// Entrada sem senha para usuário final — usada só pra abrir/acompanhar chamados. Cria a conta
/// automaticamente no primeiro acesso com aquele e-mail.
/// </summary>
public record EnterRequest(string NomeCompleto, string Email);
