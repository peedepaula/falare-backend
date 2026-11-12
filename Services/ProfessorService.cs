using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FalareBackend.Services
{
    public class ProfessorService
    {
        private readonly AppDbContext _context;

        public ProfessorService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<UsuarioResponse>> BuscarProfessoresService()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.TipoUsuario == TipoUsuario.Professor && !u.Apagado)
                .OrderBy(u => u.Nome)
                .Select(u => new UsuarioResponse
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    TipoUsuario = u.TipoUsuario
                })
                .ToListAsync();
        }

        public async Task<UsuarioResponse?> CadastrarProfessorService(CadastrarProfessorRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var existe = await _context.Usuarios.AnyAsync(u => u.Email == email);
            if (existe)
                return null; // e-mail já cadastrado

            var professor = new Usuario
            {
                Nome = request.Nome.Trim(),
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                TipoUsuario = TipoUsuario.Professor
            };

            _context.Usuarios.Add(professor);
            await _context.SaveChangesAsync();

            return new UsuarioResponse
            {
                Id = professor.Id,
                Nome = professor.Nome,
                Email = professor.Email,
                TipoUsuario = professor.TipoUsuario
            };
        }

        public async Task<(UsuarioResponse? Professor, string? Erro)> AtualizarProfessorService(Guid id, AtualizarProfessorRequest request)
        {
            var professor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.TipoUsuario == TipoUsuario.Professor && !u.Apagado);

            if (professor == null)
                return (null, "nao_encontrado");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email.Trim().ToLower();

                var emUso = await _context.Usuarios.AnyAsync(u => u.Email == email && u.Id != id);
                if (emUso)
                    return (null, "email_em_uso");

                professor.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(request.Nome))
                professor.Nome = request.Nome.Trim();

            if (!string.IsNullOrWhiteSpace(request.Senha))
                professor.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            professor.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (new UsuarioResponse
            {
                Id = professor.Id,
                Nome = professor.Nome,
                Email = professor.Email,
                TipoUsuario = professor.TipoUsuario
            }, null);
        }

        public async Task<bool> DeletarProfessorService(Guid id)
        {
            var professor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.TipoUsuario == TipoUsuario.Professor && !u.Apagado);

            if (professor == null)
                return false;

            professor.Apagado = true;
            professor.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
