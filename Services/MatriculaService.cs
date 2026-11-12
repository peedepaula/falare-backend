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

            if (turma.Matriculados >= turma.Vagas)
                return (null, "turma_cheia");

            var jaMatriculado = await _context.Matriculas.AnyAsync(m =>
                m.AlunoId == request.AlunoId &&
                m.TurmaId == request.TurmaId &&
                m.Status == StatusMatricula.Ativa);
            if (jaMatriculado)
                return (null, "ja_matriculado");

            var matricula = new Matricula
            {
                AlunoId = aluno.Id,
                TurmaId = turma.Id,
                Status = StatusMatricula.Ativa
            };

            _context.Matriculas.Add(matricula);
            turma.Matriculados += 1;
            turma.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (new MatriculaResponse
            {
                Id = matricula.Id,
                AlunoId = aluno.Id,
                AlunoNome = aluno.Nome,
                TurmaId = turma.Id,
                Idioma = turma.Idioma,
                Nivel = turma.Nivel,
                Status = matricula.Status
            }, null);
        }

        public async Task<(MatriculaResponse? Matricula, string? Erro)> AtualizarMatriculaService(Guid id, AtualizarMatriculaRequest request)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Aluno)
                .Include(m => m.Turma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matricula == null)
                return (null, "nao_encontrada");

            if (request.Status.HasValue && request.Status.Value != matricula.Status)
            {
                var eraAtiva = matricula.Status == StatusMatricula.Ativa;
                var seraAtiva = request.Status.Value == StatusMatricula.Ativa;

                // Libera a vaga ao sair de Ativa, ocupa ao voltar para Ativa
                if (eraAtiva && !seraAtiva)
                {
                    matricula.Turma.Matriculados = Math.Max(0, matricula.Turma.Matriculados - 1);
                }
                else if (!eraAtiva && seraAtiva)
                {
                    if (matricula.Turma.Matriculados >= matricula.Turma.Vagas)
                        return (null, "turma_cheia");
                    matricula.Turma.Matriculados += 1;
                }

                matricula.Status = request.Status.Value;
            }

            if (request.P1.HasValue) matricula.P1 = request.P1.Value;
            if (request.P2.HasValue) matricula.P2 = request.P2.Value;
            if (request.Trabalho.HasValue) matricula.Trabalho = request.Trabalho.Value;

            matricula.AtualizadoEm = DateTime.UtcNow;
            matricula.Turma.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (new MatriculaResponse
            {
                Id = matricula.Id,
                AlunoId = matricula.AlunoId,
                AlunoNome = matricula.Aluno.Nome,
                TurmaId = matricula.TurmaId,
                Idioma = matricula.Turma.Idioma,
                Nivel = matricula.Turma.Nivel,
                Status = matricula.Status,
                P1 = matricula.P1,
                P2 = matricula.P2,
                Trabalho = matricula.Trabalho
            }, null);
        }

        public async Task<bool> DeletarMatriculaService(Guid id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Turma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matricula == null)
                return false;

            if (matricula.Status == StatusMatricula.Ativa)
                matricula.Turma.Matriculados = Math.Max(0, matricula.Turma.Matriculados - 1);

            matricula.Apagado = true;
            matricula.AtualizadoEm = DateTime.UtcNow;
            matricula.Turma.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}