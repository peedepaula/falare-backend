namespace FalareBackend.DTOs
{
    public record AnalyticsAlunoResponse(int TotalAlunos, decimal MediaNotas, decimal MediaFaltas);
    public record AnalyticsGeralResponse(int TotalAulas, int TotalAlunos, int TotalTurmas);
    public record AnalyticsAulaResponse(int TotalAulas, decimal MediaNotas, decimal MediaFaltasPorAula);
    public record AnalyticsTurmaResponse(int TotalTurmas, decimal MediaAlunos, decimal MediaFaltas);
    public record AnalyticsPagamentoResponse(decimal TotalPago, decimal TotalPendente, decimal TotalAtrasado);
    public record AnalyticsMatriculaResponse(int TotalMatriculas, int MatriculasInativas, int CriadasEsteMes);
    public record AnalyticsProfessorResponse(int TotalProfessores, int ProfessoresInativos, int ProfessoresApagados);
}