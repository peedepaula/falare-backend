using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Aula> Aulas => Set<Aula>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Presenca> Presencas => Set<Presenca>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ENUMs nativos do PostgreSQL
        modelBuilder.HasPostgresEnum<Idioma>(name: "idioma_enum");
        modelBuilder.HasPostgresEnum<Nivel>(name: "nivel_enum");
        modelBuilder.HasPostgresEnum<TipoUsuario>(name: "tipo_usuario_enum");
        modelBuilder.HasPostgresEnum<StatusMatricula>(name: "status_matricula_enum");
        modelBuilder.HasPostgresEnum<StatusPagamento>(name: "status_pagamento_enum");

        // Carrega automaticamente todas as classes IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}