using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalareBackend.Data.Configurations;

public class PresencaConfiguration : IEntityTypeConfiguration<Presenca>
{
    public void Configure(EntityTypeBuilder<Presenca> e)
    {
        e.ToTable("presenca");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.AulaId).HasColumnName("aula_id");
        e.Property(x => x.MatriculaId).HasColumnName("matricula_id");
        e.Property(x => x.Presente).HasColumnName("presente").HasDefaultValue(false);
        e.Property(x => x.ComAtestado).HasColumnName("com_atestado").HasDefaultValue(false);
        e.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        e.HasOne(x => x.Aula)
            .WithMany(a => a.Presencas)
            .HasForeignKey(x => x.AulaId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Matricula)
            .WithMany(m => m.Presencas)
            .HasForeignKey(x => x.MatriculaId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => new { x.AulaId, x.MatriculaId })
            .IsUnique()
            .HasDatabaseName("uq_presenca_aula_matricula");
        e.HasIndex(x => x.MatriculaId).HasDatabaseName("ix_presenca_matricula");
    }
}