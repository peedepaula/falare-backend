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
    }
}