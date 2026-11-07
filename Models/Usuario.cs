using FalareBackend.Models.Enums;

namespace FalareBackend.Models;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public bool Apagado { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
}