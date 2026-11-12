using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class PagamentoService
    {
        private readonly AppDbContext _context;

        public PagamentoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PagamentoResponse>> BuscarPagamentosService(Guid? alunoId)
        {
            var query = _context.Pagamentos.AsNoTracking().AsQueryable();

            if (alunoId.HasValue)
                query = query.Where(p => p.AlunoId == alunoId.Value);

            return await query
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => new PagamentoResponse
                {
                    Id = p.Id,
                    AlunoId = p.AlunoId,
                    AlunoNome = p.Aluno.Nome,
                    Status = p.Status,
                    Valor = p.Valor,
                    CriadoEm = p.CriadoEm,
                    AtualizadoEm = p.AtualizadoEm
                })
                .ToListAsync();
        }

        public async Task<PagamentoResponse?> BuscarPagamentoService(Guid id)
        {
            return await _context.Pagamentos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PagamentoResponse
                {
                    Id = p.Id,
                    AlunoId = p.AlunoId,
                    AlunoNome = p.Aluno.Nome,
                    Status = p.Status,
                    Valor = p.Valor,
                    CriadoEm = p.CriadoEm,
                    AtualizadoEm = p.AtualizadoEm
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(PagamentoResponse? Pagamento, string? Erro)> CadastrarPagamentoService(CadastrarPagamentoRequest request)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == request.AlunoId);
            if (aluno == null)
                return (null, "aluno_nao_encontrado");

            var pagamento = new Pagamento
            {
                AlunoId = aluno.Id,
                Status = request.Status,
                Valor = request.Valor
            };

            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();

            return (ParaResponse(pagamento, aluno.Nome), null);
        }

        public async Task<PagamentoResponse?> AtualizarPagamentoService(Guid id, AtualizarPagamentoRequest request)
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Aluno)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pagamento == null)
                return null;

            if (request.Status.HasValue) pagamento.Status = request.Status.Value;
            if (request.Valor.HasValue) pagamento.Valor = request.Valor.Value;

            pagamento.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ParaResponse(pagamento, pagamento.Aluno.Nome);
        }

        public async Task<bool> DeletarPagamentoService(Guid id)
        {
            var pagamento = await _context.Pagamentos.FirstOrDefaultAsync(p => p.Id == id);
            if (pagamento == null)
                return false;

            _context.Pagamentos.Remove(pagamento);
            await _context.SaveChangesAsync();
            return true;
        }

        private static PagamentoResponse ParaResponse(Pagamento p, string alunoNome) => new()
        {
            Id = p.Id,
            AlunoId = p.AlunoId,
            AlunoNome = alunoNome,
            Status = p.Status,
            Valor = p.Valor,
            CriadoEm = p.CriadoEm,
            AtualizadoEm = p.AtualizadoEm
        };
    }
}