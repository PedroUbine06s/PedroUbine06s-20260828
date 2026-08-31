using FluentValidation;
using GestaoColaboradores.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoColaboradores.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IColaboradorService, ColaboradorService>();
        services.AddScoped<IUnidadeService, UnidadeService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
