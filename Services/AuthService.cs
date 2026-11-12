using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Models;
using FalareBackend.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FalareBackend.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
    public async Task<LoginResponse?> EntrarService(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        LoginUsuarioResponse dados;
        string? senhaHash;

        if (request.TipoUsuario == TipoLogin.Aluno)
        {
            var aluno = await _context.Alunos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == email);

            if (aluno == null) return null;

            senhaHash = aluno.SenhaHash;
            dados = new LoginUsuarioResponse
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Email = aluno.Email ?? string.Empty,
                TipoUsuario = "Aluno"
            };
        }
        else
        {
            // Converte TipoLogin (Professor/Secretaria) para o enum do banco
            var tipoBanco = request.TipoUsuario == TipoLogin.Professor
                ? TipoUsuario.Professor
                : TipoUsuario.Secretaria;

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.TipoUsuario == tipoBanco);

            if (usuario == null) return null;

            senhaHash = usuario.SenhaHash;
            dados = new LoginUsuarioResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TipoUsuario = usuario.TipoUsuario.ToString()
            };
        }

        if (string.IsNullOrEmpty(senhaHash) || !BCrypt.Net.BCrypt.Verify(request.Senha, senhaHash))
            return null;

        var (token, expiraEm) = GerarToken(dados);

        return new LoginResponse
        {
            Token = token,
            ExpiraEm = expiraEm,
            Usuario = dados
        };
    }

    private (string Token, DateTime ExpiraEm) GerarToken(LoginUsuarioResponse usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Name, usuario.Nome),
        new Claim(ClaimTypes.Email, usuario.Email),
        new Claim(ClaimTypes.Role, usuario.TipoUsuario)
    };

        var expiraEm = DateTime.UtcNow.AddHours(2);

        var jwt = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiraEm,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiraEm);
    }

    public async Task<LoginUsuarioResponse?> MeService(Guid id, string role)
    {
        if (role == "Aluno")
        {
            return await _context.Alunos
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new LoginUsuarioResponse
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Email = a.Email ?? string.Empty,
                    TipoUsuario = "Aluno"
                })
                .FirstOrDefaultAsync();
        }

        var u = await _context.Usuarios
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        return u == null ? null : new LoginUsuarioResponse
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            TipoUsuario = u.TipoUsuario.ToString()
        };
    }
}
