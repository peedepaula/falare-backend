using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class MatriculaService
    {
        private readonly AppDbContext _context;

        public MatriculaService(AppDbContext context)
        {
            _context = context;
        }

        // Ciclo de vida: só estas transições são permitidas
        private static readonly Dictionary<StatusMatricula, StatusMatricula[]> Transicoes = new()
        {
            [StatusMatricula.Ativa] = new[] { StatusMatricula.Concluida, StatusMatricula.ReprovadaNota,
                                              StatusMatricula.ReprovadaFalta, StatusMatricula.Trancada },
            [StatusMatricula.Concluida] = new[] { StatusMatricula.Certificada },
            [StatusMatricula.Certificada] = Array.Empty<StatusMatricula>(),
            [StatusMatricula.ReprovadaFalta] = Array.Empty<StatusMatricula>(), // não vira Concluída
            [StatusMatricula.ReprovadaNota] = Array.Empty<StatusMatricula>(),
            [StatusMatricula.Trancada] = Array.Empty<StatusMatricula>()        // não vira Certificada
        };

        // Aluno reprovado por falta ainda ocupa a vaga
        private static bool OcupaVaga(StatusMatricula s) =>
            s == StatusMatricula.Ativa || s == StatusMatricula.ReprovadaFalta;

        public async Task<List<MatriculaResponse>> BuscarMatriculasService()
        {
            return await _context.Matriculas
                .AsNoTracking()
                .OrderBy(m => m.Aluno.Nome)
                .Select(m => new MatriculaResponse
                {
                    Id = m.Id,
                    AlunoId = m.AlunoId,
                    AlunoNome = m.Aluno.Nome,
                    TurmaId = m.TurmaId,
                    Idioma = m.Turma.Idioma,
                    Nivel = m.Turma.Nivel,
                    Status = m.Status,
                    P1 = m.P1,
                    P2 = m.P2,
                    Trabalho = m.Trabalho
                })
                .ToListAsync();
        }

        public async Task<(MatriculaResponse? Matricula, string? Erro)> CadastrarMatriculaService(CadastrarMatriculaRequest request)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == request.AlunoId);
            if (aluno == null)
                return (null, "aluno_nao_encontrado");

            var turma = await _context.Turmas.FirstOrDefaultAsync(t => t.Id == request.TurmaId);
            if (turma == null)
                return (null, "turma_nao_encontrada");

            var jaMatriculado = await _context.Matriculas.AnyAsync(m =>
                m.AlunoId == request.AlunoId &&
                m.TurmaId == request.TurmaId &&
                m.Status == StatusMatricula.Ativa);
            if (jaMatriculado)
                return (null, "ja_matriculado");

            // RN-03: no máximo 12 (usa o maior entre o contador e a contagem real)
            var ocupadas = await _context.Matriculas.CountAsync(m => m.TurmaId == turma.Id
                && (m.Status == StatusMatricula.Ativa || m.Status == StatusMatricula.ReprovadaFalta));
            var limiteVagas = Math.Min(turma.Vagas, RegrasAcademicas.VagasPorTurma);
            if (Math.Max(turma.Matriculados, ocupadas) >= limiteVagas)
                return (null, "turma_cheia");

            // RN-07: 2 ou mais mensalidades em aberto não rematriculam
            if (await ContarEmAbertoAsync(aluno.Id) >= 2)
                return (null, "pendencia_financeira");

            // RN-04: mesmo horário e pelo menos um dia em comum
            var diasDasOutras = await _context.Matriculas
                .Where(m => m.AlunoId == aluno.Id
                         && m.Status == StatusMatricula.Ativa
                         && m.Turma.Horario == turma.Horario)
                .Select(m => m.Turma.Dias)
                .ToListAsync();
            if (diasDasOutras.Any(d => d.Intersect(turma.Dias).Any()))
                return (null, "conflito_horario");

            var matricula = new Matricula
            {
                AlunoId = aluno.Id,
                TurmaId = turma.Id,
                Status = StatusMatricula.Ativa,
                CriadoEm = DateTime.UtcNow,   // define o limite de faltas (8 ou 6)
                AtualizadoEm = DateTime.UtcNow,
                Aluno = aluno,
                Turma = turma
            };

            _context.Matriculas.Add(matricula);
            turma.Matriculados = Math.Max(turma.Matriculados, ocupadas) + 1;
            turma.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (ParaResponse(matricula), null);
        }

        public async Task<(MatriculaResponse? Matricula, string? Erro)> AtualizarMatriculaService(
            Guid id, AtualizarMatriculaRequest request, bool podeAlterarStatus)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Aluno)
                .Include(m => m.Turma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matricula == null)
                return (null, "nao_encontrada");

            // Nada muda depois de certificar (evita certificado cancelado por erro de nota/frequência)
            if (matricula.Status == StatusMatricula.Certificada)
                return (null, "matricula_certificada");

            if (request.P1.HasValue) matricula.P1 = request.P1.Value;
            if (request.P2.HasValue) matricula.P2 = request.P2.Value;
            if (request.Trabalho.HasValue) matricula.Trabalho = request.Trabalho.Value;

            if (request.Status.HasValue && request.Status.Value != matricula.Status)
            {
                if (!podeAlterarStatus)
                    return (null, "sem_permissao_status");

                var erro = await ValidarTransicaoAsync(matricula, request.Status.Value);
                if (erro != null)
                    return (null, erro);

                var ocupava = OcupaVaga(matricula.Status);
                var ocupara = OcupaVaga(request.Status.Value);
                if (ocupava && !ocupara)
                    matricula.Turma.Matriculados = Math.Max(0, matricula.Turma.Matriculados - 1);
                else if (!ocupava && ocupara)
                    matricula.Turma.Matriculados += 1;

                matricula.Status = request.Status.Value;
            }

            matricula.AtualizadoEm = DateTime.UtcNow;
            matricula.Turma.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (ParaResponse(matricula), null);
        }

        public async Task<bool> DeletarMatriculaService(Guid id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Turma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matricula == null)
                return false;

            if (OcupaVaga(matricula.Status))
                matricula.Turma.Matriculados = Math.Max(0, matricula.Turma.Matriculados - 1);

            matricula.Apagado = true;
            matricula.AtualizadoEm = DateTime.UtcNow;
            matricula.Turma.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ---------- regras auxiliares ----------

        private Task<int> ContarEmAbertoAsync(Guid alunoId) =>
            _context.Pagamentos.CountAsync(p =>
                p.AlunoId == alunoId && p.Status == StatusPagamento.Pendente);

        // RN-08: ComAtestado não é filtrado, falta com atestado conta
        private Task<int> ContarFaltasAsync(Guid matriculaId) =>
            _context.Presencas.CountAsync(p =>
                p.MatriculaId == matriculaId && !p.Presente && !p.Aula.Apagado);

        private async Task<string?> ValidarTransicaoAsync(Matricula m, StatusMatricula novo)
        {
            if (!Transicoes[m.Status].Contains(novo))
                return "transicao_invalida";

            var sit = RegrasAcademicas.Calcular(
                await ContarFaltasAsync(m.Id),
                RegrasAcademicas.LimiteFaltas(m.CriadoEm),
                m.P1, m.P2, m.Trabalho);

            switch (novo)
            {
                case StatusMatricula.Concluida when sit.Situacao != SituacaoAcademica.Aprovado:
                    return "nao_aprovado";
                case StatusMatricula.ReprovadaNota when sit.Situacao != SituacaoAcademica.ReprovadoPorNota:
                    return "transicao_invalida";
                case StatusMatricula.ReprovadaFalta when sit.Situacao != SituacaoAcademica.ReprovadoPorFalta:
                    return "transicao_invalida";
                case StatusMatricula.Trancada:
                    // sem entidade Semestre: "metade do semestre" = até 16 aulas lançadas na turma
                    var aulas = await _context.Aulas.CountAsync(a => a.TurmaId == m.TurmaId);
                    if (aulas > RegrasAcademicas.AulasPorSemestre / 2)
                        return "trancamento_fora_do_prazo";
                    break;
                case StatusMatricula.Certificada:   // RN-07
                    if (await ContarEmAbertoAsync(m.AlunoId) >= 2)
                        return "pendencia_financeira";
                    break;
            }
            return null;
        }

        private static MatriculaResponse ParaResponse(Matricula m) => new()
        {
            Id = m.Id,
            AlunoId = m.AlunoId,
            AlunoNome = m.Aluno.Nome,
            TurmaId = m.TurmaId,
            Idioma = m.Turma.Idioma,
            Nivel = m.Turma.Nivel,
            Status = m.Status,
            P1 = m.P1,
            P2 = m.P2,
            Trabalho = m.Trabalho
        };
    }
}