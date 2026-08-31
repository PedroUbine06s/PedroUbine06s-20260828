using GestaoColaboradores.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Uma sequence por entidade: os códigos de cada tipo numeram de forma independente.
    public const string SequenceUsuarios = "seq_codigo_usuario";
    public const string SequenceUnidades = "seq_codigo_unidade";
    public const string SequenceColaboradores = "seq_codigo_colaborador";

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<Unidade> Unidades => Set<Unidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(SequenceUsuarios).StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<long>(SequenceUnidades).StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<long>(SequenceColaboradores).StartsAt(1).IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
