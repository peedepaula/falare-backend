using FalareBackend.DTOs;
using FalareBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FalareBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurmaController : ControllerBase
    {
        private readonly TurmaService _turmaService;

        public TurmaController(TurmaService turmaService)
        {
            _turmaService = turmaService;
        }

        [Authorize]
        [HttpGet("turmas")]
        public async Task<IActionResult> BuscarTurmas()
        {
            return Ok(await _turmaService.BuscarTurmasService());
        }

        [Authorize(Roles = "Secretaria")]
        [HttpPost("turmas")]
        public async Task<IActionResult> CadastrarTurma([FromBody] CadastrarTurmasRequest request)
        {
            var (turma, erro) = await _turmaService.CadastrarTurmaService(request);

            return erro switch
            {
                "dias_invalidos" => BadRequest(new { mensagem = "Dias válidos: seg, ter, qua, qui, sex, sab, dom" }),
                "professor_nao_encontrado" => NotFound(new { mensagem = "Professor não encontrado" }),
                _ => StatusCode(201, turma)
            };
        }

        [Authorize(Roles = "Secretaria")]
        [HttpPatch("turmas/{id:guid}")]
        public async Task<IActionResult> AtualizarTurma(Guid id, [FromBody] AtualizarTurmasRequest request)
        {
            var (turma, erro) = await _turmaService.AtualizarTurmaService(id, request);

            return erro switch
            {
                "nao_encontrada" => NotFound(new { mensagem = "Turma não encontrada" }),
                "dias_invalidos" => BadRequest(new { mensagem = "Dias válidos: seg, ter, qua, qui, sex, sab, dom" }),
                "professor_nao_encontrado" => NotFound(new { mensagem = "Professor não encontrado" }),
                "vagas_menor_que_matriculados" => BadRequest(new { mensagem = "Vagas não pode ser menor que o número de matriculados" }),
                _ => Ok(turma)
            };
        }

        [Authorize(Roles = "Secretaria")]
        [HttpDelete("turmas/{id:guid}")]
        public async Task<IActionResult> DeletarTurma(Guid id)
        {
            var apagou = await _turmaService.DeletarTurmaService(id);
            return apagou ? NoContent() : NotFound(new { mensagem = "Turma não encontrada" });
        }
    }
}
