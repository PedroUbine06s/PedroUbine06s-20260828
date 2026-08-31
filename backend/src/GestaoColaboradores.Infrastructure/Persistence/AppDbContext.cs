using GestaoColaboradores.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<Unidade> Unidades => Set<Unidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
