using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class ChamadaService
    {
        private readonly AppDbContext _context;
        public ChamadaService(AppDbContext context) => _context = context;

        private static readonly StatusMatricula[] NaChamada =
            { StatusMatricula.Ativa, StatusMatricula.ReprovadaFalta };

        public async Task<(ChamadaResponse? Chamada, string? Erro)> BuscarChamadaService(
            Guid turmaId, DateTime data, Guid usuarioId, bool ehSecretaria)
        {
            var turma = await _context.Turmas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == turmaId);
            if (turma == null) return (null, "turma_nao_encontrada");
            if (!ehSecretaria && turma.ProfessorId != usuarioId) return (null, "sem_permissao");

            var dataAula = RegrasAcademicas.DataDaAula(data);
            var aula = await _context.Aulas.AsNoTracking()
                .FirstOrDefaultAsync(a => a.TurmaId == turmaId && a.Data == dataAula);

            return (await MontarRespostaAsync(turma.Id, dataAula, aula?.Id), null);
        }

        public async Task<(ChamadaResponse? Chamada, string? Erro)> LancarChamadaService(
            LancarChamadaRequest req, Guid usuarioId, bool ehSecretaria)
        {
            var dataAula = RegrasAcademicas.DataDaAula(req.Data);

            // RN-06
            if (RegrasAcademicas.ParaDateOnly(dataAula) > RegrasAcademicas.Hoje())
                return (null, "data_futura");

            var turma = await _context.Turmas.FirstOrDefaultAsync(t => t.Id == req.TurmaId);
            if (turma == null) return (null, "turma_nao_encontrada");
            if (!ehSecretaria && turma.ProfessorId != usuarioId) return (null, "sem_permissao");

            if (!turma.Dias.Contains(RegrasAcademicas.Dia(dataAula))) return (null, "dia_fora_da_grade");

            var matriculas = await _context.Matriculas
                .Where(m => m.TurmaId == turma.Id && NaChamada.Contains(m.Status))
                .ToListAsync();

            var ids = matriculas.Select(m => m.Id).ToHashSet();
            if (req.Itens.GroupBy(i => i.MatriculaId).Any(g => g.Count() > 1)) return (null, "item_duplicado");
            if (req.Itens.Any(i => !ids.Contains(i.MatriculaId))) return (null, "matricula_invalida");
            if (req.Itens.Count != ids.Count) return (null, "chamada_incompleta");

            var aula = await _context.Aulas.Include(a => a.Presencas)
                .FirstOrDefaultAsync(a => a.TurmaId == turma.Id && a.Data == dataAula);

            if (aula == null)
            {
                var total = await _context.Aulas.CountAsync(a => a.TurmaId == turma.Id);
                if (total >= RegrasAcademicas.AulasPorSemestre) return (null, "limite_de_aulas");
                aula = new Aula { TurmaId = turma.Id, Data = dataAula };
                _context.Aulas.Add(aula);
            }

            var existentes = aula.Presencas.ToDictionary(p => p.MatriculaId);
            foreach (var item in req.Itens)
            {
                var comAtestado = !item.Presente && item.ComAtestado;
                if (existentes.TryGetValue(item.MatriculaId, out var p))
                {
                    p.Presente = item.Presente;
                    p.ComAtestado = comAtestado;
                }
                else
                {
                    aula.Presencas.Add(new Presenca
                    {
                        MatriculaId = item.MatriculaId,
                        Presente = item.Presente,
                        ComAtestado = comAtestado
                    });
                }
            }
            await _context.SaveChangesAsync();

            // Caso 3: passou do limite → ReprovadaFalta (e volta se a falta foi corrigida)
            var faltas = await FaltasPorMatriculaAsync(ids);
            foreach (var m in matriculas)
            {
                var f = faltas.GetValueOrDefault(m.Id);
                var limite = RegrasAcademicas.LimiteFaltas(m.CriadoEm);
                if (f > limite && m.Status == StatusMatricula.Ativa)
                    m.Status = StatusMatricula.ReprovadaFalta;
                else if (f <= limite && m.Status == StatusMatricula.ReprovadaFalta)
                    m.Status = StatusMatricula.Ativa;
                else continue;
                m.AtualizadoEm = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            return (await MontarRespostaAsync(turma.Id, dataAula, aula.Id), null);
        }

        // RN-08: ComAtestado não entra no filtro, falta com atestado conta
        private async Task<Dictionary<Guid, int>> FaltasPorMatriculaAsync(IEnumerable<Guid> ids) =>
            await _context.Presencas
                .Where(p => ids.Contains(p.MatriculaId) && !p.Presente && !p.Aula.Apagado)
                .GroupBy(p => p.MatriculaId)
                .Select(g => new { g.Key, Qtd = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Qtd);

        private async Task<ChamadaResponse> MontarRespostaAsync(Guid turmaId, DateTime data, Guid? aulaId)
        {
            var matriculas = await _context.Matriculas.AsNoTracking()
                .Include(m => m.Aluno)
                .Where(m => m.TurmaId == turmaId && NaChamada.Contains(m.Status))
                .OrderBy(m => m.Aluno.Nome)
                .ToListAsync();

            var faltas = await FaltasPorMatriculaAsync(matriculas.Select(m => m.Id));

            var daAula = aulaId == null
                ? new Dictionary<Guid, Presenca>()
                : await _context.Presencas.AsNoTracking()
                    .Where(p => p.AulaId == aulaId)
                    .ToDictionaryAsync(p => p.MatriculaId);

            return new ChamadaResponse
            {
                AulaId = aulaId,
                TurmaId = turmaId,
                Data = data,
                Alunos = matriculas.Select(m =>
                {
                    var sit = RegrasAcademicas.Calcular(faltas.GetValueOrDefault(m.Id),
                        RegrasAcademicas.LimiteFaltas(m.CriadoEm), m.P1, m.P2, m.Trabalho);
                    daAula.TryGetValue(m.Id, out var p);
                    return new ChamadaAlunoResponse
                    {
                        MatriculaId = m.Id,
                        AlunoId = m.AlunoId,
                        AlunoNome = m.Aluno.Nome,
                        Presente = p?.Presente,
                        ComAtestado = p?.ComAtestado ?? false,
                        Faltas = sit.Faltas,
                        LimiteFaltas = sit.LimiteFaltas,
                        PercentualFaltas = sit.PercentualFaltas,
                        Situacao = sit.Situacao.ToString(),
                        EmRisco = sit.EmRisco
                    };
                }).ToList()
            };
        }
    }
}