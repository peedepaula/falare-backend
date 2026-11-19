using FalareBackend.Models.Enums;
using FalareBackend.Services;

public class FrequenciaAlunoResponse
{
    public Guid MatriculaId { get; set; }
    public Idioma Idioma { get; set; }
    public Nivel Nivel { get; set; }
    public int Faltas { get; set; }
    public int LimiteFaltas { get; set; }
    public SituacaoAcademica Situacao { get; set; }
}

public class NotaAlunoResponse
{
    public Guid MatriculaId { get; set; }
    public Idioma Idioma { get; set; }
    public Nivel Nivel { get; set; }
    public decimal? P1 { get; set; }
    public decimal? P2 { get; set; }
    public decimal? Trabalho { get; set; }
    public decimal? Media { get; set; }
    public SituacaoAcademica Situacao { get; set; }
}

public class FinanceiroAlunoResponse
{
    public int EmAberto { get; set; }
    public decimal ValorEmAberto { get; set; }
    public List<PagamentoHistoricoResponse> Historico { get; set; } = new();
}

public class PagamentoHistoricoResponse
{
    public Guid Id { get; set; }
    public StatusPagamento Status { get; set; }
    public decimal Valor { get; set; }
    public DateTime CriadoEm { get; set; }
}