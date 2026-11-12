using FalareBackend.Data;
using FalareBackend.DTOs;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorController : ControllerBase
    {

        private readonly ProfessorService _professorService;

        public ProfessorController(ProfessorService professorService)
        {
            _professorService = professorService;
        }

        // Só para secretaria
        [Authorize(Roles = "Secretaria")]
        [HttpGet("professores")]
        public async Task<IActionResult> BuscarProfessores()
        {
            var professores = await _professorService.BuscarProfessoresService();
            return Ok(professores);
        }
        // Só para secretaria
        [Authorize(Roles = "Secretaria")]
        [HttpPost("professor")]
        public async Task<IActionResult> CadastrarProfessor([FromBody] CadastrarProfessorRequest request)
        {
            var professor = await _professorService.CadastrarProfessorService(request);

            if (professor == null)
                return Conflict(new { mensagem = "E-mail já cadastrado" });

            return StatusCode(201, professor);
        }
        // Só para secretaria
        [Authorize(Roles = "Secretaria")]
        [HttpPatch("professor")]
        public async Task<IActionResult> AtualizarProfessor(Guid id, [FromBody] AtualizarProfessorRequest request)
        {
            var (professor, erro) = await _professorService.AtualizarProfessorService(id, request);

            return erro switch
            {
                "nao_encontrado" => NotFound(new { mensagem = "Professor não encontrado" }),
                "email_em_uso" => Conflict(new { mensagem = "E-mail já cadastrado" }),
                _ => Ok(professor)
            };
        }
        [Authorize(Roles = "Secretaria")]
        [HttpDelete("professor")]
        public async Task<IActionResult> DeletarProfessor(Guid id)
        {
            var apagou = await _professorService.DeletarProfessorService(id);

            if (!apagou)
                return NotFound(new { mensagem = "Professor não encontrado" });

            return NoContent();
        }

    }
}
