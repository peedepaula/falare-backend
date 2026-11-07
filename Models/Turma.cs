using FalareBackend.Models.Enums;

namespace FalareBackend.Models;

public class Turma
{
    public Guid Id { get; set; }
    public Idioma Idioma { get; set; }
    public Nivel Nivel { get; set; }

    /// <summary>Valores permitidos: seg, ter, qua, qui, sex, sab, dom.</summary>
    public string[] Dias { get; set; } = Array.Empty<string>();

    public TimeOnly Horario { get; set; }
    public Guid ProfessorId { get; set; }
    public int Matriculados { get; set; }
    public int Vagas { get; set; }
    public bool Apagado { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public Usuario Professor { get; set; } = null!;
    public ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}