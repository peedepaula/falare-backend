using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalareBackend.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> e)
    {
        e.ToTable("usuario");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
        e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        e.Property(x => x.SenhaHash).HasColumnName("senha_hash").IsRequired();
        e.Property(x => x.TipoUsuario).HasColumnName("tipo_usuario");
        e.Property(x => x.Apagado).HasColumnName("apagado").HasDefaultValue(false);
        e.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        e.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasDefaultValueSql("now()");

        e.HasQueryFilter(x => !x.Apagado);
    }
}