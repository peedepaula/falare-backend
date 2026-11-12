using System.ComponentModel.DataAnnotations;

namespace FalareBackend.DTOs
{
    public class CadastrarProfessorRequest
    {
        [Required, MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Senha { get; set; } = string.Empty;
    }

    public class AtualizarProfessorRequest
    {
        [MaxLength(150)]
        public string? Nome { get; set; }

        [EmailAddress, MaxLength(255)]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? Senha { get; set; }
    }
}