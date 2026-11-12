using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FalareBackend.Services;
using FalareBackend.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("entrar")]
        public async Task<IActionResult> Entrar([FromBody] LoginRequest request)
        {
            var resultado = await _authService.EntrarService(request);
            if (resultado == null)
            {
                return Unauthorized(new { mensagem = "Credenciais inválidas" });
            }
            return Ok(resultado);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;

            var usuario = await _authService.MeService(id, role);
            return usuario == null
                ? NotFound(new { mensagem = "Usuário não encontrado" })
                : Ok(usuario);
        }
    }
}
