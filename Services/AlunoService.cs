using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FalareBackend.Services
{
    public class AlunoService
    {
        private readonly AppDbContext _context;

        public AlunoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlunoResponse>> BuscarAlunosService(Guid? professorId = null, bool apenasEmRisco = false)
        {
            const decimal MediaMinima = 7.0m;
            const decimal LimiteFaltas = 15m;

            var query = _context.Alunos.AsNoTracking().AsQueryable();

            // Professor continua vendo só os alunos das suas turmas
            if (professorId.HasValue)
                query = query.Where(a => a.Matriculas.Any(m => m.Turma.ProfessorId == professorId.Value));

            var dados = await query
                .Select(a => new
                {
                    AlunoId = a.Id,
                    a.Nome,
                    a.Email,
                    a.Telefone,
                    Matriculas = a.Matriculas
                        .Where(m => m.Status == StatusMatricula.Ativa
                                    && (!professorId.HasValue || m.Turma.ProfessorId == professorId.Value))
                        .Select(m => new
                        {
                            MatriculaId = m.Id,
                            m.P1,
                            m.P2,
                            m.Trabalho,
                            TotalPresencas = m.Presencas.Count(),
                            Faltas = m.Presencas.Count(p => !p.Presente && !p.ComAtestado)
                        })
                        .ToList()
                })
                .ToListAsync();

            var resultado = new List<AlunoResponse>();

            foreach (var d in dados)
            {
                // Aluno sem matrícula ativa: entra na lista, mas nunca em risco
                if (d.Matriculas.Count == 0)
                {
                    resultado.Add(new AlunoResponse
                    {
                        Id = d.AlunoId,
                        Nome = d.Nome,
                        Email = d.Email,
                        Telefone = d.Telefone,
                        MatriculaId = null,
                        Media = null,
                        PercentualFaltas = 0m,
                        EmRisco = false
                    });
                    continue;
                }

                foreach (var m in d.Matriculas)
                {
                    var notas = new[] { m.P1, m.P2, m.Trabalho }
                        .Where(n => n.HasValue)
                        .Select(n => n!.Value)
                        .ToList();

                    decimal? media = notas.Count > 0 ? Math.Round(notas.Average(), 1) : null;

                    var percentualFaltas = m.TotalPresencas > 0
                        ? Math.Round(m.Faltas * 100m / m.TotalPresencas, 1)
                        : 0m;

                    resultado.Add(new AlunoResponse
                    {
                        Id = d.AlunoId,
                        Nome = d.Nome,
                        Email = d.Email,
                        Telefone = d.Telefone,
                        MatriculaId = m.MatriculaId,
                        Media = media,
                        PercentualFaltas = percentualFaltas,
                        EmRisco = percentualFaltas > LimiteFaltas
                                  || (media.HasValue && media < MediaMinima)
                    });
                }
            }

            if (apenasEmRisco)
                resultado = resultado.Where(a => a.EmRisco).ToList();

            return resultado.OrderBy(a => a.Nome).ToList();
        }

        public async Task<AlunoStatusInicioResponse> AlunoStatusInicioService(Guid alunoId)
        {
            const int limiteFaltas = 8;

            var matriculaId = await _context.Matriculas
                .Where(m => m.AlunoId == alunoId && !m.Apagado)
                .OrderByDescending(m => m.CriadoEm)
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync();

            var presencas = _context.Presencas
                .Where(p => p.MatriculaId == matriculaId);

            var total = await presencas.CountAsync();
            var faltas = await presencas.CountAsync(p => !p.Presente && !p.ComAtestado);

            var statusFalta = "Sem registros";

            if (total > 0)
            {
                statusFalta = faltas <= limiteFaltas ? "Boa" : "Atenção";
            }

            var concluiuCurso = await _context.Matriculas
                .AnyAsync(m => m.AlunoId == alunoId
                            && !m.Apagado
                            && (m.Status == StatusMatricula.Concluida
                             || m.Status == StatusMatricula.Certificada));

            return new AlunoStatusInicioResponse
            {
                StatusFalta = statusFalta,
                StatusCertificado = concluiuCurso ? "Liberado" : "Pendente"
            };
        }

        public async Task<StatusMatricula> AlternarTrancamentoMatriculaService(Guid alunoId)
        {
            var matricula = await _context.Matriculas
                .Where(m => m.AlunoId == alunoId && !m.Apagado)
                .OrderByDescending(m => m.CriadoEm)
                .FirstOrDefaultAsync();

            if (matricula is null)
                throw new KeyNotFoundException("Você não possui matrícula.");

            matricula.Status = matricula.Status switch
            {
                StatusMatricula.Ativa => StatusMatricula.Trancada,
                StatusMatricula.Trancada => StatusMatricula.Ativa,
                _ => throw new InvalidOperationException(
                         "Somente matrículas ativas ou trancadas podem ser alteradas.")
            };

            matricula.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return matricula.Status;
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
                    MatriculaId = m.Id,
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

        public async Task<(byte[] Pdf, string NomeArquivo)> EmitirCertificadoService(Guid alunoId)
        {
            // Pendência financeira bloqueia a emissão
            var temPendencia = await _context.Pagamentos
                .AnyAsync(p => p.AlunoId == alunoId && p.Status == StatusPagamento.Pendente);

            if (temPendencia)
                throw new InvalidOperationException("Existem pendências financeiras. Regularize para emitir o certificado.");

            // Matrícula concluída ou certificada (a mais recente)
            var dados = await _context.Matriculas
                .AsNoTracking()
                .Where(m => m.AlunoId == alunoId
                            && !m.Apagado
                            && (m.Status == StatusMatricula.Concluida || m.Status == StatusMatricula.Certificada))
                .OrderByDescending(m => m.AtualizadoEm)
                .Select(m => new
                {
                    AlunoNome = m.Aluno.Nome,
                    m.Turma.Idioma,
                    m.Turma.Nivel
                })
                .FirstOrDefaultAsync();

            if (dados is null)
                throw new InvalidOperationException("Certificado indisponível: o curso ainda não foi concluído.");

            var fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            var hoje = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, fuso);
            var dataTexto = hoje.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"));

            var pdf = Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);

                    page.Content()
                        .Border(3).BorderColor(Colors.Blue.Darken3)
                        .Padding(40)
                        .Column(col =>
                        {
                            col.Spacing(18);

                            col.Item().AlignCenter()
                                .Text("FALARE").FontSize(40).Bold().FontColor(Colors.Blue.Darken3);

                            col.Item().AlignCenter()
                                .Text("CERTIFICADO DE CONCLUSÃO").FontSize(22).SemiBold();

                            col.Item().PaddingTop(20).AlignCenter()
                                .Text("Certificamos que").FontSize(14);

                            col.Item().AlignCenter()
                                .Text(dados.AlunoNome).FontSize(32).Bold();

                            col.Item().AlignCenter()
                                .Text($"concluiu com êxito o curso de {dados.Idioma}, nível {dados.Nivel}.")
                                .FontSize(16);

                            col.Item().PaddingTop(30).AlignCenter()
                                .Text($"Emitido em {dataTexto}").FontSize(12);
                        });
                });
            }).GeneratePdf();

            return (pdf, $"certificado-falare-{dados.Idioma}-{dados.Nivel}.pdf");
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