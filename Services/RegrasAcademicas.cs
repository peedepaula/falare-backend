namespace FalareBackend.Services;

public enum SituacaoAcademica { EmAndamento, Aprovado, ReprovadoPorFalta, ReprovadoPorNota }

public record SituacaoAluno(
    int Faltas, int LimiteFaltas, decimal PercentualFaltas,
    decimal? Media, SituacaoAcademica Situacao, bool EmRisco);

public static class RegrasAcademicas
{
    public const int AulasPorSemestre = 32;
    public const int VagasPorTurma = 12;
    public const decimal MediaAprovacao = 7.0m;
    public const decimal RiscoFaltas = 0.15m;
    public static readonly DateOnly VigenciaLimite20 = new(2026, 11, 1);

    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateOnly Hoje() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Fuso));

    // Mudança contratada: matrícula criada até 31/10/2026 → 8 faltas (25%); a partir de 01/11/2026 → 6 faltas (20%)
    public static int LimiteFaltas(DateTime matriculaCriadaEm)
    {
        var utc = matriculaCriadaEm.Kind == DateTimeKind.Local
            ? matriculaCriadaEm.ToUniversalTime()
            : DateTime.SpecifyKind(matriculaCriadaEm, DateTimeKind.Utc);
        var data = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, Fuso));
        return data >= VigenciaLimite20 ? 6 : 8;
    }

    // RN-02
    public static decimal? CalcularMedia(decimal? p1, decimal? p2, decimal? trabalho)
    {
        if (p1 is null || p2 is null || trabalho is null) return null;
        return Math.Round((p1.Value * 3 + p2.Value * 3 + trabalho.Value * 4) / 10m,
                          1, MidpointRounding.AwayFromZero);
    }

    // RN-01 + RN-02. As faltas já incluem as com atestado (RN-08).
    public static SituacaoAluno Calcular(int faltas, int limiteFaltas,
        decimal? p1, decimal? p2, decimal? trabalho)
    {
        var media = CalcularMedia(p1, p2, trabalho);

        var situacao =
            faltas > limiteFaltas ? SituacaoAcademica.ReprovadoPorFalta :
            media is null ? SituacaoAcademica.EmAndamento :
            media >= MediaAprovacao ? SituacaoAcademica.Aprovado :
            SituacaoAcademica.ReprovadoPorNota;

        var emRisco = (decimal)faltas / AulasPorSemestre > RiscoFaltas
                      || (media is not null && media < MediaAprovacao);

        return new SituacaoAluno(faltas, limiteFaltas,
            Math.Round(faltas * 100m / AulasPorSemestre, 1), media, situacao, emRisco);
    }

    public static string Dia(DateTime d) => d.DayOfWeek switch
    {
        DayOfWeek.Monday => "seg",
        DayOfWeek.Tuesday => "ter",
        DayOfWeek.Wednesday => "qua",
        DayOfWeek.Thursday => "qui",
        DayOfWeek.Friday => "sex",
        DayOfWeek.Saturday => "sab",
        _ => "dom"
    };

    // Aula.Data é DateTime no banco: guardamos sempre a data à meia-noite.
    // Se a coluna for "timestamp without time zone", troque Utc por Unspecified aqui (único lugar).
    public static DateTime DataDaAula(DateTime d) =>
        DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

    public static DateOnly ParaDateOnly(DateTime d) => DateOnly.FromDateTime(d);
}