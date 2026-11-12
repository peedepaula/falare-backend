using System.ComponentModel.DataAnnotations;

namespace FalareBackend.DTOs
{
    public class LancarChamadaRequest
    {
        [Required] public Guid TurmaId { get; set; }
        [Required] public DateTime Data { get; set; }
        [Required, MinLength(1)] public List<ItemChamadaRequest> Itens { get; set; } = new();
    }

    public class ItemChamadaRequest
    {
        [Required] public Guid MatriculaId { get; set; }
        public bool Presente { get; set; }
        public bool ComAtestado { get; set; }   // só vale quando Presente = false
    }

    public class ChamadaResponse
    {
        public Guid? AulaId { get; set; }
        public Guid TurmaId { get; set; }
        public DateTime Data { get; set; }
        public List<ChamadaAlunoResponse> Alunos { get; set; } = new();
    }

    public class ChamadaAlunoResponse
    {
        public Guid MatriculaId { get; set; }
        public Guid AlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;
        public bool? Presente { get; set; }      // null = ainda não lançado
        public bool ComAtestado { get; set; }
        public int Faltas { get; set; }
        public int LimiteFaltas { get; set; }
        public decimal PercentualFaltas { get; set; }
        public string Situacao { get; set; } = string.Empty;
        public bool EmRisco { get; set; }
    }
}