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
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }
}