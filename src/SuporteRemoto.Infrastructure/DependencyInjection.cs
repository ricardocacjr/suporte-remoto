using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuporteRemoto.Application.Interfaces;
using SuporteRemoto.Infrastructure.Identity;
using SuporteRemoto.Infrastructure.Persistence;
using SuporteRemoto.Infrastructure.Persistence.Repositories;

namespace SuporteRemoto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IChatThreadRepository, ChatThreadRepository>();
        services.AddScoped<IRemoteSessionRepository, RemoteSessionRepository>();

        return services;
    }
}
