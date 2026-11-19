using FalareBackend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FalareBackend.DTOs
{
    public class CadastrarAlunoRequest
    {
        [Required, MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress, MaxLength(255)]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? Senha { get; set; }

        [MaxLength(30)]
        public string? Telefone { get; set; }
    }

    public class AtualizarAlunoRequest
    {
        [MaxLength(150)]
        public string? Nome { get; set; }

        [EmailAddress, MaxLength(255)]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? Senha { get; set; }

        [MaxLength(30)]
        public string? Telefone { get; set; }
    }

    public class AlunoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Guid? MatriculaId { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }

    public class AlunoCompletoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public List<MatriculaAlunoResponse> Matriculas { get; set; } = new();
    }

    public class MatriculaAlunoResponse
    {
        public Guid MatriculaId { get; set; }
        public Guid TurmaId { get; set; }
        public Idioma Idioma { get; set; }
        public Nivel Nivel { get; set; }
        public StatusMatricula Status { get; set; }
        public decimal? P1 { get; set; }
        public decimal? P2 { get; set; }
        public decimal? Trabalho { get; set; }
        public int Faltas { get; set; }
    }
}