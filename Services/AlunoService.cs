using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
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

        public async Task<List<AlunoResponse>> BuscarAlunosService()
        {
            return await _context.Alunos
                .AsNoTracking()
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

        public async Task<AlunoResponse?> BuscarAlunoService(Guid id)
        {
            return await _context.Alunos
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AlunoResponse
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Email = a.Email,
                    Telefone = a.Telefone
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

        private static AlunoResponse ParaResponse(Aluno a) => new()
        {
            Id = a.Id,
            Nome = a.Nome,
            Email = a.Email,
            Telefone = a.Telefone
        };
    }
}