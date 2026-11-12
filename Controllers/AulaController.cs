using FalareBackend.DTOs;
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
            return Ok(await _aulaService.BuscarAulasService());
        }

        [Authorize(Roles = "Secretaria,Professor")]
        [HttpPost("aula")]
        public async Task<IActionResult> CadastrarAula([FromBody] CadastrarAulaRequest request)
        {
            var (aula, erro) = await _aulaService.CadastrarAulaService(request);

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
            var apagou = await _aulaService.DeletarAulaService(id);
            return apagou ? NoContent() : NotFound(new { mensagem = "Aula não encontrada" });
        }
    }
}