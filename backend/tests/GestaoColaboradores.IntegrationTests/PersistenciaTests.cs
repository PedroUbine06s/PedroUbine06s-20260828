using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;
using GestaoColaboradores.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GestaoColaboradores.IntegrationTests;

/// <summary>
/// Exercita o UnitOfWork direto contra o PostgreSQL, sem passar pela API.
///
/// Pela API não dá: a checagem prévia do serviço devolve 409 antes de o SaveChanges rodar, e
/// um teste que passasse por ela nunca alcançaria a tradução do erro do banco — daria
/// cobertura falsa justamente ao código escrito para essa rede de segurança. Aqui a checagem
/// é contornada de propósito, que é o que acontece de fato quando duas requisições
/// simultâneas passam por ela juntas.
/// </summary>
public class PersistenciaTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Commit_ComViolacaoDeIndiceUnico_LancaConflitoDePersistencia()
    {
        Guid unidadeId, usuarioId;

        using (var preparo = factory.Services.CreateScope())
        {
            var contexto = preparo.ServiceProvider.GetRequiredService<AppDbContext>();
            var uow = preparo.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var unidade = Unidade.Criar("UNI-UOW", "Unidade do UoW");
            var usuario = Usuario.Criar("USR-UOW", "uow.teste", "hash-irrelevante");
            contexto.AddRange(unidade, usuario);
            contexto.Add(Colaborador.Criar("COL-UOW1", "Primeiro", unidade, usuario));
            await uow.CommitAsync();

            unidadeId = unidade.Id;
            usuarioId = usuario.Id;
        }

        // Contexto novo: aqui o change tracker desconhece o colaborador que já existe, então
        // o INSERT é enviado e quem recusa é o banco — que é exatamente o cenário de duas
        // requisições simultâneas passando juntas pela checagem prévia.
        using var escopo = factory.Services.CreateScope();
        var contextoNovo = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        var uowNovo = escopo.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var unidadePersistida = (await contextoNovo.Unidades.FindAsync(unidadeId))!;
        var usuarioPersistido = (await contextoNovo.Usuarios.FindAsync(usuarioId))!;

        contextoNovo.Add(Colaborador.Criar("COL-UOW2", "Segundo", unidadePersistida, usuarioPersistido));

        var excecao = await Assert.ThrowsAsync<ConflitoDePersistenciaException>(
            () => uowNovo.CommitAsync());

        Assert.Contains("unicidade", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Commit_ComAlteracaoConcorrente_LancaConflitoDeConcorrencia()
    {
        var unidadeId = Guid.Empty;

        using (var preparo = factory.Services.CreateScope())
        {
            var contexto = preparo.ServiceProvider.GetRequiredService<AppDbContext>();
            var uow = preparo.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var unidade = Unidade.Criar("UNI-CONC", "Unidade concorrente");
            contexto.Add(unidade);
            await uow.CommitAsync();
            unidadeId = unidade.Id;
        }

        // Dois contextos carregam a mesma linha, como duas requisições simultâneas fariam.
        using var escopoA = factory.Services.CreateScope();
        using var escopoB = factory.Services.CreateScope();

        var contextoA = escopoA.ServiceProvider.GetRequiredService<AppDbContext>();
        var contextoB = escopoB.ServiceProvider.GetRequiredService<AppDbContext>();

        var versaoA = await contextoA.Unidades.FindAsync(unidadeId);
        var versaoB = await contextoB.Unidades.FindAsync(unidadeId);

        versaoA!.AlterarNome("Renomeada por A");
        await escopoA.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();

        // B ainda tem o token de versão antigo: o UPDATE não encontra a linha.
        versaoB!.AlterarNome("Renomeada por B");

        await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
            () => escopoB.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync());
    }
}
