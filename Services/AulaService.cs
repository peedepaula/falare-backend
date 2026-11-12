using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class AulaService
    {
        private readonly AppDbContext _context;

        public AulaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AulaResponse>> BuscarAulasService()
        {
            return await _context.Aulas
                .AsNoTracking()
                .OrderBy(a => a.Data)
                .Select(a => new AulaResponse
                {
                    Id = a.Id,
                    TurmaId = a.TurmaId,
                    Data = a.Data
                })
                .ToListAsync();
        }

        public async Task<(AulaResponse? Aula, string? Erro)> CadastrarAulaService(CadastrarAulaRequest request)
        {
            var data = RegrasAcademicas.DataDaAula(request.Data);

            // RN-06
            if (RegrasAcademicas.ParaDateOnly(data) > RegrasAcademicas.Hoje())
                return (null, "data_futura");

            var turmaExiste = await _context.Turmas.AnyAsync(t => t.Id == request.TurmaId);
            if (!turmaExiste)
                return (null, "turma_nao_encontrada");

            var jaExiste = await _context.Aulas.AnyAsync(a =>
                a.TurmaId == request.TurmaId && a.Data == data);
            if (jaExiste)
                return (null, "aula_duplicada");

            var aula = new Aula
            {
                TurmaId = request.TurmaId,
                Data = data
            };

            _context.Aulas.Add(aula);
            await _context.SaveChangesAsync();

            return (new AulaResponse
            {
                Id = aula.Id,
                TurmaId = aula.TurmaId,
                Data = aula.Data
            }, null);
        }

        public async Task<bool> DeletarAulaService(Guid id)
        {
            var aula = await _context.Aulas.FirstOrDefaultAsync(a => a.Id == id);
            if (aula == null)
                return false;

            aula.Apagado = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}