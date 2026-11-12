using System.ComponentModel.DataAnnotations;
using FalareBackend.Models.Enums;

namespace FalareBackend.DTOs
{
    public class CadastrarTurmasRequest
    {
        public Idioma Idioma { get; set; }
        public Nivel Nivel { get; set; }

        [Required, MinLength(1)]
        public string[] Dias { get; set; } = Array.Empty<string>();

        [Required]
        public TimeOnly Horario { get; set; }

        [Required]
        public Guid ProfessorId { get; set; }

        [Range(1, 12)]
        public int Vagas { get; set; }
    }

    public class AtualizarTurmasRequest
    {
        public Idioma? Idioma { get; set; }
        public Nivel? Nivel { get; set; }
        public string[]? Dias { get; set; }
        public TimeOnly? Horario { get; set; }
        public Guid? ProfessorId { get; set; }

        [Range(1, 12)]
        public int? Vagas { get; set; }
    }

    public class TurmaResponse
    {
        public Guid Id { get; set; }
        public Idioma Idioma { get; set; }
        public Nivel Nivel { get; set; }
        public string[] Dias { get; set; } = Array.Empty<string>();
        public TimeOnly Horario { get; set; }
        public Guid ProfessorId { get; set; }
        public string ProfessorNome { get; set; } = string.Empty;
        public int Matriculados { get; set; }
        public int Vagas { get; set; }
    }
}