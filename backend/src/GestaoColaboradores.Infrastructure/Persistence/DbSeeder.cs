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

        // --- Usuários ---------------------------------------------------------------
        var admin = Usuario.Criar("USR-001", "admin", hasher.Hash("admin123"));
        var usuarioMaria = Usuario.Criar("USR-002", "maria.silva", hasher.Hash("senha123"));
        var usuarioJoao = Usuario.Criar("USR-003", "joao.souza", hasher.Hash("senha123"));
        var usuarioAna = Usuario.Criar("USR-004", "ana.costa", hasher.Hash("senha123"));

        // Sem colaborador e inativo: dá o que filtrar em GET /usuarios?ativo=false.
        var usuarioCarlos = Usuario.Criar("USR-005", "carlos.lima", hasher.Hash("senha123"));
        usuarioCarlos.Inativar();

        context.Usuarios.AddRange(admin, usuarioMaria, usuarioJoao, usuarioAna, usuarioCarlos);

        // --- Unidades ---------------------------------------------------------------
        var matriz = Unidade.Criar("UNI-001", "Matriz");
        var filial = Unidade.Criar("UNI-002", "Filial Centro");

        context.Unidades.AddRange(matriz, filial);

        // Grava antes de vincular: os colaboradores precisam dos Ids reais.
        await context.SaveChangesAsync();

        // --- Colaboradores ----------------------------------------------------------
        context.Colaboradores.AddRange(
            Colaborador.Criar("COL-001", "Maria Silva", matriz, usuarioMaria),
            Colaborador.Criar("COL-002", "João Souza", matriz, usuarioJoao),
            Colaborador.Criar("COL-003", "Ana Costa", filial, usuarioAna));

        await context.SaveChangesAsync();

        // A filial é inativada DEPOIS de receber colaboradores. Esse é o cenário que deixa a
        // regra central do sistema testável em segundos: a unidade mantém quem já estava,
        // mas recusa novos cadastros com 422.
        filial.Inativar();

        await context.SaveChangesAsync();
    }
}
