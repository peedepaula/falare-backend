using FalareBackend.Models.Enums;

namespace FalareBackend.Models;

public class Matricula
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public Guid TurmaId { get; set; }
    public StatusMatricula Status { get; set; } = StatusMatricula.Ativa;
    public decimal? P1 { get; set; }
    public decimal? P2 { get; set; }
    public decimal? Trabalho { get; set; }
    public bool Apagado { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public Aluno Aluno { get; set; } = null!;
    public Turma Turma { get; set; } = null!;
    public ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();
}