using Microsoft.AspNetCore.Identity;

namespace SuporteRemoto.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string NomeCompleto { get; set; }
    public Guid? TenantId { get; set; }
}
