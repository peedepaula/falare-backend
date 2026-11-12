using System.ComponentModel.DataAnnotations;
using FalareBackend.Models.Enums;

namespace FalareBackend.DTOs
{
    public class CadastrarPagamentoRequest
    {
        [Required]
        public Guid AlunoId { get; set; }

        public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

        [Range(0.01, 1000000)]
        public decimal Valor { get; set; }
    }

    public class AtualizarPagamentoRequest
    {
        public StatusPagamento? Status { get; set; }

        [Range(0.01, 1000000)]
        public decimal? Valor { get; set; }
    }

    public class PagamentoResponse
    {
        public Guid Id { get; set; }
        public Guid AlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;
        public StatusPagamento Status { get; set; }
        public decimal Valor { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}