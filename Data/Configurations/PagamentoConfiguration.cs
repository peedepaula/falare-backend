using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalareBackend.Data.Configurations;

public class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> e)
    {
        e.ToTable("pagamento");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.AlunoId).HasColumnName("aluno_id");
        e.Property(x => x.Status).HasColumnName("status");
        e.Property(x => x.Valor).HasColumnName("valor").HasPrecision(10, 2);
        e.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasDefaultValueSql("now()");
        e.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        e.HasOne(x => x.Aluno)
            .WithMany(a => a.Pagamentos)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => x.AlunoId).HasDatabaseName("ix_pagamento_aluno");
    }
}