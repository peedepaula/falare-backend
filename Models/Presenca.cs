namespace FalareBackend.Models;

public class Presenca
{
    public Guid Id { get; set; }
    public Guid AulaId { get; set; }
    public Guid MatriculaId { get; set; }
    public bool Presente { get; set; }
    public bool ComAtestado { get; set; }
    public DateTime CriadoEm { get; set; }

    public Aula Aula { get; set; } = null!;
    public Matricula Matricula { get; set; } = null!;
}