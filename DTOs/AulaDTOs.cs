using FalareBackend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FalareBackend.DTOs
{
    public class CadastrarAulaRequest
    {
        [Required]
        public Guid TurmaId { get; set; }

        [Required]
        public DateTime Data { get; set; }
    }

    public class AulaResponse
    {
        public Guid Id { get; set; }
        public Guid TurmaId { get; set; }
        public DateTime Data { get; set; }
        public string ProfessorNome { get; set; } = string.Empty;
        public Idioma Idioma { get; set; }
        public Nivel Nivel { get; set; }
        public TimeOnly HorarioInicio { get; set; }
        public TimeOnly HorarioFim { get; set; }
    }
}