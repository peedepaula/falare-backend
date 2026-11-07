namespace FalareBackend.Models;

public class Aula
{
    public Guid Id { get; set; }
    public Guid TurmaId { get; set; }
    public DateTime Data { get; set; }
    public bool Apagado { get; set; }
    public DateTime CriadoEm { get; set; }

    public Turma Turma { get; set; } = null!;
    public ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();
}