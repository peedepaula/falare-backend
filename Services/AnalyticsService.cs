using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class AnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        // ---------- helpers ----------

        // Média de todas as notas lançadas (P1, P2, Trabalho), ignorando as nulas
        private async Task<decimal> MediaNotasAsync()
        {
            var notas = await _context.Matriculas
                .AsNoTracking()
                .Select(m => new { m.P1, m.P2, m.Trabalho })
                .ToListAsync();

            var valores = notas
                .SelectMany(n => new[] { n.P1, n.P2, n.Trabalho })
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            return valores.Count == 0 ? 0 : Math.Round(valores.Average(), 2);
        }

        // Faltas por matrícula (presença com Presente = false)
        private async Task<List<int>> FaltasPorMatriculaAsync()
        {
            return await _context.Matriculas
                .AsNoTracking()
                .Select(m => m.Presencas.Count(p => !p.Presente))
                .ToListAsync();
        }

        private static decimal Media(List<int> valores) =>
            valores.Count == 0 ? 0 : Math.Round((decimal)valores.Average(), 2);

        // ---------- rotas ----------

        public async Task<AnalyticsAlunoResponse> AlunoAsync()
        {
            var total = await _context.Alunos.CountAsync();
            var faltas = await FaltasPorMatriculaAsync();
            return new AnalyticsAlunoResponse(total, await MediaNotasAsync(), Media(faltas));
        }

        public async Task<AnalyticsGeralResponse> GeralAsync()
        {
            return new AnalyticsGeralResponse(
                await _context.Aulas.CountAsync(),
                await _context.Alunos.CountAsync(),
                await _context.Turmas.CountAsync());
        }

        public async Task<AnalyticsAulaResponse> AulaAsync()
        {
            var totalAulas = await _context.Aulas.CountAsync();
            var totalFaltas = await _context.Presencas.CountAsync(p => !p.Presente);

            var mediaFaltas = totalAulas == 0 ? 0 : Math.Round((decimal)totalFaltas / totalAulas, 2);
            return new AnalyticsAulaResponse(totalAulas, await MediaNotasAsync(), mediaFaltas);
        }

        public async Task<AnalyticsTurmaResponse> TurmaAsync()
        {
            var totalTurmas = await _context.Turmas.CountAsync();

            var mediaAlunos = totalTurmas == 0
                ? 0
                : Math.Round((decimal)await _context.Turmas.AverageAsync(t => t.Matriculados), 2);

            var totalFaltas = await _context.Presencas.CountAsync(p => !p.Presente);
            var mediaFaltas = totalTurmas == 0 ? 0 : Math.Round((decimal)totalFaltas / totalTurmas, 2);

            return new AnalyticsTurmaResponse(totalTurmas, mediaAlunos, mediaFaltas);
        }

        public async Task<AnalyticsPagamentoResponse> PagamentoAsync()
        {
            var pago = await _context.Pagamentos
                .Where(p => p.Status == StatusPagamento.Paga)
                .SumAsync(p => p.Valor);

            var pendentes = _context.Pagamentos.Where(p => p.Status == StatusPagamento.Pendente);

            // ATRASADO: suposição = pendente criado há mais de 30 dias
            var limite = DateTime.UtcNow.AddDays(-30);

            var atrasado = await pendentes
                .Where(p => p.CriadoEm < limite)
                .SumAsync(p => p.Valor);

            var pendente = await pendentes
                .Where(p => p.CriadoEm >= limite)
                .SumAsync(p => p.Valor);

            return new AnalyticsPagamentoResponse(pago, pendente, atrasado);
        }

        public async Task<AnalyticsMatriculaResponse> MatriculaAsync()
        {
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            return new AnalyticsMatriculaResponse(
                await _context.Matriculas.CountAsync(),
                await _context.Matriculas.CountAsync(m => m.Status != StatusMatricula.Ativa),
                await _context.Matriculas.CountAsync(m => m.CriadoEm >= inicioMes));
        }

        public async Task<AnalyticsProfessorResponse> ProfessorAsync()
        {
            var professores = _context.Usuarios.Where(u => u.TipoUsuario == TipoUsuario.Professor);

            var total = await professores.CountAsync();

            // INATIVO: suposição = professor sem nenhuma turma
            var inativos = await professores.CountAsync(p => !_context.Turmas.Any(t => t.ProfessorId == p.Id));

            var apagados = await _context.Usuarios
                .IgnoreQueryFilters()
                .CountAsync(u => u.TipoUsuario == TipoUsuario.Professor && u.Apagado);

            return new AnalyticsProfessorResponse(total, inativos, apagados);
        }
    }
}