using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class AlunoService
    {
        private readonly AppDbContext _context;

        public AlunoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlunoResponse>> BuscarAlunosService(Guid? professorId = null)
        {
            var query = _context.Alunos.AsNoTracking().AsQueryable();

            if (professorId.HasValue)
                query = query.Where(a => a.Matriculas.Any(m => m.Turma.ProfessorId == professorId.Value));

            return await query
                .OrderBy(a => a.Nome)
                .Select(a => new AlunoResponse
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Email = a.Email,
                    Telefone = a.Telefone
                })
                .ToListAsync();
        }

        public async Task<List<AlunoResponse>> BuscarAlunosPorTurmaService(Guid turmaId, Guid? professorId = null)
        {
            var query = _context.Matriculas
                .AsNoTracking()
                .Where(m => m.TurmaId == turmaId && m.Status == StatusMatricula.Ativa);

            if (professorId.HasValue)
                query = query.Where(m => m.Turma.ProfessorId == professorId.Value);

            return await query
                .OrderBy(m => m.Aluno.Nome)
                .Select(m => new AlunoResponse
                {
                    Id = m.Aluno.Id,
                    Nome = m.Aluno.Nome,
                    Email = m.Aluno.Email,
                    Telefone = m.Aluno.Telefone
                })
                .ToListAsync();
        }

        public async Task<AlunoResponse?> BuscarAlunoService(Guid id, Guid? professorId = null)
        {
            var query = _context.Alunos.AsNoTracking().Where(a => a.Id == id);

            if (professorId.HasValue)
                query = query.Where(a => a.Matriculas.Any(m => m.Turma.ProfessorId == professorId.Value));

            return await query
                .Select(a => new AlunoResponse
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Email = a.Email,
                    Telefone = a.Telefone
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AlunoCompletoResponse?> BuscarAlunoCompletoService(Guid id, Guid? professorId = null)
        {
            var query = _context.Alunos.AsNoTracking().Where(a => a.Id == id);

            if (professorId.HasValue)
                query = query.Where(a => a.Matriculas.Any(m => m.Turma.ProfessorId == professorId.Value));

            return await query
                .Select(a => new AlunoCompletoResponse
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Email = a.Email,
                    Telefone = a.Telefone,
                    Matriculas = a.Matriculas
                        .Where(m => professorId == null || m.Turma.ProfessorId == professorId)
                        .OrderBy(m => m.CriadoEm)
                        .Select(m => new MatriculaAlunoResponse
                        {
                            MatriculaId = m.Id,
                            TurmaId = m.TurmaId,
                            Idioma = m.Turma.Idioma,
                            Nivel = m.Turma.Nivel,
                            Status = m.Status,
                            P1 = m.P1,
                            P2 = m.P2,
                            Trabalho = m.Trabalho,
                            Faltas = m.Presencas.Count(p => !p.Presente && !p.Aula.Apagado)
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(AlunoResponse? Aluno, string? Erro)> CadastrarAlunoService(CadastrarAlunoRequest request)
        {
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower();

            if (email != null)
            {
                var emUso = await _context.Alunos
                    .IgnoreQueryFilters()
                    .AnyAsync(a => a.Email == email);
                if (emUso)
                    return (null, "email_em_uso");
            }

            var aluno = new Aluno
            {
                Nome = request.Nome.Trim(),
                Email = email,
                Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim(),
                SenhaHash = string.IsNullOrWhiteSpace(request.Senha)
                    ? null
                    : BCrypt.Net.BCrypt.HashPassword(request.Senha)
            };

            _context.Alunos.Add(aluno);
            await _context.SaveChangesAsync();

            return (ParaResponse(aluno), null);
        }


        public async Task<(AlunoResponse? Aluno, string? Erro)> AtualizarAlunoService(Guid id, AtualizarAlunoRequest request)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id);
            if (aluno == null)
                return (null, "nao_encontrado");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email.Trim().ToLower();
                var emUso = await _context.Alunos
                    .IgnoreQueryFilters()
                    .AnyAsync(a => a.Email == email && a.Id != id);
                if (emUso)
                    return (null, "email_em_uso");
                aluno.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(request.Nome))
                aluno.Nome = request.Nome.Trim();

            if (request.Telefone != null)
                aluno.Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();

            if (!string.IsNullOrWhiteSpace(request.Senha))
                aluno.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            aluno.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (ParaResponse(aluno), null);
        }

        public async Task<bool> DeletarAlunoService(Guid id)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id);
            if (aluno == null)
                return false;

            aluno.Apagado = true;
            aluno.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FrequenciaAlunoResponse>> FrequenciaAlunoService(Guid alunoId)
        {
            var dados = await _context.Matriculas
                .AsNoTracking()
                .Where(m => m.AlunoId == alunoId)
                .OrderBy(m => m.CriadoEm)
                .Select(m => new
                {
                    m.Id,
                    m.CriadoEm,
                    m.P1,
                    m.P2,
                    m.Trabalho,
                    Idioma = m.Turma.Idioma,
                    Nivel = m.Turma.Nivel,
                    Faltas = m.Presencas.Count(p => !p.Presente && !p.Aula.Apagado)
                })
                .ToListAsync();

            return dados.Select(d =>
            {
                var limite = RegrasAcademicas.LimiteFaltas(d.CriadoEm);
                var sit = RegrasAcademicas.Calcular(d.Faltas, limite, d.P1, d.P2, d.Trabalho);
                return new FrequenciaAlunoResponse
                {
                    MatriculaId = d.Id,
                    Idioma = d.Idioma,
                    Nivel = d.Nivel,
                    Faltas = d.Faltas,
                    LimiteFaltas = limite,
                    Situacao = sit.Situacao
                };
            }).ToList();
        }

        public async Task<List<NotaAlunoResponse>> NotaAlunoService(Guid alunoId)
        {
            var dados = await _context.Matriculas
                .AsNoTracking()
                .Where(m => m.AlunoId == alunoId)
                .OrderBy(m => m.CriadoEm)
                .Select(m => new
                {
                    m.Id,
                    m.CriadoEm,
                    m.P1,
                    m.P2,
                    m.Trabalho,
                    Idioma = m.Turma.Idioma,
                    Nivel = m.Turma.Nivel,
                    Faltas = m.Presencas.Count(p => !p.Presente && !p.Aula.Apagado)
                })
                .ToListAsync();

            return dados.Select(d =>
            {
                var notas = new[] { d.P1, d.P2, d.Trabalho };
                decimal? media = notas.All(n => n.HasValue)
                    ? Math.Round(notas.Sum(n => n!.Value) / 3, 2)
                    : null;

                var sit = RegrasAcademicas.Calcular(
                    d.Faltas, RegrasAcademicas.LimiteFaltas(d.CriadoEm), d.P1, d.P2, d.Trabalho);

                return new NotaAlunoResponse
                {
                    MatriculaId = d.Id,
                    Idioma = d.Idioma,
                    Nivel = d.Nivel,
                    P1 = d.P1,
                    P2 = d.P2,
                    Trabalho = d.Trabalho,
                    Media = media,
                    Situacao = sit.Situacao
                };
            }).ToList();
        }

        public async Task<FinanceiroAlunoResponse> FinanceiroAlunoService(Guid alunoId)
        {
            var historico = await _context.Pagamentos
                .AsNoTracking()
                .Where(p => p.AlunoId == alunoId)
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => new PagamentoHistoricoResponse
                {
                    Id = p.Id,
                    Status = p.Status,
                    Valor = p.Valor,
                    CriadoEm = p.CriadoEm
                })
                .ToListAsync();

            var abertos = historico.Where(p => p.Status == StatusPagamento.Pendente).ToList();

            return new FinanceiroAlunoResponse
            {
                EmAberto = abertos.Count,
                ValorEmAberto = abertos.Sum(p => p.Valor),
                Historico = historico
            };
        }

        private static AlunoResponse ParaResponse(Aluno a) => new()
        {
            Id = a.Id,
            Nome = a.Nome,
            Email = a.Email,
            Telefone = a.Telefone
        };
    }
}