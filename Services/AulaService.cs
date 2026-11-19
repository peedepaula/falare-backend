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

        public async Task<List<AulaResponse>> BuscarAulasService(Guid? professorId = null)
        {
            var query = _context.Aulas.AsNoTracking().AsQueryable();

            if (professorId.HasValue)
                query = query.Where(a => a.Turma.ProfessorId == professorId.Value);

            return await query
                .AsNoTracking()
                .OrderBy(a => a.Data)
                .Select(a => new AulaResponse
                {
                    Id = a.Id,
                    TurmaId = a.TurmaId,
                    Data = a.Data,
                    ProfessorNome = a.Turma.Professor.Nome,
                    Idioma = a.Turma.Idioma,
                    Nivel = a.Turma.Nivel,
                    HorarioInicio = a.Turma.HorarioInicio,
                    HorarioFim = a.Turma.HorarioFim
                })
                .ToListAsync();
        }

        public async Task<(AulaResponse? Aula, string? Erro)> CadastrarAulaService(CadastrarAulaRequest request, Guid? professorId = null)
        {
            var data = RegrasAcademicas.DataDaAula(request.Data);

            // RN-06
            if (RegrasAcademicas.ParaDateOnly(data) > RegrasAcademicas.Hoje())
                return (null, "data_futura");

            var turma = await _context.Turmas
                .AsNoTracking()
                .Include(t => t.Professor)
                .FirstOrDefaultAsync(t => t.Id == request.TurmaId
                                       && (professorId == null || t.ProfessorId == professorId));
            if (turma == null)
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
                Data = aula.Data,
                ProfessorNome = turma.Professor.Nome,
                Idioma = turma.Idioma,
                Nivel = turma.Nivel,
                HorarioInicio = turma.HorarioInicio,
                HorarioFim = turma.HorarioFim
            }, null);
        }

        public async Task<bool> DeletarAulaService(Guid id, Guid? professorId = null)
        {
            var aula = await _context.Aulas.FirstOrDefaultAsync(a =>
                a.Id == id && (professorId == null || a.Turma.ProfessorId == professorId));
            if (aula == null)
                return false;

            aula.Apagado = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}