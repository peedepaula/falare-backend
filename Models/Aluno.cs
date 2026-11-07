namespace FalareBackend.Models;

public class Aluno
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SenhaHash { get; set; }
    public string? Telefone { get; set; }
    public bool Apagado { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
}