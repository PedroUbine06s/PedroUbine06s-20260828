using GestaoColaboradores.Domain.Common;
using GestaoColaboradores.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoColaboradores.Infrastructure.Persistence.Configurations;

// Unicidade de código garantida NO BANCO (unique index), não só na aplicação.
// A violação vira 409 Conflict via checagem prévia no service + tratamento no middleware.
//
// Os limites de tamanho vêm das constantes do domínio: schema e validação leem da mesma
// fonte, então é impossível o domínio aceitar um valor que a coluna rejeitaria.

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Codigo).HasMaxLength(BaseEntity.TamanhoMaximoCodigo).IsRequired();
        builder.HasIndex(u => u.Codigo).IsUnique();
        builder.Property(u => u.Login).HasMaxLength(Usuario.TamanhoMaximoLogin).IsRequired();
        builder.HasIndex(u => u.Login).IsUnique();
        builder.Property(u => u.SenhaHash).HasMaxLength(Usuario.TamanhoMaximoSenhaHash).IsRequired();
    }
}

public class ColaboradorConfiguration : IEntityTypeConfiguration<Colaborador>
{
    public void Configure(EntityTypeBuilder<Colaborador> builder)
    {
        builder.ToTable("colaboradores");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Codigo).HasMaxLength(BaseEntity.TamanhoMaximoCodigo).IsRequired();
        builder.HasIndex(c => c.Codigo).IsUnique();
        builder.Property(c => c.Nome).HasMaxLength(Colaborador.TamanhoMaximoNome).IsRequired();

        builder.HasOne(c => c.Unidade)
            .WithMany(u => u.Colaboradores)
            .HasForeignKey(c => c.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Colaborador>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UnidadeConfiguration : IEntityTypeConfiguration<Unidade>
{
    public void Configure(EntityTypeBuilder<Unidade> builder)
    {
        builder.ToTable("unidades");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Codigo).HasMaxLength(BaseEntity.TamanhoMaximoCodigo).IsRequired();
        builder.HasIndex(u => u.Codigo).IsUnique();
        builder.Property(u => u.Nome).HasMaxLength(Unidade.TamanhoMaximoNome).IsRequired();
    }
}
