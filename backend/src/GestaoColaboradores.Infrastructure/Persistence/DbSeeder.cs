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
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher, IGeradorCodigo gerador)
    {
        await context.Database.MigrateAsync();

        if (await context.Usuarios.AnyAsync()) return; // já semeado

        // Os códigos saem do mesmo gerador usado pela API, então o seed produz exatamente o
        // formato que o sistema produziria em uso normal.
        async Task<string> Codigo(TipoCodigo tipo) => await gerador.GerarAsync(tipo);

        // --- Usuários ---------------------------------------------------------------
        var admin = Usuario.Criar(await Codigo(TipoCodigo.Usuario), "admin", hasher.Hash("admin123"));
        var usuarioMaria = Usuario.Criar(await Codigo(TipoCodigo.Usuario), "maria.silva", hasher.Hash("senha123"));
        var usuarioJoao = Usuario.Criar(await Codigo(TipoCodigo.Usuario), "joao.souza", hasher.Hash("senha123"));
        var usuarioAna = Usuario.Criar(await Codigo(TipoCodigo.Usuario), "ana.costa", hasher.Hash("senha123"));

        // Sem colaborador e inativo: dá o que filtrar em GET /usuarios?ativo=false.
        var usuarioCarlos = Usuario.Criar(await Codigo(TipoCodigo.Usuario), "carlos.lima", hasher.Hash("senha123"));
        usuarioCarlos.Inativar();

        context.Usuarios.AddRange(admin, usuarioMaria, usuarioJoao, usuarioAna, usuarioCarlos);

        // --- Unidades ---------------------------------------------------------------
        var matriz = Unidade.Criar(await Codigo(TipoCodigo.Unidade), "Matriz");
        var filial = Unidade.Criar(await Codigo(TipoCodigo.Unidade), "Filial Centro");

        context.Unidades.AddRange(matriz, filial);

        // --- Colaboradores ----------------------------------------------------------
        // Com o Id gerado no domínio, as entidades já têm identidade antes de serem gravadas:
        // dá para vincular tudo e commitar de uma vez só.
        context.Colaboradores.AddRange(
            Colaborador.Criar(await Codigo(TipoCodigo.Colaborador), "Maria Silva", matriz, usuarioMaria),
            Colaborador.Criar(await Codigo(TipoCodigo.Colaborador), "João Souza", matriz, usuarioJoao),
            Colaborador.Criar(await Codigo(TipoCodigo.Colaborador), "Ana Costa", filial, usuarioAna));

        // A filial é inativada só depois de receber colaboradores: é o cenário que deixa a
        // regra central do sistema testável em segundos — a unidade mantém quem já estava,
        // mas recusa novos cadastros com 422.
        filial.Inativar();

        await context.SaveChangesAsync();
    }
}
