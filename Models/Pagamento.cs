using FalareBackend.Models.Enums;

namespace FalareBackend.Models;

public class Pagamento
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;
    public decimal Valor { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public DateTime CriadoEm { get; set; }

    public Aluno Aluno { get; set; } = null!;
}