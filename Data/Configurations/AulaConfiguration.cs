using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalareBackend.Data.Configurations;

public class AulaConfiguration : IEntityTypeConfiguration<Aula>
{
    public void Configure(EntityTypeBuilder<Aula> e)
    {
        e.ToTable("aula");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.TurmaId).HasColumnName("turma_id");
        e.Property(x => x.Data).HasColumnName("data");
        e.Property(x => x.Apagado).HasColumnName("apagado").HasDefaultValue(false);
        e.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        e.HasOne(x => x.Turma)
            .WithMany(t => t.Aulas)
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => new { x.TurmaId, x.Data }).HasDatabaseName("ix_aula_turma_data");
        e.HasQueryFilter(x => !x.Apagado);
    }
}