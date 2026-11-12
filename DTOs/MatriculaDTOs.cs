using System.ComponentModel.DataAnnotations;
using FalareBackend.Models.Enums;

namespace FalareBackend.DTOs
{
    public class CadastrarMatriculaRequest
    {
        [Required]
        public Guid AlunoId { get; set; }

        [Required]
        public Guid TurmaId { get; set; }
    }

    // Só secretaria/professor lançam notas e mudam o status
    public class AtualizarMatriculaRequest
    {
        public StatusMatricula? Status { get; set; }

        [Range(0, 10)]
        public decimal? P1 { get; set; }

        [Range(0, 10)]
        public decimal? P2 { get; set; }

        [Range(0, 10)]
        public decimal? Trabalho { get; set; }
    }

    public class MatriculaResponse
    {
        public Guid Id { get; set; }
        public Guid AlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;
        public Guid TurmaId { get; set; }
        public Idioma Idioma { get; set; }
        public Nivel Nivel { get; set; }
        public StatusMatricula Status { get; set; }
        public decimal? P1 { get; set; }
        public decimal? P2 { get; set; }
        public decimal? Trabalho { get; set; }
    }
}