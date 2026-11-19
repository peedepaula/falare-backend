using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class TurmaService
    {
        private static readonly string[] DiasValidos = { "seg", "ter", "qua", "qui", "sex", "sab", "dom" };
        private readonly AppDbContext _context;

        public TurmaService(AppDbContext context)
        {
            _context = context;
        }

        private static bool DiasSaoValidos(string[] dias) =>
            dias.Length > 0 && dias.All(d => DiasValidos.Contains(d));

        // RN-05: professor não pode ter duas turmas no mesmo dia e horário
        private async Task<bool> ProfessorTemConflitoAsync(
            Guid professorId, string[] dias, TimeOnly inicio, TimeOnly fim, Guid? ignorarTurmaId)
        {
            var outras = await _context.Turmas.AsNoTracking()
                .Where(t => t.ProfessorId == professorId
                         && t.Id != ignorarTurmaId
                         && t.HorarioInicio < fim
                         && inicio < t.HorarioFim)
                .Select(t => t.Dias)
                .ToListAsync();
            return outras.Any(d => d.Intersect(dias).Any());
        }

        public async Task<List<TurmaResponse>> BuscarTurmasService(Guid? professorId = null)
        {
            var query = _context.Turmas.AsNoTracking().AsQueryable();

            if (professorId.HasValue)
                query = query.Where(t => t.ProfessorId == professorId.Value);

            return await query
                .AsNoTracking()
                .OrderBy(t => t.Idioma).ThenBy(t => t.Nivel)
                .Select(t => new TurmaResponse
                {
                    Id = t.Id,
                    Idioma = t.Idioma,
                    Nivel = t.Nivel,
                    Dias = t.Dias,
                    HorarioInicio = t.HorarioInicio,
                    HorarioFim = t.HorarioFim,
                    ProfessorId = t.ProfessorId,
                    ProfessorNome = t.Professor.Nome,
                    Matriculados = t.Matriculados,
                    Vagas = t.Vagas,
                    CriadoEm = t.CriadoEm
                })
                .ToListAsync();
        }

        public async Task<(TurmaResponse? Turma, string? Erro)> CadastrarTurmaService(CadastrarTurmasRequest request)
        {
            if (!DiasSaoValidos(request.Dias))
                return (null, "dias_invalidos");

            var professor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.ProfessorId && u.TipoUsuario == TipoUsuario.Professor);
            if (professor == null)
                return (null, "professor_nao_encontrado");

            var dias = request.Dias.Distinct().ToArray();
            if (await ProfessorTemConflitoAsync(professor.Id, dias, request.HorarioInicio, request.HorarioFim, null))
                return (null, "conflito_horario_professor");

            var turma = new Turma
            {
                Idioma = request.Idioma,
                Nivel = request.Nivel,
                Dias = dias,
                HorarioInicio = request.HorarioInicio,
                HorarioFim = request.HorarioFim,
                ProfessorId = professor.Id,
                Matriculados = 0,
                Vagas = Math.Min(request.Vagas, RegrasAcademicas.VagasPorTurma)
            };

            _context.Turmas.Add(turma);
            await _context.SaveChangesAsync();

            return (ParaResponse(turma, professor.Nome), null);
        }

        public async Task<(TurmaResponse? Turma, string? Erro)> AtualizarTurmaService(Guid id, AtualizarTurmasRequest request)
        {
            var turma = await _context.Turmas.Include(t => t.Professor).FirstOrDefaultAsync(t => t.Id == id);
            if (turma == null)
                return (null, "nao_encontrada");

            if (request.Dias != null)
            {
                if (!DiasSaoValidos(request.Dias))
                    return (null, "dias_invalidos");
                turma.Dias = request.Dias.Distinct().ToArray();
            }

            if (request.ProfessorId.HasValue)
            {
                var professor = await _context.Usuarios.FirstOrDefaultAsync(u =>
                    u.Id == request.ProfessorId.Value && u.TipoUsuario == TipoUsuario.Professor);
                if (professor == null)
                    return (null, "professor_nao_encontrado");
                turma.ProfessorId = professor.Id;
                turma.Professor = professor;
            }

            if (request.Vagas.HasValue)
            {
                if (request.Vagas.Value > RegrasAcademicas.VagasPorTurma)
                    return (null, "vagas_acima_do_limite");
                if (request.Vagas.Value < turma.Matriculados)
                    return (null, "vagas_menor_que_matriculados");
                turma.Vagas = request.Vagas.Value;
            }

            if (request.Idioma.HasValue) turma.Idioma = request.Idioma.Value;
            if (request.Nivel.HasValue) turma.Nivel = request.Nivel.Value;

            if (request.HorarioInicio.HasValue) turma.HorarioInicio = request.HorarioInicio.Value;
            if (request.HorarioFim.HasValue) turma.HorarioFim = request.HorarioFim.Value;

            if (turma.HorarioFim <= turma.HorarioInicio)
                return (null, "horario_invalido");

            if (await ProfessorTemConflitoAsync(turma.ProfessorId, turma.Dias, turma.HorarioInicio, turma.HorarioFim, turma.Id))
                return (null, "conflito_horario_professor");

            turma.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (ParaResponse(turma, turma.Professor.Nome), null);
        }

        public async Task<bool> DeletarTurmaService(Guid id)
        {
            var turma = await _context.Turmas.FirstOrDefaultAsync(t => t.Id == id);
            if (turma == null)
                return false;

            turma.Apagado = true;
            turma.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static TurmaResponse ParaResponse(Turma t, string professorNome) => new()
        {
            Id = t.Id,
            Idioma = t.Idioma,
            Nivel = t.Nivel,
            Dias = t.Dias,
            HorarioInicio = t.HorarioInicio,
            HorarioFim = t.HorarioFim,
            ProfessorId = t.ProfessorId,
            ProfessorNome = professorNome,
            Matriculados = t.Matriculados,
            Vagas = t.Vagas
        };
    }
}