namespace SuporteRemoto.Infrastructure.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Tecnico = "Tecnico";
    public const string UsuarioFinal = "UsuarioFinal";

    public static readonly string[] All = [Admin, Tecnico, UsuarioFinal];
}
