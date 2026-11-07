using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalareBackend.Data.Configurations;

public class TurmaConfiguration : IEntityTypeConfiguration<Turma>
{
    public void Configure(EntityTypeBuilder<Turma> e)
    {
        e.ToTable("turma");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.Idioma).HasColumnName("idioma");
        e.Property(x => x.Nivel).HasColumnName("nivel");
        e.Property(x => x.Dias).HasColumnName("dias").HasColumnType("text[]");
        e.Property(x => x.Horario).HasColumnName("horario");
        e.Property(x => x.ProfessorId).HasColumnName("professor_id");
        e.Property(x => x.Matriculados).HasColumnName("matriculados").HasDefaultValue(0);
        e.Property(x => x.Vagas).HasColumnName("vagas");
        e.Property(x => x.Apagado).HasColumnName("apagado").HasDefaultValue(false);
        e.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        e.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasDefaultValueSql("now()");

        e.HasOne(x => x.Professor)
            .WithMany(u => u.Turmas)
            .HasForeignKey(x => x.ProfessorId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => x.ProfessorId).HasDatabaseName("ix_turma_professor");
        e.HasQueryFilter(x => !x.Apagado);
    }
}