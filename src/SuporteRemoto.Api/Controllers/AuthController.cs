using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuporteRemoto.Api.Auth;
using SuporteRemoto.Infrastructure.Identity;
using SuporteRemoto.Shared.Auth;

namespace SuporteRemoto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService tokenService) : ControllerBase
{
    private static readonly string[] StaffRoles = [Roles.Tecnico, Roles.Admin];

    /// <summary>
    /// Cadastro com senha — só pra equipe (Técnico/Admin). Usuário final não passa por aqui,
    /// usa <see cref="Enter"/>.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (!StaffRoles.Contains(request.Role))
            return BadRequest($"Papel inválido. Use um de: {string.Join(", ", StaffRoles)}");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            NomeCompleto = request.NomeCompleto,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, request.Role);

        var token = tokenService.GenerateToken(user, [request.Role]);
        return Ok(new AuthResponse(token));
    }

    /// <summary>
    /// Login com senha — só pra equipe (Técnico/Admin).
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Any(StaffRoles.Contains))
            return Unauthorized();

        var token = tokenService.GenerateToken(user, roles);
        return Ok(new AuthResponse(token));
    }

    /// <summary>
    /// Entrada sem senha pra usuário final: cria a conta automaticamente no primeiro acesso
    /// (com uma senha aleatória que nunca é usada/exposta) e devolve o token. Serve só pra abrir
    /// e acompanhar chamados — não dá pra entrar como equipe por aqui.
    /// </summary>
    [HttpPost("enter")]
    public async Task<ActionResult<AuthResponse>> Enter(EnterRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NomeCompleto = request.NomeCompleto,
            };

            var randomPassword = $"{Guid.NewGuid():N}Aa1!";
            var result = await userManager.CreateAsync(user, randomPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await userManager.AddToRoleAsync(user, Roles.UsuarioFinal);
        }

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains(Roles.UsuarioFinal))
            return BadRequest("Este e-mail já é usado pela equipe. Use o acesso da equipe pra entrar.");

        var token = tokenService.GenerateToken(user, roles);
        return Ok(new AuthResponse(token));
    }
}
