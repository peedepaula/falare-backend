using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalareBackend.Data.Configurations;

public class MatriculaConfiguration : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> e)
    {
        e.ToTable("matricula");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.AlunoId).HasColumnName("aluno_id");
        e.Property(x => x.TurmaId).HasColumnName("turma_id");
        e.Property(x => x.Status).HasColumnName("status");
        e.Property(x => x.P1).HasColumnName("p1").HasPrecision(4, 2);
        e.Property(x => x.P2).HasColumnName("p2").HasPrecision(4, 2);
        e.Property(x => x.Trabalho).HasColumnName("trabalho").HasPrecision(4, 2);
        e.Property(x => x.Apagado).HasColumnName("apagado").HasDefaultValue(false);
        e.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        e.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasDefaultValueSql("now()");

        e.HasOne(x => x.Aluno)
            .WithMany(a => a.Matriculas)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Turma)
            .WithMany(t => t.Matriculas)
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => x.TurmaId).HasDatabaseName("ix_matricula_turma");
        e.HasQueryFilter(x => !x.Apagado);
    }
}