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
        public async Task<List<TurmaResponse>> BuscarTurmasService()
        {
            return await _context.Turmas
                .AsNoTracking()
                .OrderBy(t => t.Idioma).ThenBy(t => t.Nivel)
                .Select(t => new TurmaResponse
                {
                    Id = t.Id,
                    Idioma = t.Idioma,
                    Nivel = t.Nivel,
                    Dias = t.Dias,
                    Horario = t.Horario,
                    ProfessorId = t.ProfessorId,
                    ProfessorNome = t.Professor.Nome,
                    Matriculados = t.Matriculados,
                    Vagas = t.Vagas
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

            var turma = new Turma
            {
                Idioma = request.Idioma,
                Nivel = request.Nivel,
                Dias = request.Dias.Distinct().ToArray(),
                Horario = request.Horario,
                ProfessorId = professor.Id,
                Matriculados = 0,
                Vagas = request.Vagas
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
                if (request.Vagas.Value < turma.Matriculados)
                    return (null, "vagas_menor_que_matriculados");
                turma.Vagas = request.Vagas.Value;
            }

            if (request.Idioma.HasValue) turma.Idioma = request.Idioma.Value;
            if (request.Nivel.HasValue) turma.Nivel = request.Nivel.Value;
            if (request.Horario.HasValue) turma.Horario = request.Horario.Value;

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
            Horario = t.Horario,
            ProfessorId = t.ProfessorId,
            ProfessorNome = professorNome,
            Matriculados = t.Matriculados,
            Vagas = t.Vagas
        };
    }
}
