using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Infrastructure.Auth;
using GestaoColaboradores.Infrastructure.Persistence;
using GestaoColaboradores.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoColaboradores.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IColaboradorRepository, ColaboradorRepository>();
        services.AddScoped<IUnidadeRepository, UnidadeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGeradorCodigo, GeradorCodigo>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>(); // Strategy
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Secao)); // Options
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
