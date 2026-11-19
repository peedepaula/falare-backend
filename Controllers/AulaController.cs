using FalareBackend.DTOs;
using FalareBackend.Extensions;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AulaController : ControllerBase
    {
        private readonly AulaService _aulaService;

        public AulaController(AulaService aulaService)
        {
            _aulaService = aulaService;
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpGet("aulas")]
        public async Task<IActionResult> BuscarAulas()
        {
            Guid? professorId = User.EhProfessor() ? User.UsuarioId() : null;
            return Ok(await _aulaService.BuscarAulasService(professorId));
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpPost("aula")]
        public async Task<IActionResult> CadastrarAula([FromBody] CadastrarAulaRequest request)
        {
            Guid? professorId = User.EhProfessor() ? User.UsuarioId() : null;
            var (aula, erro) = await _aulaService.CadastrarAulaService(request, professorId);

            return erro switch
            {
                "data_futura" => BadRequest(new { mensagem = "Não é possível lançar aula com data futura" }),
                "turma_nao_encontrada" => NotFound(new { mensagem = "Turma não encontrada" }),
                "aula_duplicada" => Conflict(new { mensagem = "Já existe uma aula dessa turma nesta data" }),
                _ => StatusCode(201, aula)
            };
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpDelete("aula/{id:guid}")]
        public async Task<IActionResult> DeletarAula(Guid id)
        {
            Guid? professorId = User.EhProfessor() ? User.UsuarioId() : null;
            var apagou = await _aulaService.DeletarAulaService(id, professorId);
            return apagou ? NoContent() : NotFound(new { mensagem = "Aula não encontrada" });
        }
    }
}