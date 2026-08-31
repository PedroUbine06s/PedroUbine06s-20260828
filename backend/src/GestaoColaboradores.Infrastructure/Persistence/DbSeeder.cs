using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence;

/// <summary>
/// Seed executado no startup: garante que o avaliador loga e testa em segundos.
/// Inclui de propósito uma unidade INATIVA para a regra de bloqueio ser testável de cara.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher)
    {
        await context.Database.MigrateAsync();

        if (await context.Usuarios.AnyAsync()) return; // já semeado

        var admin = Usuario.Criar("USR-001", "admin", hasher.Hash("admin123"));
        context.Usuarios.Add(admin);

        // TODO:
        // - Unidade "UNI-001" Matriz (ativa) e "UNI-002" Filial Desativada (chamar Inativar())
        // - 1–2 usuários extras + colaboradores de exemplo via Colaborador.Criar(...)
        await context.SaveChangesAsync();
    }
}
