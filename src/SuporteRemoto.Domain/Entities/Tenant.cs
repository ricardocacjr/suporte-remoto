using SuporteRemoto.Domain.Common;

namespace SuporteRemoto.Domain.Entities;

/// <summary>
/// Placeholder for the future multi-tenant product offering. Unused while the system
/// runs single-tenant for the internal IT team (all TenantId columns stay null).
/// </summary>
public class Tenant : BaseEntity
{
    public required string Nome { get; set; }
}
